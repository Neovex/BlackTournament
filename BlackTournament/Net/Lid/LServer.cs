using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public abstract class LServer
    {
        public const int ID = 2;
        public const int ERROR = -1;
    }

    public abstract class LServer<TEnum> : IDisposable where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private _IntFormat _FormatProvider;
        protected Int32 _ClientIdProvider = 100;
        protected List<User<NetConnection>> _ConnectedClients;
        protected NetServer _Server;

        public Boolean Disposed { get; private set; }
        public String AppIdentifier { get; private set; }
        protected Boolean ServerNotOperational
        {
            get
            {
                if (Disposed) throw new ObjectDisposedException(nameof(LServer<TEnum>));
                return _Server == null;
            }
        }


        public LServer(string appIdentifier)
        {
            if (String.IsNullOrWhiteSpace(appIdentifier)) throw new ArgumentException(nameof(appIdentifier));
            Disposed = false;
            AppIdentifier = appIdentifier;
            _FormatProvider = new _IntFormat();
        }
        ~LServer()
        {
            Dispose();
        }


        public void Host(int port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServer<TEnum>));

            var config = new NetPeerConfiguration(AppIdentifier);
            config.Port = port;

            _Server = new NetServer(config);
            _Server.Start();
        }

        public void ProcessMessages()
        {
            if (ServerNotOperational) return;

            NetIncomingMessage msg;
            while ((msg = _Server.ReadMessage()) != null)
            {
                //Log.Debug("Server", msg.MessageType);
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Error: // Should never happen
                        Log.Fatal(msg.MessageType, msg.SenderConnection.Status);
                    break;

                    case NetIncomingMessageType.StatusChanged:
                        Log.Debug(nameof(LServer<TEnum>), "-", (NetConnectionStatus)msg.ReadByte(), msg.ReadString(), "-", _Server.Status, msg.SenderConnection.Status);

                        switch (msg.SenderConnection.Status) // check for running server?
                        {
                            case NetConnectionStatus.Connected:
                            {
                                var user = new User<NetConnection>(GetNextFreeClientID(), msg.SenderConnection,
                                    ValidateName(msg.SenderConnection.RemoteHailMessage.ReadString()));
                                _ConnectedClients.Add(user);
                                ClientConnected(user.Channel);
                            }
                            break;

                            case NetConnectionStatus.Disconnected:
                            {
                                var user = _ConnectedClients.FirstOrDefault(u => u.Channel == msg.SenderConnection);
                                if(user == null)
                                {
                                    Log.Warning("Received disconnection message from unknown channel!", msg.SenderEndPoint);
                                }
                                else
                                {
                                    _ConnectedClients.Remove(user);
                                    ClientDisconnected(msg.SenderConnection);
                                }
                            }
                            break;
                        }
                    break;

                    case NetIncomingMessageType.Data:
                        ProcessIncommingData((TEnum)(Object)msg.ReadInt32(), msg);
                    break;

                    case NetIncomingMessageType.DiscoveryRequest: // TODO
                    break;
                    case NetIncomingMessageType.DiscoveryResponse: // TODO
                    break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                        Log.Debug(msg.ReadString());
                    break;

                    case NetIncomingMessageType.WarningMessage:
                        Log.Warning(msg.ReadString());
                    break;

                    case NetIncomingMessageType.ErrorMessage:
                        Log.Error(msg.ReadString());
                    break;

                    case NetIncomingMessageType.NatIntroductionSuccess:
                    case NetIncomingMessageType.ConnectionLatencyUpdated: //?
                        Log.Info(msg.MessageType);
                    break;
                }
                _Server.Recycle(msg);
            }
        }


        protected virtual int GetNextFreeClientID()
        {
            return ++_ClientIdProvider;
        }

        protected virtual string ValidateName(string name)
        {
            return name;
        }

        protected abstract void ProcessIncommingData(TEnum subType, NetIncomingMessage msg);

        protected abstract void ClientConnected(NetConnection senderConnection);

        protected abstract void ClientDisconnected(NetConnection senderConnection);


        protected virtual void Broadcast(TEnum subType, Action<NetOutgoingMessage> operation, NetDeliveryMethod netMethod = NetDeliveryMethod.UnreliableSequenced)
        {
            if (ServerNotOperational || operation == null) return;
            var message = _Server.CreateMessage();
            message.Write(subType.ToInt32(_FormatProvider)); // works?
            operation(message);
            _Server.SendToAll(message, netMethod);
        }

        public void StopServer()
        {
            if (ServerNotOperational) return;
            _Server.Shutdown("Server Stopped"); //$
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            _Server?.Shutdown("Server Error"); //$
        }

        private sealed class _IntFormat : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return typeof(int);
            }
        }
    }
}