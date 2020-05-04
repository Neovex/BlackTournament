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
        public bool NeedsUpdate => _Dirty || _NeverSynced;

        protected bool _Dirty;
        private bool _NeverSynced;


        public NetEntityBase(int id)
        {
            Id = id;
            _Dirty = true;
            _NeverSynced = true;
        }
        public NetEntityBase(int id, NetIncomingMessage m) : this(id)
        {
            Deserialize(m);
        }


        public void Serialize(NetOutgoingMessage m, bool fullSync)
        {
            var needsFullSync = fullSync || _NeverSynced;
            if (_Dirty || needsFullSync)
            {
                m.Write(Id);
                m.Write(needsFullSync);
                SerializeInternal(m, needsFullSync);
                _Dirty = false;
                _NeverSynced = false;
            }
        }

        public void Deserialize(NetIncomingMessage m)
        {
            DeserializeInternal(m, m.ReadBoolean());
        }

        protected abstract void SerializeInternal(NetOutgoingMessage m, bool fullSync);
        protected abstract void DeserializeInternal(NetIncomingMessage m, bool fullSync);
    }
}