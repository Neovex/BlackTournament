﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlackCoat;
using BlackTournament.Systems;
using BlackTournament.Net.Server;
using BlackTournament.Tmx;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    class GameLogic
    {
        private Core _Core;
        private TmxMapper _Map;
        private Dictionary<int, ServerPlayer> _PlayerLookup;
        private List<ServerPlayer> _Players;

        public event Action<ServerUser<NetConnection>, PickupType> PlayerGotPickup = (u, p) => { };


        public string MapName { get { return _Map.Name; } }


        public GameLogic(Core core, TmxMapper map)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Map = map ?? throw new ArgumentNullException(nameof(map));
            _PlayerLookup = new Dictionary<int, ServerPlayer>();
            _Players = new List<ServerPlayer>();
        }


        public void AddPlayer(ServerUser<NetConnection> user)
        {
            var player = new ServerPlayer(user, _Core.CollisionSystem);
            player.Spawn += HandlePlayerSpawn;
            player.ShotFired += HandlePlayerShoot;
            _PlayerLookup.Add(user.Id, player);
            _Players.Add(player);
        }

        public void RemovePlayer(ServerUser<NetConnection> user)
        {
            var player = _PlayerLookup[user.Id];
            player.Spawn -= HandlePlayerSpawn;
            player.ShotFired -= HandlePlayerShoot;
            _PlayerLookup.Remove(user.Id);
            _Players.Remove(player);
        }

        private void HandlePlayerSpawn(ServerPlayer player)
        {
            var spawnPoint = _Players.All(p => p.Dead) ? _Map.SpawnPoints.First()
                : _Map.SpawnPoints.OrderByDescending(sp => _Players.Where(p => !p.Dead).Min(p => p.Position.DistanceBetweenSquared(sp))).First();

            player.Respawn(spawnPoint);
        }

        private void HandlePlayerShoot(ServerPlayer player, bool primaryFire)
        {
            // TODO
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
                    player.ShootPrimary();
                    break;
                case GameAction.ShootSecundary:
                    player.ShootSecundary();
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

            // TODO : shots
        }

        public void Update(float deltaT)
        {
            // Update Entities
            foreach (var pickup in _Map.Pickups)
            {
                pickup.Update(deltaT);
            }

            foreach (var player in _Players)
            {
                player.Update(deltaT);

                // Handle Collisions
                if (!player.Dead)
                {
                    foreach (var wall in _Map.WallCollider)
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
                        if(pickup.Active && player.Collision.Collide(pickup.Collision))
                        {
                            pickup.Active = false;
                            player.GivePickup(pickup.Type, pickup.Amount);
                            //PlayerGotPickup.Invoke(player.User, pickup.Type); // nötig?!
                        }
                    }
                }

                // no collision found
                player.Move(player.Collision.Position);
            }


        }
    }
}