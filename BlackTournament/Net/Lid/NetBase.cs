using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public abstract class NetBase<TEnum> : IDisposable where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        protected const NetDeliveryMethod _DEFAULT_METHOD = NetDeliveryMethod.ReliableOrdered;

        protected NetPeer _BasePeer;

        public Boolean Disposed { get; private set; }

        
        public NetBase(NetPeer peer)
        {
            if (peer == null) throw new ArgumentNullException(nameof(peer));
            _BasePeer = peer;
        }
        ~NetBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            _BasePeer.Shutdown("Unexpected Shutdown");
            _BasePeer = null;
        }

        public void ProcessMessages()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));

            NetIncomingMessage msg;
            while ((msg = _BasePeer.ReadMessage()) != null)
            {
                //Log.Debug("Server", msg.MessageType);
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Error: // Should never happen
                        Log.Fatal(msg.MessageType, msg.SenderConnection.Status);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        Log.Debug(nameof(LServerBase<TEnum>), "-", (NetConnectionStatus)msg.ReadByte(), msg.ReadString(), "-", _BasePeer.Status, msg.SenderConnection.Status);

                        switch (msg.SenderConnection.Status) // TODO check for running server?
                        {
                            case NetConnectionStatus.Connected:
                                NewConnection(msg.SenderConnection);
                                break;

                            case NetConnectionStatus.Disconnected:
                                ConnectionLost(msg.SenderConnection);
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
                _BasePeer.Recycle(msg);
            }
        }


        // INCOMMING

        protected abstract void ProcessIncommingData(TEnum subType, NetIncomingMessage msg);

        protected abstract void NewConnection(NetConnection senderConnection);

        protected abstract void ConnectionLost(NetConnection senderConnection);

        
        // OUTGOING

        protected virtual NetOutgoingMessage CreateMessage(TEnum subType, Action<NetOutgoingMessage> operation = null)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(NetBase<TEnum>));
            var message = _BasePeer.CreateMessage();
            message.Write(subType.ToInt32(Format.DEFAULT));
            operation?.Invoke(message);
            return message;
        }

        
        //MISC
        public sealed class Format : IFormatProvider
        {
            // SINGLETON
            private static Format _INSTANCE;
            public static Format DEFAULT { get { return _INSTANCE ?? (_INSTANCE = new Format()); } }


            private Format()
            { }

            public object GetFormat(Type formatType)
            {
                return typeof(int);
            }
        }
    }
}