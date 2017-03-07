using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Client
{
    public abstract class ManagedClient<TEnum> : Client<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        protected Commands<TEnum> _Commands;
        protected List<User> _ConnectedClients;


        public virtual Int32 Id { get; protected set; }
        public virtual String Alias { get; protected set; }
        public virtual IEnumerable<User> ConnectedUsers { get { return _ConnectedClients; } }
        public virtual Boolean IsAdmin { get { return Id == AdminId; } }
        public abstract Int32 AdminId { get; }


        public ManagedClient(String alias, Commands<TEnum> commands)
        {
            if (String.IsNullOrWhiteSpace(alias)) throw new ArgumentException(nameof(alias));
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            Alias = alias;
            _Commands = commands;
            _ConnectedClients = new List<User>();
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
                HandleHandshake(msg);
            }
            else if (subType.Equals(_Commands.UserConnected))
            {
                HandleUserConnected(msg);
            }
            else if (subType.Equals(_Commands.UserDisconnected))
            {
                HandleUserDisconnected(msg);
            }
            else
            {
                DataReceived(subType, msg);
            }
        }

        private void HandleHandshake(NetIncomingMessage msg)
        {
            Id = msg.ReadInt32();
            Alias = msg.ReadString();
            var clientCount = msg.ReadInt32();
            for (int i = 0; i < clientCount; i++)
            {
                HandleUserConnected(msg, false);
            }
            Connected(Id, Alias);
        }

        private void HandleUserConnected(NetIncomingMessage msg, bool callUserConnected = true)
        {
            var id = msg.ReadInt32();
            if (id != Id)
            {
                var user = new User(id, msg.ReadString());
                _ConnectedClients.Add(user);
                if (callUserConnected) UserConnected(user);
            }
        }

        private void HandleUserDisconnected(NetIncomingMessage msg)
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

        protected abstract void Connected(int id, string alias);
        protected abstract void UserConnected(User user);
        protected abstract void UserDisconnected(User user);
        protected abstract void DataReceived(TEnum subType, NetIncomingMessage msg); // hmm msg UU mit interface ersetzen
    }
}