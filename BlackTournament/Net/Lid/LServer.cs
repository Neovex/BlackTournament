using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public abstract class LServer<TEnum> : LServerBase<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        protected const int _ADMIN_ID = -1;

        protected int _ClientIdProvider = 100;
        protected List<User<NetConnection>> _ConnectedClients;
        protected Commands _Commands;

        public IEnumerable<User<NetConnection>> ConnectedUsers { get { return _ConnectedClients; } }


        public LServer(string appIdentifier, Commands commands) : base(appIdentifier)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            _ConnectedClients = new List<User<NetConnection>>();
            _Commands = commands;
        }



        protected override void ClientConnected(NetConnection connection)
        {
            var user = new User<NetConnection>(GetNextFreeClientID(), connection, ValidateName(connection.RemoteHailMessage.ReadString()));
            _ConnectedClients.Add(user);

            var message = new Action<NetOutgoingMessage>(m => { m.Write(user.Id); m.Write(user.Alias); });
            Send(user.Connection, _Commands.Handshake, message);
            Broadcast(_Commands.UserConnected, message);

            UserConnected(user);
        }

        protected abstract void UserConnected(User<NetConnection> user);


        protected override void ClientDisconnected(NetConnection connection)
        {
            var user = _ConnectedClients.FirstOrDefault(u => u.Connection == connection);
            if (user == null)
            {
                Log.Warning("Received disconnection message from unknown connection:", connection.RemoteEndPoint);
            }
            else
            {
                _ConnectedClients.Remove(user);
                Broadcast(_Commands.UserDisconnected, m => { m.Write(user.Id); m.Write(user.Alias); });
                UserDisconnected(user);
            }
        }

        protected abstract void UserDisconnected(User<NetConnection> user);


        protected virtual int GetNextFreeClientID()
        {
            return _ConnectedClients.Count == 0 ? _ADMIN_ID : ++_ClientIdProvider;
        }

        protected virtual string ValidateName(string name)
        {
            return name;
        }

        public class Commands
        {
            public readonly TEnum Handshake;
            public readonly TEnum UserConnected;
            public readonly TEnum UserDisconnected;

            public Commands(TEnum handshake, TEnum userConnected, TEnum userDisconnected)
            {
                Handshake = handshake;
                UserConnected = userConnected;
                UserDisconnected = userDisconnected;
            }
        }
    }
}