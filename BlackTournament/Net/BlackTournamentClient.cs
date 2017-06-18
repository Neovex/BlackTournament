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
        private Dictionary<int, ClientPlayer> _PlayerLookup;
        private Single _UpdateImpulse;


        public String MapName { get; private set; }
        public ClientPlayer Player { get; private set; }
        public float PlayerRotation { get; set; }

        public Boolean IsConnected { get { return _BasePeer.ConnectionsCount != 0; } }
        public override Int32 AdminId { get { return Net.ADMIN_ID; } }

        // Connection Events
        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionHasBeenLost = () => { };

        // Game Events
        public event Action<ClientPlayer> UserJoined = u => { };
        public event Action<ClientPlayer> UserLeft = u => { };

        public event Action<ClientPlayer, String> MessageReceived = (u, m) => { };
        public event Action ChangeLevelReceived = () => { };
        public event Action UpdateReceived = () => { };



        public BlackTournamentClient(String userName):base(Game.ID, userName, Net.COMMANDS)
        {
        }


        // CONTROL
        internal void Update(float deltaT)
        {
            if (IsConnected)
            {
                // Process incoming data
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

        internal String GetAlias(int id)
        {
            return _ConnectedClients.First(u => u.Id == id).Alias;
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
            Send(NetMessage.Rotate, m => { m.Write(Id); m.Write(PlayerRotation); }, method);
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
            var player = _PlayerLookup[msg.ReadInt32()];
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
            var entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var id = msg.ReadInt32();
                if(_PlayerLookup.TryGetValue(id, out ClientPlayer otherPlayer))
                {
                    otherPlayer.Deserialize(msg);
                }
                else
                {
                    otherPlayer = new ClientPlayer(id, msg);
                    otherPlayer.Alias = GetAlias(id);
                    _PlayerLookup.Add(id, otherPlayer);
                    UserJoined.Invoke(otherPlayer);
                }
            }
            UpdateReceived.Invoke();
        }

        protected override void UserConnected(User user)
        {
            // Called from server when another user has connected to the current server
            // override is intentionally empty - user adding is handled in ServerUpdate(msg);
        }

        protected override void UserDisconnected(User user)
        {
            UserLeft.Invoke(_PlayerLookup[user.Id]);
            _PlayerLookup.Remove(user.Id);
        }

        protected override void Connected(int id, string alias)
        {
            Player = new ClientPlayer(id) { Alias = alias };
            _PlayerLookup = new Dictionary<int, ClientPlayer>();
            _PlayerLookup.Add(id, Player);
            ConnectionEstablished.Invoke();
        }

        protected override void Disconnected()
        {
            ConnectionHasBeenLost.Invoke();
        }
    }
}