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
        private List<Impact> _Impacts;

        public event Action<ServerUser<NetConnection>, PickupType> PlayerGotPickup = (u, p) => { };


        public string MapName { get { return _Map.Name; } }


        public GameLogic(Core core, TmxMapper map)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Map = map ?? throw new ArgumentNullException(nameof(map));
            _PlayerLookup = new Dictionary<int, ServerPlayer>();
            _Players = new List<ServerPlayer>();
            _Shots = new List<Shot>();
            _Impacts = new List<Impact>();
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
            var data = primaryFire ? player.Weapon.PrimaryWeapon : player.Weapon.SecundaryWeapon;
            switch (data.ProjectileGeometry)
            {
                case Geometry.Point:
                    // Spawn Shot
                    var s = new Shot(Net.GetNextId(), player.Rotation, data.Speed, data.Damage, data.TTL, player.CurrentWeapon, primaryFire, player.Position);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                case Geometry.Line: // Ray caster
                    // Check Intersections, Occlusion etc...
                    CheckRayWeaponIntersections(player, data.Length, data.Damage, primaryFire);
                    // Perform data adjustments (remove health etc...)
                    // Spawn effects
                    break;
                case Geometry.Circle:
                    // Spawn Shot
                    var circ = new CircleCollisionShape(_Core.CollisionSystem, player.Position, data.Length);
                    s = new Shot(Net.GetNextId(), player.Rotation, data.Speed, data.Damage, data.TTL, player.CurrentWeapon, primaryFire, player.Position, p => circ.Position = p, circ);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                case Geometry.Rectangle:
                    // Spawn Shot
                    var rect = new RectangleCollisionShape(_Core.CollisionSystem, player.Position, new Vector2f(data.Length, data.Length)); // problem? non cubic collision rectangles
                    s = new Shot(Net.GetNextId(), player.Rotation, data.Speed, data.Damage, data.TTL, player.CurrentWeapon, primaryFire, player.Position, p => rect.Position = p, rect);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                //NA: case Geometry.Polygon:
                default:
                    Log.Error("Invalid weapon fire", data.ProjectileGeometry);
                    break;
            }
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
                case GameAction.ShootPrimary: // todo improve weapon simulation (fix doubletab issue)
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

            // Impacts
            msg.Write(_Impacts.Count);
            foreach (var impact in _Impacts)
            {
                impact.Serialize(msg);
            }
            _Impacts.Clear();
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
                if (shot.Alive) CheckProjectileCollisions(shot);
            }

            // Handle Player movement
            foreach (var player in _Players)
            {
                player.Update(deltaT);

                // Handle Collisions
                if (!player.Dead)
                {
                    foreach (var wall in _Map.WallCollider) // Fix player stuck in wall issue
                    {
                        if (player.Collision.Collide(wall))
                        {
                            player.Collision.Position = player.Position;
                        }
                    }
                    foreach (var otherPlayer in _Players)
                    {
                        if (!otherPlayer.Dead
                            && otherPlayer != player
                            && player.Collision.Collide(otherPlayer.Collision))
                        {
                            player.Collision.Position = player.Position;
                        }
                    }
                    foreach (var pickup in _Map.Pickups)
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

        private float CheckRayWeaponIntersections(ServerPlayer player, float length, float damage, bool primary)
        {
            // find first wall intersection
            var wallIintersectionPoints = _Map.WallCollider.SelectMany(wall => _Core.CollisionSystem.Intersections(player.Position, player.Rotation, wall)).OrderBy(p => p.ToLocal(player.Position).LengthSquared()).ToArray();

            if (wallIintersectionPoints.Length != 0)
            {
                var impactlength = (float)wallIintersectionPoints[0].ToLocal(player.Position).Length();
                if (impactlength <= length)
                {
                    // walls occlude ray weapons hence update the length for player intersections
                    length = impactlength;
                    // add impact
                    _Impacts.Add(new Impact(Net.GetNextId(), wallIintersectionPoints[0], player.CurrentWeapon, primary));
                }
            }

            // find all player intersections
            var affectedPlayers = _Players.Where(p => !p.Dead && p != player)
                                          .Select(p => new
                                          {
                                              Player = p,
                                              Intersections = _Core.CollisionSystem.Intersections(player.Position, player.Rotation, p.Collision)
                                                              .OrderBy(i => i.ToLocal(player.Position).LengthSquared()).ToArray()
                                          })
                                          .Where(pi => pi.Intersections.Length != 0 && pi.Intersections[0].ToLocal(player.Position).Length() <= length);

            //update game
            foreach (var pi in affectedPlayers)
            {
                    // add impacts
                    _Impacts.Add(new Impact(Net.GetNextId(), pi.Intersections[0], player.CurrentWeapon, primary));
                    // damage player
                    pi.Player.DamagePlayer(damage);
            }

            return length;
        }

        private void CheckProjectileCollisions(Shot shot) // fix missing explosion damage
        {
            foreach (var wall in _Map.WallCollider.Where(w => shot.Collision.Collide(w)))
            {
                // remove shot
                shot.Destroy();
                // add impact
                _Impacts.Add(new Impact(Net.GetNextId(), shot.Position, shot.SourceWeapon, shot.Primary));
                return;
            }

            foreach (var player in _Players.Where(p => !p.Dead && shot.Collision.Collide(p.Collision) /*&& p != shot.sourcePlayer*/))//Fixme
            {
                // remove shot
                shot.Destroy();
                // add first impact
                _Impacts.Add(new Impact(Net.GetNextId(), shot.Position, shot.SourceWeapon, shot.Primary));
                // damage player
                player.DamagePlayer(shot.Damage);
                return;
            }
        }
    }
}