using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlackCoat;
using BlackTournament.Systems;
using BlackTournament.Net.Data;
using BlackTournament.Net.Server;
using Lidgren.Network;


namespace BlackTournament
{
    class GameLogic
    {
        private Core _Core;
        private Dictionary<int, Player> _Players;


        public string MapName { get; private set; }


        public GameLogic(Core core, String map)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;

            if (String.IsNullOrEmpty(map)) throw new ArgumentException(nameof(map));
            MapName = map;

            _Players = new Dictionary<int, Player>();
        }


        internal void AddPlayer(ServerUser<NetConnection> player)
        {
            _Players.Add(player.Id, new Player(player.Id));
        }

        internal void RemovePlayer(ServerUser<NetConnection> player)
        {
            _Players.Remove(player.Id);
        }

        internal void ProcessGameAction(int id, GameAction action, Boolean activate)
        {
            var player = _Players[id];
            switch (action)
            {
                case GameAction.MoveUp:
                    break;
                case GameAction.MoveDown:
                    break;
                case GameAction.MoveLeft:
                    break;
                case GameAction.MoveRight:
                    break;
                case GameAction.ShootPrimary:
                    break;
                case GameAction.ShootSecundary:
                    break;
                case GameAction.NextWeapon:
                    HandleWeaponSwitch(player, 1);
                    break;
                case GameAction.PreviousWeapon:
                    HandleWeaponSwitch(player, -1);
                    break;
            }
        }

        private void HandleWeaponSwitch(Player player, int direction)
        {
            var index = player.OwnedWeapons.IndexOf(player.CurrentWeapon) + direction;
            if (index == -1) index = player.OwnedWeapons.Count - 1;
            else if (index == player.OwnedWeapons.Count) index = 0;
            player.CurrentWeapon = player.OwnedWeapons[index];
        }

        internal void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(_Players.Count);
            foreach (var player in _Players.Values)
            {
                player.Serialize(msg);
            }
            // TODO : hanlde pickups (oh and maps)
        }

        internal void Update(float deltaT)
        {
            throw new NotImplementedException();
        }
    }
}