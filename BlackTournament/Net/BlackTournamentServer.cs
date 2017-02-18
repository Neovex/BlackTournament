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
        private float _UpdateImpulse;

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
            Host(port);
            ChangeLevel(AdminId, map);
        }

        // Update from core->game
        internal void Update(float deltaT)
        {
            // Handle Incomming Data
            ProcessMessages();

            // Update Server Data
            //TODO : update DAL

            // Update Client Data (~60Hz)
            _UpdateImpulse += deltaT;
            if (_UpdateImpulse >= _IMPULSE)
            {
                _UpdateImpulse = 0;
                SendClientUpdate();
            }
        }

        // OUTGOING
        private void SendClientUpdate()
        {
            Broadcast(NetMessage.Update, null, NetDeliveryMethod.UnreliableSequenced); // TODO : replace null
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
                    ProcessGameAction(msg.ReadInt32(), (GameAction)msg.ReadInt32());
                break;

                case NetMessage.StopServer:
                    StopServer(msg.ReadInt32());
                break;
            }
        }

        protected override void UserConnected(ServerUser<NetConnection> user)
        {
            Send(user.Connection, NetMessage.ChangeLevel, m => m.Write(CurrentMap));
            _Score.Add(user.Id, 0);
        }

        protected override void UserDisconnected(ServerUser<NetConnection> user)
        {
            _Score.Remove(user.Id);
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
            if (IsAdmin(id)) StopServer(String.Empty);
            // TODO : check if dropped players will be removed by other events or if this needs to be done manually
        }

        private void ChangeLevel(int id, string map)
        {
            if (IsAdmin(id))
            {
                // TODO : validate & load level data
                CurrentMap = map;
                _Score = _Score.ToDictionary(kvp => kvp.Key, kvp => 0);
                Broadcast(NetMessage.ChangeLevel, m => m.Write(CurrentMap));
            }
        }

        private void ProcessGameAction(int id, GameAction action) // CSH
        {
            switch (action)
            {
                case GameAction.Confirm:
                    break;
                case GameAction.Cancel:
                    break;
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
                    break;
                case GameAction.PreviousWeapon:
                    break;
                case GameAction.ShowStats:
                    break;
            }
        }
    }
}