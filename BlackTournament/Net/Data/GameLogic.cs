using System;
using System.Collections.Generic;
using System.Linq;

using SFML.System;
using Lidgren.Network;

using BlackCoat;
using BlackCoat.Network;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;

using BlackTournament.InputMaps;
using BlackTournament.Tmx;


namespace BlackTournament.Net.Data
{
    class GameLogic : BlackCoatBase
    {
        private float _GameTime;
        private TmxMapper _Map;
        private Dictionary<int, ServerPlayer> _PlayerLookup;
        private List<ServerPlayer> _Players;
        private List<Shot> _Shots;
        private List<Effect> _Effects;
        private IEnumerable<Pickup> _Pickups;


        public MatchCicle MatchCicle { get; private set; }
        public string MapName => _Map.Name;
        public int CurrentPlayers => _Players.Count;


        public event Action<string> GameMessage = m => { };


        public GameLogic(Core core, TmxMapper map) : base(core)
        {
            MatchCicle = MatchCicle.None;
            _Map = map ?? throw new ArgumentNullException(nameof(map));
            _PlayerLookup = new Dictionary<int, ServerPlayer>();
            _Players = new List<ServerPlayer>();
            _Shots = new List<Shot>();
            _Effects = new List<Effect>();
            _Pickups = _Map.Pickups.Select(p => new Pickup(NetIdProvider.NEXT_ID, p.Position, p.Item, p.Amount, p.RespawnTime, _Core.CollisionSystem)).ToArray();
        }


        public void AddPlayer(ServerUser<NetConnection> user)
        {
            var player = new ServerPlayer(user, new CircleCollisionShape(_Core.CollisionSystem, new Vector2f(), 0));
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
            var spawnPoint = _Players.All(p => !p.IsAlive) ? _Map.SpawnPoints.OrderBy(p=> _Core.Random.Next()).First()
                : _Map.SpawnPoints.OrderByDescending(sp => _Players.Where(p => p.IsAlive).Min(p => p.Position.DistanceBetweenSquared(sp))).First();

            player.Respawn(spawnPoint);
        }

        private void HandlePlayerShoot(ServerPlayer player, bool primaryFire)
        {
            var effect = new Effect(NetIdProvider.NEXT_ID, EffectType.Gunfire, player.WeaponSpawn, player.Rotation, player.CurrentWeaponType, primaryFire);

            var weaponData = WeaponData.Get(player.CurrentWeaponType, primaryFire);
            switch (weaponData.ProjectileGeometry)
            {
                case Geometry.Point:
                    // Spawn Shot
                    var s = new Shot(NetIdProvider.NEXT_ID, 
                                    player.WeaponSpawn, player.Rotation, player.CurrentWeaponType, primaryFire, player.Id,
                                    weaponData.Speed, weaponData.Damage, weaponData.BlastRadius, weaponData.TTL);
                    // Check Collisions via Update Cycle
                    _Shots.Add(s);
                    break;
                case Geometry.Line: // Ray caster - instant effect
                    // Check Intersections, Occlusion etc. +
                    // Perform data adjustments (remove health etc.) +
                    // Spawn impact effects
                    CheckRayWeaponIntersections(player, primaryFire, effect);
                    break;
                case Geometry.Circle:
                    // Spawn Shot
                    var circ = new CircleCollisionShape(_Core.CollisionSystem, player.WeaponSpawn, weaponData.Length);
                    s = new Shot(NetIdProvider.NEXT_ID,
                                 player.WeaponSpawn, player.Rotation, player.CurrentWeaponType, primaryFire, player.Id,
                                 weaponData.Speed, weaponData.Damage, weaponData.BlastRadius, weaponData.TTL,
                                 p => circ.Position = p, circ);
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
                    if (player.IsAlive) player.ShootPrimary(activate);
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
            // Time
            msg.Write(_GameTime + 1);

            // Players
            var dirtyPlayers = _Players.Where(p => p.IsDirty || forceFullUpdate).ToArray();
            msg.Write(dirtyPlayers.Length);
            foreach (var player in dirtyPlayers)
            {
                player.Serialize(msg);
            }

            // Pickups
            var dirtyPickups = _Pickups.Where(p => p.IsDirty || forceFullUpdate).ToArray();
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
            UpdateTimer(deltaT);

            // Update Pickups
            foreach (var pickup in _Pickups)
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

            // Handle Players
            foreach (var player in _Players)
            {
                player.Update(deltaT);


                if (player.IsAlive) // Handle Collisions
                {
                    foreach (var wall in _Map.WallCollider) // Walls
                    {
                        if (player.Collision.CollidesWith(wall))
                        {
                            var movement = player.Collision.Position.ToLocal(player.Position);
                            var angle = movement.Angle();
                            var length = (float)movement.Length();
                            for (int i = -50; i < 50; i += 2)
                            {
                                player.Collision.Position = player.Position + Create.Vector2fFromAngleLookup(MathHelper.ValidateAngle(angle + i), length);
                                if (player.Collision.CollidesWith(wall))
                                {
                                    player.Collision.Position = player.Position;
                                }
                                else break;
                            }
                        }
                    }
                    foreach (var otherPlayer in _Players) // Players
                    {
                        if (otherPlayer.IsAlive && otherPlayer != player
                            && player.Collision.CollidesWith(otherPlayer.Collision))
                        {
                            player.Collision.Position = player.Position;
                        }
                    }
                    foreach (var pickup in _Pickups) // Pickups
                    {
                        if (pickup.Active && player.Collision.CollidesWith(pickup.Collision))
                        {
                            pickup.Active = false;
                            player.GivePickup(pickup.Type, pickup.Amount);
                            _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.Pickup, pickup.Position, 0, pickup.Type, false));
                        }
                    }
                    foreach (var killzone in _Map.Killzones) // Killzones
                    {
                        if (killzone.CollisionShape.CollidesWith(player.Position))
                        {
                            player.DamagePlayer(killzone.Damage * deltaT);
                            if (!player.IsAlive)
                            {
                                _Effects.Add(new Effect(NetIdProvider.NEXT_ID, killzone.Effect, player.Position));
                                switch (killzone.Effect)
                                {
                                    case EffectType.PlayerDrop:
                                        GameMessage.Invoke($"{player.User.Alias} went into the abyss.");
                                        break;
                                    case EffectType.Gore:
                                        GameMessage.Invoke($"{player.User.Alias} got shredded.");
                                        break;
                                    default:
                                        GameMessage.Invoke($"{player.User.Alias} was in the wrong place.");
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (MatchCicle == MatchCicle.Game && player.RespawnTimeout <= 0)
                {
                    Spawn(player);
                }
                player.Move(player.Collision.Position);
            }
        }

        private void UpdateTimer(float deltaT)
        {
            _GameTime -= deltaT;
            if (_GameTime <= 0)
            {
                switch (MatchCicle)
                {
                    case MatchCicle.None:
                        MatchCicle = MatchCicle.Warmup;
                        GameMessage.Invoke("Get ready!");
                        break;
                    case MatchCicle.Warmup:
                        MatchCicle = MatchCicle.Game;
                        _Players.ForEach(Spawn);
                        GameMessage.Invoke("FIGHT!");
                        break;
                    case MatchCicle.Game:
                        MatchCicle = MatchCicle.Cooldown;
                        _Players.ForEach(p => p.Fragg());
                        GameMessage.Invoke($"Match Complete. The winner is: {_Players.OrderBy(p => p.Score).First().User.Alias}");
                        break;
                    case MatchCicle.Cooldown:
                        MatchCicle = MatchCicle.Complete;
                        break;
                }
                _GameTime = (float)MatchCicle;
            }
        }

        private void CheckRayWeaponIntersections(ServerPlayer player, bool primary, Effect effect)
        {
            // inaccuracy modification
            var rotation = player.Rotation;
            var weaponData = WeaponData.Get(player.CurrentWeaponType, primary);
            if (weaponData.Inaccuracy != 0)
            {
                rotation += _Core.Random.NextFloat(-weaponData.Inaccuracy, weaponData.Inaccuracy);
                rotation = MathHelper.ValidateAngle(rotation);
            }

            // find wall intersections
            var wallIintersectionPoints = _Map.WallCollider.SelectMany(wall => _Core.CollisionSystem.Raycast(player.WeaponSpawn, rotation, wall))
                                                                               .Select(i => i.Position)
                                                                               .OrderBy(p => p.ToLocal(player.WeaponSpawn).LengthSquared())
                                                                               .ToArray();

            var length = weaponData.Length;
            if (wallIintersectionPoints.Length != 0)
            {
                if (!primary && player.CurrentWeaponType == PickupType.Titandrill) // titan drills secondary fire penetrates walls!
                {
                    foreach (var wallIntersection in wallIintersectionPoints)
                    {
                        // add impact
                        _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.WallImpact, wallIntersection, rotation, player.CurrentWeaponType, primary));
                    }
                }
                else
                {
                    var impactlength = (float)wallIintersectionPoints[0].ToLocal(player.WeaponSpawn).Length();
                    if (impactlength <= length)
                    {
                        // walls occlude ray weapons hence update the length for player intersections
                        length = impactlength;
                        // add impact
                        _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.WallImpact, wallIintersectionPoints[0], rotation, player.CurrentWeaponType, primary));
                    }
                }
            }

            // find all player intersections
            var affectedPlayers = _Players.Where(p => p.IsAlive && p != player)
                                          .Select(p => new
                                          {
                                              Player = p,
                                              Intersections = _Core.CollisionSystem.Raycast(player.WeaponSpawn, rotation, p.Collision).Select(i => i.Position)
                                                              .OrderBy(ip => ip.ToLocal(player.WeaponSpawn).LengthSquared()).ToArray()
                                          })
                                          .Where(pi => pi.Intersections.Length != 0 && pi.Intersections[0].ToLocal(player.WeaponSpawn).Length() <= length);

            //update players
            foreach (var pi in affectedPlayers)
            {
                // add impacts
                _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.PlayerImpact, pi.Intersections[0], rotation, player.CurrentWeaponType, primary));
                // damage player
                pi.Player.DamagePlayer(weaponData.Damage);
                if (!pi.Player.IsAlive)
                {
                    GameMessage.Invoke($"{pi.Player.User.Alias} was on the wrong end of {player.User.Alias}'s gun.");
                }
            }

            // update weapon effect
            effect.Size = length;
            effect.Rotation = rotation;
        }

        private void CheckProjectileCollisions(Shot shot, float deltaT)
        {
            var wall = _Map.WallCollider.FirstOrDefault(w => shot.Collision == null ? w.CollidesWith(shot.Position) : w.CollidesWith(shot.Collision));
            if(wall != null)
            {
                if (shot.IsBouncy) // do bounce
                {
                    //Move Projectile backwards out of the collision
                    shot.Direction += 180;
                    do
                    {
                        shot.Update(deltaT + 0.001f);
                    } while (shot.Collision == null ? wall.CollidesWith(shot.Position) : wall.CollidesWith(shot.Collision));
                    shot.Direction -= 180;
                    // Find collision surface angle
                    var intersects = _Core.CollisionSystem.Raycast(shot.Position, shot.Direction, wall);
                    if (intersects.Length == 0)
                    {
                        //only happens when a collision shape grazes a wall
                        var hitscan = new List<(Vector2f position, float angle)>();
                        var direction = shot.Direction - 90;
                        for (int i = 0; i <= 180; i++)
                        {
                            hitscan.AddRange(_Core.CollisionSystem.Raycast(shot.Position, MathHelper.ValidateAngle(direction + i), wall));
                        }
                        if (hitscan.Count == 0) return; // should never happen - hopefully
                        intersects = hitscan.OrderBy(i => shot.Position.DistanceBetweenSquared(i.position)).ToArray();
                        if (shot.Collision.CollisionGeometry == Geometry.Circle) intersects[0].Angle = MathHelper.ValidateAngle(shot.Position.AngleTowards(intersects[0].Position) + 90);
                    }
                    var (position, angle) = intersects[0];
                    //_Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.Environment, position, angle, shot.SourceWeapon, shot.Primary));
                    shot.Direction = MathHelper.CalculateReflectionAngle(shot.Direction, angle);
                    _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.WallImpact, position, shot.Direction, shot.SourceWeapon, shot.Primary));
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
                        _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.WallImpact, shot.Position, shot.Direction, shot.SourceWeapon, shot.Primary));
                    }
                    return;
                }
            }

            var player = _Players.FirstOrDefault(p => p.IsAlive && (shot.Collision == null ? p.Collision.CollidesWith(shot.Position) : p.Collision.CollidesWith(shot.Collision)));
            if(player != null)
            {
                if (shot.IsExplosive)
                {
                    // go boom!
                    CheckExplosion(shot);
                    // then remove shot
                    shot.Destroy();
                }
                else
                {
                    // add impact
                    _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.PlayerImpact, player.Position, shot.Direction, shot.SourceWeapon, shot.Primary));
                    if (shot.IsPenetrating)
                    {
                        // damage player
                        player.DamagePlayer(shot.Damage * deltaT);
                    }
                    else
                    {
                        // damage player
                        player.DamagePlayer(shot.Damage);
                        // remove shot
                        shot.Destroy();
                    }
                    if (!player.IsAlive)
                    {
                        if (player.Id == shot.Owner)
                            GameMessage.Invoke($"{player.User.Alias} committed sudoku");
                        else
                            GameMessage.Invoke($"{player.User.Alias} was fragged by {_PlayerLookup[shot.Owner].User.Alias}");
                    }
                }
            }
        }

        private void CheckExplosion(Shot shot)
        {
            if (shot.Exploded) return;
            shot.Exploded = true;

            // Visualize
            _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.Explosion, shot.Position, 0, shot.SourceWeapon, shot.Primary, shot.BlastRadius));

            // Distribute Damage
            foreach (var player in _Players)
            {
                if (player.IsAlive)
                {
                    var distance = (float)player.Position.DistanceBetween(shot.Position);
                    if (distance <= shot.BlastRadius)
                    {
                        player.DamagePlayer(Math.Max(shot.Damage / 4, shot.Damage * (1 - (distance / shot.BlastRadius))));
                        // Add impact
                        _Effects.Add(new Effect(NetIdProvider.NEXT_ID, EffectType.PlayerImpact, player.Position, shot.Position.AngleTowards(player.Position), shot.SourceWeapon, shot.Primary));

                        if (!player.IsAlive)
                        {
                            if (player.Id == shot.Owner)
                                GameMessage.Invoke($"{player.User.Alias} checked his explosives too thoroughly");
                            else
                                GameMessage.Invoke($"{player.User.Alias} was evenly distributed by {_PlayerLookup[shot.Owner].User.Alias}");
                        }
                    }
                }
            }
        }
    }

    enum MatchCicle
    {
        None = 0,
        Warmup = 10,
        Game = 600,
        Cooldown = 30,
        Complete = -1
    }
}