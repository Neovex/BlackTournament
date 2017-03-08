﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lidgren.Network;

namespace BlackTournament.Net.Server
{
    public abstract class Server<TEnum> : NetBase<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private NetServer _Server;

        public String AppIdentifier { get; private set; }


        public Server(string appIdentifier):base(new NetPeer(new NetPeerConfiguration(appIdentifier)))
        {
            if (String.IsNullOrWhiteSpace(appIdentifier)) throw new ArgumentException(nameof(appIdentifier));
            AppIdentifier = appIdentifier;
        }


        // CONTROL

        public void Host(int port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TEnum>));

            StopServer(String.Empty);

            var config = new NetPeerConfiguration(AppIdentifier);
            config.Port = port;

            _BasePeer = _Server = new NetServer(config);
            _Server.Start();
            Log.Info("Server started, listening on:", port);
        }

        public void StopServer(string stopMessage = "")
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TEnum>));
            if (_BasePeer.Status != NetPeerStatus.Running) return;
            _BasePeer.Shutdown(stopMessage);
            BlockUntilShutdownIsComplete();
            Log.Info("Server Stopped");
        }

        private void BlockUntilShutdownIsComplete()
        {
            // Hacky as fuck, i know - but its working flawlessly
            var c = 0;
            while (_BasePeer.Status == NetPeerStatus.ShutdownRequested)
            {
                // we wait until the old serving thread has finished
                // this is practically a thread join
                // does not last longer that a couple milliseconds
                c++;
                Thread.Sleep(1);
                //Console.WriteLine("###### Server Shutdown WAIT");
            }
            Log.Debug("Shutdown took ~", c, "milliseconds");
        }


        // OUTGOING

        protected virtual void Broadcast(TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Broadcast(CreateMessage(subType, operation), netMethod);
        }
        protected virtual void Broadcast(NetOutgoingMessage message, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TEnum>));
            _Server.SendToAll(message, netMethod);
        }

        protected virtual void Send(NetConnection receiver, TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Send(CreateMessage(subType, operation), receiver, netMethod);
        }
        protected virtual void Send(NetOutgoingMessage message, NetConnection receiver, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TEnum>));
            _Server.SendMessage(message, receiver, netMethod);
        }
    }
}