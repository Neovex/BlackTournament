using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Client;
using BlackTournament.Systems;
using Lidgren.Network;

namespace BlackTournament.Net
{
    public class BlackTournamentClient : ManagedClient<NetMessage>
    {
        public String MapName { get; private set; }
        public Boolean IsConnected { get { return _BasePeer.ConnectionsCount != 0; } }
        public override Int32 AdminId { get { return Net.ADMIN_ID; } }

        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionHasBeenLost = () => { };

        public event Action ChangeLevelReceived = () => { };
        public event Action<User, String> MessageReceived = (u, m) => { };
        public event Action<User> UserJoined = u => { };
        public event Action<User> UserLeft = u => { };



        public BlackTournamentClient(String userName):base(userName, Net.COMMANDS)
        {
        }


        // OUTGOING
        public void ProcessGameAction(GameAction action)
        {
            var method = NetDeliveryMethod.UnreliableSequenced;
            if(action == GameAction.ShootPrimary || action == GameAction.ShootSecundary)
            {
                method = NetDeliveryMethod.ReliableSequenced;
            }
            Send(NetMessage.ProcessGameAction, m => m.Write((int)action), method);
        }

        public void SendMessage(String txt)
        {
            Send(NetMessage.TextMessage, m => m.Write(txt));
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
                {
                    var id = msg.ReadInt32();
                    var user = _ConnectedClients.FirstOrDefault(u => u.Id == id);
                    if (user == null) return;
                    var txt = msg.ReadString();
                    Log.Info(user.Alias, txt);
                    MessageReceived(user, txt);
                }
                break;

                case NetMessage.ChangeLevel:
                    MapName = msg.ReadString();
                    ChangeLevelReceived();
                break;
            }
        }

        protected override void UserConnected(User user)
        {
            UserJoined.Invoke(user);
        }

        protected override void UserDisconnected(User user)
        {
            UserLeft.Invoke(user);
        }

        protected override void Connected(int id, string alias)
        {
            ConnectionEstablished.Invoke();
        }

        protected override void Disconnected()
        {
            ConnectionHasBeenLost.Invoke();
        }
    }
}