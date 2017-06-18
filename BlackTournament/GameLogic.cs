using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlackCoat;
using BlackTournament.Systems;
using BlackTournament.Net.Data;
using BlackTournament.Net.Server;
using BlackTournament.Tmx;
using Lidgren.Network;

namespace BlackTournament
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
            var player = new ServerPlayer(user);
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
            var spawnPoint = _Players.All(p=>p.Dead) ? _Map.SpawnPoints.First()
                : _Map.SpawnPoints.OrderByDescending(sp => _Players.Where(p => !p.Dead).Min(p => p.Position.DistanceBetweenSquared(sp))).First();

            player.Respawn(spawnPoint);
        }

        private void HandlePlayerShoot(ServerPlayer player, bool primaryFire)
        {
            
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

        internal void RotatePlayer(int id, float rotation)
        {
            _PlayerLookup[id].Rotate(rotation);
        }

        internal void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(_PlayerLookup.Count);
            foreach (var player in _PlayerLookup.Values)
            {
                player.Serialize(msg);
            }
            // TODO : handle pickups oh and maps
        }

        internal void Update(float deltaT)
        {
            // FIXME
            foreach (var player in _PlayerLookup.Values)
            {
                player.Update(deltaT);
            }
        }
    }
}