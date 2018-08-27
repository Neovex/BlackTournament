using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlackCoat;
using BlackTournament.Systems;
using BlackTournament.Net.Server;
using BlackTournament.Tmx;
using Lidgren.Network;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using SFML.System;

namespace BlackTournament.Net.Data
{
    class GameLogic
    {
        private Core _Core;
        private TmxMapper _Map;
        private Dictionary<Int32, ServerPlayer> _PlayerLookup;
        private List<ServerPlayer> _Players;
        private List<Shot> _Shots;
        private List<Effect> _Effects;

        public event Action<ServerUser<NetConnection>, PickupType> PlayerGotPickup = (u, p) => { };


        public string MapName { get { return _Map.Name; } }


        public GameLogic(Core core, TmxMapper map)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Map = map ?? throw new ArgumentNullException(nameof(map));
            _PlayerLookup = new Dictionary<int, ServerPlayer>();
            _Players = new List<ServerPlayer>();
            _Shots = new List<Shot>();
            _Effects = new List<Effect>();
        }


        public void AddPlayer(ServerUser<NetConnection> user)
        {
            var player = new ServerPlayer(user, _Core.CollisionSystem);
            player.ShotFired += HandlePlayerShoot;
            _PlayerLookup.Add(user.Id, player);
            _Players.Add(player);
        }

        public void RemovePlayer(ServerUser<NetConnection> user)
        {
            var player = _PlayerLookup[user.Id];
            player.ShotFired -= HandlePlayerShoot;
            _PlayerLookup.Remove(user.Id);
            _Players.Remove(player);
        }

        private void Spawn(ServerPlayer player)
        {
            var spawnPoint = _Players.All(p => p.Dead) ? _Map.SpawnPoints.First()
                : _Map.SpawnPoints.OrderByDescending(sp => _Players.Where(p => !p.Dead).Min(p => p.Position.DistanceBetweenSquared(sp))).First();

            player.Respawn(spawnPoint);
        }

        private void HandlePlayerShoot(ServerPlayer player, bool primaryFire)
        {
            var effect = new Effect(Net.GetNextId(), EffectType.Gunfire, player.WeaponSpawn, player.Rotation, player.CurrentWeaponType, primaryFire);

            var weaponData = primaryFire ? player.Weapon.PrimaryWeapon : player.Weapon.SecundaryWeapon;
            switch (weaponData.ProjectileGeometry)
            {
                case Geometry.Point:
                    // Spawn Shot
                    var s = new Shot(Net.GetNextId(), player.Rotation, weaponData.Speed, weaponData.Damage, weaponData.BlastRadius, weaponData.TTL, player.CurrentWeaponType, primaryFire, player.WeaponSpawn);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                case Geometry.Line: // Ray caster
                    // Check Intersections, Occlusion etc. +
                    // Perform data adjustments (remove health etc.) +
                    // Spawn impact effects
                    CheckRayWeaponIntersections(player, weaponData.Length, weaponData.Damage, primaryFire, effect);
                    break;
                case Geometry.Circle:
                    // Spawn Shot
                    var circ = new CircleCollisionShape(_Core.CollisionSystem, player.WeaponSpawn, weaponData.Length);
                    s = new Shot(Net.GetNextId(), player.Rotation, weaponData.Speed, weaponData.Damage, weaponData.BlastRadius, weaponData.TTL, player.CurrentWeaponType, primaryFire, player.WeaponSpawn, p => circ.Position = p, circ);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                //NA: case Geometry.Rectangle:
                //NA: case Geometry.Polygon:
                default:
                    Log.Error("Invalid weapon fire projectile geometry", weaponData.ProjectileGeometry);
                    break;
            }

            _Effects.Add(effect);
        }

        internal void ProcessGameAction(int id, GameAction action, Boolean activate)
        {
            var player = _PlayerLookup[id];
            switch (action)
            {
                case GameAction.MoveUp:
                case GameAction.MoveDown:
                case GameAction.MoveLeft:
                case GameAction.MoveRight:
                    if (activate) player.Input.Add(action);
                    else player.Input.Remove(action);
                    break;
                case GameAction.ShootPrimary:
                    if (player.Dead)
                    {
                        if (player.RespawnTimeout <= 0) Spawn(player);
                    }
                    else
                    {
                        player.ShootPrimary(activate);
                    }
                    break;
                case GameAction.ShootSecundary:
                    player.ShootSecundary(activate);
                    break;
                case GameAction.NextWeapon:
                    player.SwitchWeapon(1);
                    break;
                case GameAction.PreviousWeapon:
                    player.SwitchWeapon(-1);
                    break;
            }
        }

        public void RotatePlayer(int id, float rotation)
        {
            _PlayerLookup[id].Rotate(rotation);
        }

        public void Serialize(NetOutgoingMessage msg, bool forceFullUpdate)
        {
            // Players
            var dirtyPlayers = _Players.Where(p => p.IsDirty || forceFullUpdate).ToArray();
            msg.Write(dirtyPlayers.Length);
            foreach (var player in dirtyPlayers)
            {
                player.Serialize(msg);
            }

            // Pickups
            var dirtyPickups = _Map.Pickups.Where(p => p.IsDirty || forceFullUpdate).ToArray();
            msg.Write(dirtyPickups.Length);
            foreach (var pickup in dirtyPickups)
            {
                pickup.Serialize(msg);
            }

            // Shots
            msg.Write(_Shots.Count);
            foreach (var shot in _Shots)
            {
                shot.Serialize(msg);
            }
            _Shots.RemoveAll(s => !s.Alive);

            // Effects
            msg.Write(_Effects.Count);
            foreach (var effect in _Effects)
            {
                effect.Serialize(msg);
            }
            _Effects.Clear();
        }

        public void Update(float deltaT)
        {
            // Update Pickups
            foreach (var pickup in _Map.Pickups)
            {
                pickup.Update(deltaT);
            }

            // Handle projectiles
            foreach (var shot in _Shots)
            {
                shot.Update(deltaT);
                if (shot.Alive) CheckProjectileCollisions(shot, deltaT);
                else if (shot.IsExplosive) CheckExplosion(shot);
            }

            // Handle Player movement
            foreach (var player in _Players)
            {
                player.Update(deltaT);

                // Handle Collisions
                if (!player.Dead)
                {
                    foreach (var wall in _Map.WallCollider) // TODO : Walls -> Fix player glued to wall issue
                    {
                        if (player.Collision.Collide(wall))
                        {
                            player.Collision.Position = player.Position;
                        }
                    }
                    foreach (var otherPlayer in _Players) // Players
                    {
                        if (!otherPlayer.Dead && otherPlayer != player
                            && player.Collision.Collide(otherPlayer.Collision))
                        {
                            player.Collision.Position = player.Position;
                        }
                    }
                    foreach (var pickup in _Map.Pickups) // Pickups
                    {
                        if (pickup.Active && player.Collision.Collide(pickup.Collision))
                        {
                            pickup.Active = false;
                            player.GivePickup(pickup.Type, pickup.Amount);
                        }
                    }
                }
                player.Move(player.Collision.Position);
            }
        }

        private void CheckRayWeaponIntersections(ServerPlayer player, float length, float damage, bool primary, Effect effect)
        {
            // inaccuracy modification
            var rotation = player.Rotation;
            var weapon = primary ? player.Weapon.PrimaryWeapon : player.Weapon.SecundaryWeapon;
            if (weapon.Inaccuracy != 0)
            {
                rotation += _Core.Random.NextFloat(-weapon.Inaccuracy, weapon.Inaccuracy);
                rotation = MathHelper.ValidateAngle(rotation);
            }

            // find first wall intersection
            var wallIintersectionPoints = _Map.WallCollider.SelectMany(wall => _Core.CollisionSystem.Raycast(player.WeaponSpawn, rotation, wall)).Select(i=>i.Position) // ANGLE REMOVED
                                                                               .OrderBy(p => p.ToLocal(player.WeaponSpawn).LengthSquared()).ToArray();
            
            if (wallIintersectionPoints.Length != 0)
            {
                var impactlength = (float)wallIintersectionPoints[0].ToLocal(player.WeaponSpawn).Length();
                if (impactlength <= length)
                {
                    // walls occlude ray weapons hence update the length for player intersections
                    length = impactlength;
                    // add impact
                    _Effects.Add(new Effect(Net.GetNextId(), EffectType.WallImpact, wallIintersectionPoints[0], rotation, player.CurrentWeaponType, primary));
                }
            }

            // find all player intersections
            var affectedPlayers = _Players.Where(p => !p.Dead && p != player)
                                          .Select(p => new
                                          {
                                              Player = p,
                                              Intersections = _Core.CollisionSystem.Raycast(player.WeaponSpawn, rotation, p.Collision).Select(i => i.Position)
                                                              .OrderBy(ip => ip.ToLocal(player.WeaponSpawn).LengthSquared()).ToArray()
                                          })
                                          .Where(pi => pi.Intersections.Length != 0 && pi.Intersections[0].ToLocal(player.WeaponSpawn).Length() <= length);

            //update game
            foreach (var pi in affectedPlayers)
            {
                // add impacts
                _Effects.Add(new Effect(Net.GetNextId(), EffectType.WallImpact, pi.Intersections[0], rotation, player.CurrentWeaponType, primary));
                // damage player
                pi.Player.DamagePlayer(damage);
            }

            effect.Size = length;
            effect.Rotation = rotation;
        }

        private void CheckProjectileCollisions(Shot shot, float deltaT)
        {
            var wall = _Map.WallCollider.FirstOrDefault(w => shot.Collision == null ? w.Collide(shot.Position) : w.Collide(shot.Collision));
            if(wall != null)
            {
                if (shot.IsBouncy)
                {
                    // do bounce
                    var (position, angle) = _Core.CollisionSystem.Raycast(shot.LastPosition, shot.Direction, wall).First();
                    _Effects.Add(new Effect(Net.GetNextId(), EffectType.WallImpact, position, angle, shot.SourceWeapon, shot.Primary));
                    shot.Direction = MathHelper.CalculateReflectionAngle(shot.Direction, angle);
                }
                else
                {
                    // remove shot
                    shot.Destroy();

                    if (shot.IsExplosive)
                    {
                        // go boom
                        CheckExplosion(shot);
                    }
                    else
                    {
                        // add impact
                        _Effects.Add(new Effect(Net.GetNextId(), EffectType.WallImpact, shot.Position, shot.Direction, shot.SourceWeapon, shot.Primary));
                    }
                    return;
                }
            }

            var player = _Players.FirstOrDefault(p => !p.Dead && (shot.Collision == null ? p.Collision.Collide(shot.Position) : p.Collision.Collide(shot.Collision)));
            if(player != null)
            {
                if (shot.IsExplosive)
                {
                    // go boom
                    CheckExplosion(shot);
                    // remove shot
                    shot.Destroy();
                }
                else
                {
                    // add impact
                    _Effects.Add(new Effect(Net.GetNextId(), EffectType.PlayerImpact, shot.Position, shot.Direction, shot.SourceWeapon, shot.Primary));
                    if (shot.IsPenetrating)
                    {
                        // damage player
                        player.DamagePlayer(shot.Damage * deltaT); // TODO : check damage
                    }
                    else
                    {
                        // damage player
                        player.DamagePlayer(shot.Damage);
                        // remove shot
                        shot.Destroy();
                    }
                }
            }
        }

        private void CheckExplosion(Shot shot)
        {
            if (shot.Exploded) return;
            shot.Exploded = true;

            // Visualize
            _Effects.Add(new Effect(Net.GetNextId(), EffectType.Explosion, shot.Position, 0, shot.SourceWeapon, shot.Primary, shot.BlastRadius));

            // Distribute Damage
            foreach (var player in _Players)
            {
                if (!player.Dead)
                {
                    var distance = (float)player.Position.DistanceBetween(shot.Position);
                    if (distance <= shot.BlastRadius)
                    {
                        player.DamagePlayer(Math.Max(shot.Damage / 3, shot.Damage * (1 - (distance / shot.BlastRadius)))); // Todo : check damage falloff
                    }
                }
            }
        }
    }
}