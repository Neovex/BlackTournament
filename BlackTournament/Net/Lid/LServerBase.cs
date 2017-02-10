using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public abstract class LServerBase<TEnum> : IDisposable where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private const NetDeliveryMethod _DEFAULT_METHOD = NetDeliveryMethod.ReliableOrdered;

        private _IntFormat _FormatProvider;
        protected NetServer _Server;

        public Boolean Disposed { get; private set; }
        public String AppIdentifier { get; private set; }
        protected Boolean ServerNotOperational
        {
            get
            {
                if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));
                return _Server == null;
            }
        }


        public LServerBase(string appIdentifier)
        {
            if (String.IsNullOrWhiteSpace(appIdentifier)) throw new ArgumentException(nameof(appIdentifier));
            Disposed = false;
            AppIdentifier = appIdentifier;
            _FormatProvider = new _IntFormat();
        }
        ~LServerBase()
        {
            Dispose();
        }


        // CONTROL

        public void Host(int port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));

            var config = new NetPeerConfiguration(AppIdentifier);
            config.Port = port;

            _Server = new NetServer(config);
            _Server.Start();
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

        // INCOMMING

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
                        Log.Debug(nameof(LServerBase<TEnum>), "-", (NetConnectionStatus)msg.ReadByte(), msg.ReadString(), "-", _Server.Status, msg.SenderConnection.Status);

                        switch (msg.SenderConnection.Status) // check for running server?
                        {
                            case NetConnectionStatus.Connected:
                                ClientConnected(msg.SenderConnection);
                            break;

                            case NetConnectionStatus.Disconnected:
                                ClientDisconnected(msg.SenderConnection);
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
                    case NetIncomingMessageType.ConnectionLatencyUpdated: // TODO
                        Log.Info(msg.MessageType);
                    break;
                }
                _Server.Recycle(msg);
            }
        }

        protected abstract void ProcessIncommingData(TEnum subType, NetIncomingMessage msg);

        protected abstract void ClientConnected(NetConnection senderConnection);

        protected abstract void ClientDisconnected(NetConnection senderConnection);

        // OUTGOING

        protected virtual void Broadcast(TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Broadcast(CreateMessage(subType, operation), netMethod);
        }
        protected virtual void Broadcast(NetOutgoingMessage message, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (ServerNotOperational) return;
            _Server.SendToAll(message, netMethod);
        }

        protected virtual void Send(NetConnection receiver, TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Send(CreateMessage(subType, operation), receiver, netMethod);
        }
        protected virtual void Send(NetOutgoingMessage message, NetConnection receiver, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (ServerNotOperational) return;
            _Server.SendMessage(message, receiver, netMethod);
        }

        protected virtual NetOutgoingMessage CreateMessage(TEnum subType, Action<NetOutgoingMessage> operation = null)
        {
            var message = _Server.CreateMessage();
            message.Write(subType.ToInt32(_FormatProvider));
            operation?.Invoke(message);
            return message;
        }

        // FORMATTER

        private sealed class _IntFormat : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return typeof(int);
            }
        }
    }
}