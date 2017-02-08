using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackTournament.System;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    class LGameServer : LServer<GameMessageType>
    {
        private Core _Core;
        private Dictionary<int, int> _Score;


        public string CurrentMap { get; set; }
        public int AdminId { get; set; }


        public LGameServer(Core core) : base(Game.NET_ID)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;
            _Score = new Dictionary<int, int>();
        }

        //protected override int GetNextFreeClientID()
        //{
        //    if (_ConnectedClients.Count == 0)
        //    {
        //        return AdminId = _Core.Random.Next(int.MinValue, -1);
        //    }
        //    return base.GetNextFreeClientID();
        //}

        public void HostGame(string map, int port)
        {
            ChangeLevel(Server.ID, map);
            Host(port);

            // Test
            H1(null);
        }

        private void H1(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(0, 200, 1, UpdatePosition, BlackCoat.Animation.InterpolationType.Linear, H2);
        }
        private void H2(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(200, 0, 2, UpdatePosition, BlackCoat.Animation.InterpolationType.InQuad, H1);
        }

        private bool IsAdmin(int id)
        {
            return id == Server.ID || id == AdminId;
        }


        // Update from core->game
        internal void Update(float deltaT)
        {
            ProcessMessages();
        }


        // CONTRACT METHODS:

        public string GetLevel()
        {
            return CurrentMap;
        }

        public void ChangeLevel(int id, string map)
        {
            if (IsAdmin(id))
            {
                // TODO : validate & load level data
                CurrentMap = map;
                _Score = _Score.ToDictionary(kvp => kvp.Key, kvp => 0);
                Broadcast(GameMessageType.LoadMap, msg => msg.Write(map), NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void StopServer(int id)
        {
            //if (IsAdmin(id)) StopServer();
        }

        public void ProcessGameAction(int id, GameAction action) // CSH
        {
            // idea: before the full system just move simple block by 1 px for update client testing
            //Broadcast(client => client.Shoot(id));
        }


        public void UpdatePosition(float pos)
        {
            Broadcast(GameMessageType.UpdatePosition, msg => msg.Write(pos));
        }

        protected override void ProcessIncommingData(GameMessageType type, NetIncomingMessage msg)
        {
            Log.Debug("game server data", type, msg.ReadString());
        }

        protected override void ClientConnected(NetConnection senderConnection)
        {
            ChangeLevel(Server.ID, CurrentMap);
        }

        protected override void ClientDisconnected(NetConnection senderConnection)
        {
            //throw new NotImplementedException();
        }
    }
}