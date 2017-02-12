using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackTournament.Net.Server;
using BlackTournament.Systems;
using Lidgren.Network;

namespace BlackTournament.Net
{
    class BlackTournamentServer : ManagedServer<GameMessageType>
    {
        private Core _Core;
        private Dictionary<int, int> _Score;


        public string CurrentMap { get; set; }
        public override int AdminId { get { return Net.ADMIN_ID; } }

        public BlackTournamentServer(Core core) : base(Game.NET_ID, Net.COMMANDS)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;
            _Score = new Dictionary<int, int>();
        }

        // CONTROL

        private bool IsAdmin(int id)
        {
            return id == AdminId;
        }

        public void HostGame(string map, int port)
        {
            StopServer("Restart?"); //$
            ChangeLevel(AdminId, map);
            Host(port);

            // Test
            H1(null);
        }

        // TEST ##########################
        private void H1(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(0, 200, 1, UpdatePosition, BlackCoat.Animation.InterpolationType.Linear, H2);
        }
        private void H2(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(200, 0, 2, UpdatePosition, BlackCoat.Animation.InterpolationType.InQuad, H1);
        }
        // /TEST #########################


        // Update from core->game
        internal void Update(float deltaT)
        {
            ProcessMessages();
            if (true) DoSomething(); // FIXME
        }

        // OUTGOING
        private void DoSomething()
        {
            // FIXME
        }


        // INCOMMING

        protected override void ProcessIncommingData(GameMessageType type, NetIncomingMessage msg)
        {
            Log.Debug("game server data", type, msg.ReadString());
        }

        protected override void UserConnected(ServerUser<NetConnection> user)
        {
            //ChangeLevel(Server.ID, CurrentMap); whu?
        }

        protected override void UserDisconnected(ServerUser<NetConnection> user)
        {
            // TODO
        }

        // MAPPED HANDLERS

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
                Broadcast(GameMessageType.LoadMap, msg => msg.Write(map));
            }
        }

        public void StopServer(int id)
        {
            //if (IsAdmin(id)) StopServer();
        }

        public void ProcessGameAction(int id, GameAction action) // CSH
        {

        }

        public void UpdatePosition(float pos)
        {
            Broadcast(GameMessageType.UpdatePosition, msg => msg.Write(pos), NetDeliveryMethod.UnreliableSequenced);
        }
    }
}