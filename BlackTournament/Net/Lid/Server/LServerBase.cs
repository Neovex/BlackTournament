using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public abstract class LServerBase<TEnum> : NetBase<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private NetServer _Server;

        public String AppIdentifier { get; private set; }


        public LServerBase(string appIdentifier):base(new NetPeer(new NetPeerConfiguration(appIdentifier)))
        {
            if (String.IsNullOrWhiteSpace(appIdentifier)) throw new ArgumentException(nameof(appIdentifier));
            AppIdentifier = appIdentifier;
        }


        // CONTROL

        public void Host(int port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));

            StopServer(String.Empty);

            var config = new NetPeerConfiguration(AppIdentifier);
            config.Port = port;
            
            _BasePeer = _Server = new NetServer(config);
            _Server.Start();
            Log.Info("Server started, listening on:", port);
        }

        public void StopServer(string stopMessage)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));
            _BasePeer.Shutdown(stopMessage);
            Log.Info("Server Stopped");
        }


        // OUTGOING

        protected virtual void Broadcast(TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Broadcast(CreateMessage(subType, operation), netMethod);
        }
        protected virtual void Broadcast(NetOutgoingMessage message, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));
            _Server.SendToAll(message, netMethod);
        }

        protected virtual void Send(NetConnection receiver, TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Send(CreateMessage(subType, operation), receiver, netMethod);
        }
        protected virtual void Send(NetOutgoingMessage message, NetConnection receiver, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (Disposed) throw new ObjectDisposedException(nameof(LServerBase<TEnum>));
            _Server.SendMessage(message, receiver, netMethod);
        }
    }
}