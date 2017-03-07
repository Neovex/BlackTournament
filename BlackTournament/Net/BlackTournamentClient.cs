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


        public String MapName { get; private set; }
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


        // OUTGOING
        public void ProcessGameAction(GameAction action, Boolean activate)
        {
            var method = NetDeliveryMethod.UnreliableSequenced;
            if(action == GameAction.ShootPrimary || action == GameAction.ShootSecundary)
            {
                method = NetDeliveryMethod.ReliableSequenced;
            }
            Send(NetMessage.ProcessGameAction, m => { m.Write(Id); m.Write((int)action); m.Write(activate); }, method);
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
                    Update(msg);
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

        private void Update(NetIncomingMessage msg)
        {
            ClientPlayer player;
            int entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                if(_Players.TryGetValue(msg.PeekInt32(), out player))
                {
                    msg.ReadInt32(); // FIXME
                    player.Deserialize(msg);
                }
                else
                {
                    player = new ClientPlayer(msg);
                    player.Alias = _ConnectedClients.FirstOrDefault(u => u.Id == player.Id)?.Alias ?? "NoName"; // Check alias issue
                    _Players.Add(player.Id, player);
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
            _Players = new Dictionary<int, ClientPlayer>();
            _Players.Add(id, new ClientPlayer(id)); // add self
            ConnectionEstablished.Invoke();
        }

        protected override void Disconnected()
        {
            ConnectionHasBeenLost.Invoke();
        }
    }
}