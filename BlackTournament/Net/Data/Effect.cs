using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public enum EffectType
    {
        Environment,
        Impact,
        Gunfire
    }

    public class Effect : NetEntityBase
    {
        public EffectType EffectType { get; private set; }
        public Vector2f Position { get; private set; }

        public PickupType Source { get; private set; }
        public bool Primary { get; private set; }


        public Effect(int id, EffectType type, Vector2f position, PickupType source = PickupType.BigHealth, bool primary = false) : base(id)
        {
            EffectType = type;
            Position = position;
            Source = source;
            Primary = primary;
        }

        public Effect(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            EffectType = (EffectType)m.ReadInt32();
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());

            Source = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write((int)EffectType);
            m.Write(Position.X);
            m.Write(Position.Y);

            m.Write((int)Source);
            m.Write(Primary);
        }
    }
}