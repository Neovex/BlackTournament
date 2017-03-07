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
    class BlackTournamentServer : ManagedServer<NetMessage>
    {
        private const float _IMPULSE = 1f / 60;


        private Core _Core;
        private GameLogic _Logic;
        private float _UpdateImpulse;


        public override int AdminId { get { return Net.ADMIN_ID; } }


        public BlackTournamentServer(Core core) : base(Game.ID, Net.COMMANDS)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;
        }


        // CONTROL
        private bool IsAdmin(int id)
        {
            return id == AdminId;
        }

        public void HostGame(string map, int port)
        {
            Host(port);
            ChangeLevel(AdminId, map);
        }

        // Update from core->game
        internal void Update(float deltaT)
        {
            // Handle Incomming Data
            ProcessMessages();

            if (_Logic == null) return;

            // Update Server Data
            _Logic.Update(deltaT); // consider 60hz as well

            // Update Client Data (~60Hz)
            _UpdateImpulse += deltaT;
            if (_UpdateImpulse >= _IMPULSE)
            {
                _UpdateImpulse = 0;
                Broadcast(NetMessage.Update, _Logic.Serialize, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        // INCOMMING
        protected override void ProcessIncommingData(NetMessage type, NetIncomingMessage msg)
        {
            switch (type)
            {
                case NetMessage.TextMessage:
                    SendMessage(msg.ReadInt32(), msg.ReadString());
                break;

                case NetMessage.ChangeLevel:
                    ChangeLevel(msg.ReadInt32(), msg.ReadString());
                break;

                case NetMessage.ProcessGameAction:
                    _Logic.ProcessGameAction(msg.ReadInt32(), (GameAction)msg.ReadInt32(), msg.ReadBoolean());
                break;

                case NetMessage.StopServer:
                    StopServer(msg.ReadInt32());
                break;
            }
        }

        protected override void UserConnected(ServerUser<NetConnection> user)
        {
            _Logic.AddPlayer(user);
            Send(user.Connection, NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
        }

        protected override void UserDisconnected(ServerUser<NetConnection> user)
        {
            _Logic.RemovePlayer(user);
        }

        // MAPPED HANDLERS
        private void SendMessage(int id, string message)
        {
            Broadcast(NetMessage.TextMessage, m =>
            {
                m.Write(id);
                m.Write(message);
            });
        }

        private void StopServer(int id)
        {
            if (IsAdmin(id))
            {
                StopServer(String.Empty);
                // TODO : check if dropped players will be removed by other events or if this needs to be done here manually
                _Logic = null;
            }
        }

        private void ChangeLevel(int id, string map)
        {
            if (IsAdmin(id))
            {
                _Logic = new GameLogic(_Core, map);
                foreach (var user in ConnectedUsers) _Logic.AddPlayer(user);
                Broadcast(NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
            }
        }
    }
}