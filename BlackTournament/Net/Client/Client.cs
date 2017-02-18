﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Client
{
    public abstract class Client<TEnum> : NetBase<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private NetClient _Client;
        

        public Client() : base(new NetClient(new NetPeerConfiguration(Game.ID)))
        {
            _Client = _BasePeer as NetClient;
        }


        public void Connect(String host, Int32 port, String hail)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Client<TEnum>));
            if (String.IsNullOrWhiteSpace(host)) throw new ArgumentException(nameof(host));

            _Client.Start();
            _Client.Connect(host, port, _Client.CreateMessage(hail));
        }

        public void Disconnect(string disconnectMessage)
        {
            _Client.Disconnect(disconnectMessage);
            // TODO : check if Disconnected needs to be "invoked" here
        }
        

        // OVERRIDES

        protected override void NewConnection(NetConnection senderConnection)
        {
            Connected();
        }
        protected override void ConnectionLost(NetConnection senderConnection)
        {
            Disconnected();
        }

        protected abstract void Connected();
        protected abstract void Disconnected();
        

        // OUTGOING

        protected virtual void Send(TEnum subType, Action<NetOutgoingMessage> operation = null, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            Send(CreateMessage(subType, operation), netMethod);
        }
        protected virtual void Send(NetOutgoingMessage message, NetDeliveryMethod netMethod = _DEFAULT_METHOD)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Client<TEnum>));
            _Client.SendMessage(message, netMethod);
        }
    }
}