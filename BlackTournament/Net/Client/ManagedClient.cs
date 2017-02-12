using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.System;
using Lidgren.Network;

namespace BlackTournament.Net.Client
{
    public abstract class ManagedClient<TEnum> : Client<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        protected Commands<TEnum> _Commands;
        protected List<ClientUser> _ConnectedClients;


        public virtual Int32 Id { get; protected set; }
        public virtual String Alias { get; protected set; }
        public virtual IEnumerable<ClientUser> ConnectedUsers { get { return _ConnectedClients; } }
        public virtual Boolean IsAdmin { get { return Id == AdminId; } }
        public abstract Int32 AdminId { get; }


        public ManagedClient(String alias, Commands<TEnum> commands)
        {
            if (String.IsNullOrWhiteSpace(alias)) throw new ArgumentException(nameof(alias));
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            Alias = alias;
            _Commands = commands;
        }

        public void Connect(String host, Int32 port)
        {
            Connect(host, port, Alias);
        }

        protected override void Connected()
        {
            // Replaced by Connected(Int32 id, String alias);
        }

        protected override void ProcessIncommingData(TEnum subType, NetIncomingMessage msg)
        {
            if (subType.Equals(_Commands.Handshake))
            {
                Connected(Id = msg.ReadInt32(), Alias = msg.ReadString());
            }
            else if (subType.Equals(_Commands.UserConnected))
            {
                var user = new ClientUser(msg.ReadInt32(), msg.ReadString());
                _ConnectedClients.Add(user);
                UserConnected(user);
            }
            else if (subType.Equals(_Commands.UserDisconnected))
            {
                var id = msg.ReadInt32();
                var user = _ConnectedClients.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    Log.Warning("Received disconnect from unknown user id", id);
                }
                else
                {
                    UserDisconnected(user);
                }
            }
            else
            {
                DataReceived(subType, msg);
            }
        }

        protected abstract void Connected(Int32 id, String alias);
        protected abstract void UserConnected(ClientUser user);
        protected abstract void UserDisconnected(ClientUser user);
        protected abstract void DataReceived(TEnum subType, NetIncomingMessage msg); // hmm msg UU mit interface ersetzen
    }
}