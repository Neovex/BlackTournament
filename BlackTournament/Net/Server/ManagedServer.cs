using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Server
{
    public abstract class ManagedServer<TEnum> : Server<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        protected int _ClientIdProvider = 100;
        protected List<ServerUser<NetConnection>> _ConnectedClients;
        protected Commands<TEnum> _Commands;

        public abstract int AdminId { get; }
        public virtual IEnumerable<ServerUser<NetConnection>> ConnectedUsers { get { return _ConnectedClients; } }


        public ManagedServer(string appIdentifier, Commands<TEnum> commands) : base(appIdentifier)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            _ConnectedClients = new List<ServerUser<NetConnection>>();
            _Commands = commands;
        }

        // CONTROL

        protected virtual int GetNextFreeClientID()
        {
            return _ConnectedClients.Count == 0 ? AdminId : ++_ClientIdProvider;
        }

        protected virtual string ValidateName(string name)
        {
            return name;
        }

        // INCOMMING

        protected override void NewConnection(NetConnection connection)
        {
            var user = new ServerUser<NetConnection>(GetNextFreeClientID(), connection, ValidateName(connection.RemoteHailMessage.ReadString()));

            // Accept Client - respond with designated id, validated alias and the list of other connected users
            Send(user.Connection, _Commands.Handshake, new Action<NetOutgoingMessage>(m => 
            {
                m.Write(user.Id);
                m.Write(user.Alias);
                m.Write(_ConnectedClients.Count);
                foreach (var client in _ConnectedClients)
                {
                    m.Write(client.Id);
                    m.Write(client.Alias);
                }
            }));

            // Add user to user management list
            _ConnectedClients.Add(user);
            // Inform other Clients of new user
            Broadcast(_Commands.UserConnected, new Action<NetOutgoingMessage>(m => { m.Write(user.Id); m.Write(user.Alias); }));

            UserConnected(user);
        }


        protected override void ConnectionLost(NetConnection connection)
        {
            var user = _ConnectedClients.FirstOrDefault(u => u.Connection == connection);
            if (user == null)
            {
                Log.Warning("Received disconnect message from unknown connection:", connection.RemoteEndPoint);
            }
            else
            {
                _ConnectedClients.Remove(user);
                Broadcast(_Commands.UserDisconnected, m => m.Write(user.Id));
                UserDisconnected(user);
            }
        }

        protected abstract void UserConnected(ServerUser<NetConnection> user);
        protected abstract void UserDisconnected(ServerUser<NetConnection> user);
    }
}