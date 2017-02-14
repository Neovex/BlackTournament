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

        public abstract EntityType EntityType { get; }


        public NetEntityBase(int id)
        {
            Id = id;
        }
        public NetEntityBase(NetIncomingMessage m)
        {
            Id = m.ReadInt32();
            Deserialize(m);
        }


        public void SerializeCreationInfo(NetOutgoingMessage m)
        {
            m.Write((int)EntityType);
            Serialize(m);
        }

        public void Serialize(NetOutgoingMessage m)
        {
            m.Write(Id);
            SerializeInternal(m);
        }

        protected abstract void SerializeInternal(NetOutgoingMessage m);
        public abstract void Deserialize(NetIncomingMessage m);
    }
}