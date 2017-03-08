using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    class Pickup : NetEntityBase
    {
        public override EntityType EntityType { get { return EntityType.Pickup; } }

        public Boolean Active { get; set; }


        public Pickup(int id) : base(id)
        {
            Active = true;
        }
        public Pickup(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Active);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Active = m.ReadBoolean();
        }
    }
}