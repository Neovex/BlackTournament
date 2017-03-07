﻿using System;
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
        private Dictionary<int, ServerPlayer> _Players;


        public string MapName { get; private set; }


        public GameLogic(Core core, String map)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;

            if (String.IsNullOrEmpty(map)) throw new ArgumentException(nameof(map));
            MapName = map;

            _Players = new Dictionary<int, ServerPlayer>();
        }


        internal void AddPlayer(ServerUser<NetConnection> player)
        {
            _Players.Add(player.Id, new ServerPlayer(player.Id));
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
                case GameAction.MoveDown:
                case GameAction.MoveLeft:
                case GameAction.MoveRight:
                    if (activate) player.Input.Add(action);
                    else player.Input.Remove(action);
                    break;
                case GameAction.ShootPrimary:
                    break;
                case GameAction.ShootSecundary:
                    break;
                case GameAction.NextWeapon:
                    player.SwitchWeapon(1);
                    break;
                case GameAction.PreviousWeapon:
                    player.SwitchWeapon(-1);
                    break;
            }
        }

        internal void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(_Players.Count);
            foreach (var player in _Players.Values)
            {
                player.Serialize(msg);
            }
            // TODO : handle pickups (oh and maps)
        }

        internal void Update(float deltaT)
        {
            // FIXME
            foreach (var player in _Players.Values)
            {
                player.Update(deltaT);
            }
        }
    }
}