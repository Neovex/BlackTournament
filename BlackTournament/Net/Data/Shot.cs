using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    class Shot : NetEntityBase
    {
        public Vector2f Start { get; set; }
        public float Direction { get; set; }
        public float Length { get; set; }
        public PickupType SourceWeapon { get; private set; }
        public bool Primary { get; private set; }

        public Shot(int id, PickupType sourceWeapon, bool primary):base(id)
        {
            SourceWeapon = sourceWeapon;
            Primary = primary;
        }


        public override void Deserialize(NetIncomingMessage m)
        {
            Start = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Direction = m.ReadFloat();
            Length = m.ReadFloat();
            SourceWeapon = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Start.X);
            m.Write(Start.Y);
            m.Write(Direction);
            m.Write(Length);
            m.Write((int)SourceWeapon);
            m.Write(Primary);
        }
    }
}