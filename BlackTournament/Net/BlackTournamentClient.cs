using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Client;
using BlackTournament.Net.Data;
using BlackTournament.Systems;
using Lidgren.Network;

namespace BlackTournament.Net
{
    public class BlackTournamentClient : ManagedClient<NetMessage>
    {
        public  Dictionary<int, ClientPlayer> _Players; // FIXME!!
        private Single _UpdateImpulse;


        public String MapName { get; private set; }
        public ClientPlayer Player { get; private set; }

        public Boolean IsConnected { get { return _BasePeer.ConnectionsCount != 0; } }
        public override Int32 AdminId { get { return Net.ADMIN_ID; } }

        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionHasBeenLost = () => { };

        public event Action ChangeLevelReceived = () => { };
        public event Action<ClientPlayer, String> MessageReceived = (u, m) => { };
        public event Action<ClientPlayer> UserJoined = u => { };
        public event Action<ClientPlayer> UserLeft = u => { };
        public event Action UpdateReceived = () => { };



        public BlackTournamentClient(String userName):base(userName, Net.COMMANDS)
        {
        }


        // CONTROL
        internal void Update(float deltaT)
        {
            if (IsConnected)
            {
                // Process Incomming Data
                ProcessMessages();

                // Update Rotation on Server (~60Hz)
                _UpdateImpulse += deltaT;
                if (_UpdateImpulse >= Net.UPDATE_IMPULSE && Player != null)
                {
                    _UpdateImpulse = 0;
                    UpdatePlayerRotation(NetDeliveryMethod.UnreliableSequenced);
                }
            }
        }


        // OUTGOING
        public void ProcessGameAction(GameAction action, Boolean activate)
        {
            var method = NetDeliveryMethod.UnreliableSequenced;
            if(action == GameAction.ShootPrimary || action == GameAction.ShootSecundary)
            {
                method = NetDeliveryMethod.ReliableOrdered;
                UpdatePlayerRotation(method);
            }
            Send(NetMessage.ProcessGameAction, m => { m.Write(Id); m.Write((int)action); m.Write(activate); }, method);
        }

        private void UpdatePlayerRotation(NetDeliveryMethod method)
        {
            Send(NetMessage.Rotate, m => { m.Write(Id); m.Write(Player.R); }, method);
        }

        public void SendMessage(String txt)
        {
            Send(NetMessage.TextMessage, m => { m.Write(Id); m.Write(txt); });
        }

        public void StopServer()
        {
            Send(NetMessage.StopServer, m => m.Write(Id));
        }

        public void Disconnect()
        {
            Disconnect(String.Empty);
        }


        // INCOMMING
        protected override void DataReceived(NetMessage message, NetIncomingMessage msg)
        {
            switch (message)
            {
                case NetMessage.TextMessage:
                    TextMessage(msg);
                break;

                case NetMessage.ChangeLevel:
                    ChangeLevel(msg);
                break;

                case NetMessage.Update:
                    ServerUpdate(msg);
                break;
            }
        }

        private void TextMessage(NetIncomingMessage msg)
        {
            var player = _Players[msg.ReadInt32()];
            var txt = msg.ReadString();
            Log.Info(player.Alias, txt);
            MessageReceived(player, txt);
        }

        private void ChangeLevel(NetIncomingMessage msg)
        {
            MapName = msg.ReadString();
            ChangeLevelReceived();
        }

        private void ServerUpdate(NetIncomingMessage msg)
        {
            int id;
            ClientPlayer player;
            int entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                id = msg.ReadInt32();
                if(_Players.TryGetValue(id, out player))
                {
                    player.Deserialize(msg);
                }
                else
                {
                    player = new ClientPlayer(id, msg);
                    player.Alias = _ConnectedClients.First(u => u.Id == id).Alias;
                    _Players.Add(id, player);
                    UserJoined.Invoke(player);
                }
            }
            UpdateReceived.Invoke();
        }

        protected override void UserConnected(User user)
        {
            // Handled in Update
        }

        protected override void UserDisconnected(User user)
        {
            UserLeft.Invoke(_Players[user.Id]);
            _Players.Remove(user.Id);
        }

        protected override void Connected(int id, string alias)
        {
            Player = new ClientPlayer(id) { Alias = alias };
            _Players = new Dictionary<int, ClientPlayer>();
            _Players.Add(id, Player);
            ConnectionEstablished.Invoke();
        }

        protected override void Disconnected()
        {
            ConnectionHasBeenLost.Invoke();
        }
    }
}