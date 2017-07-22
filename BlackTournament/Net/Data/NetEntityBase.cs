using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public abstract class NetEntityBase
    {
        public int Id { get; private set; }
        public Boolean IsDirty { get; protected set; }


        public NetEntityBase(int id)
        {
            Id = id;
        }
        public NetEntityBase(int id, NetIncomingMessage m) : this(id)
        {
            Deserialize(m);
        }


        public void Serialize(NetOutgoingMessage m)
        {
            m.Write(Id);
            SerializeInternal(m);
            IsDirty = false;
        }

        protected abstract void SerializeInternal(NetOutgoingMessage m);
        public abstract void Deserialize(NetIncomingMessage m);
    }
}