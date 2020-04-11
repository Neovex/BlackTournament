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
        WallImpact,
        PlayerImpact,
        Gunfire,
        Explosion,
        PlayerDrop,
        Gore,
        Pickup,
        None
    }

    public class Effect : NetEntityBase
    {
        public EffectType EffectType { get; private set; }
        public Vector2f Position { get; set; }
        public float Rotation { get; set; }
        public PickupType Source { get; set; }
        public bool Primary { get; set; }
        public float Size { get; set; }


        public Effect(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public Effect(int id, EffectType type, Vector2f position) : base(id)
        {
            EffectType = type;
            Position = position;
        }

        public Effect(int id, EffectType type, Vector2f position, float rotation, PickupType source, bool primary, float size = 0) : this(id, type, position)
        {
            Rotation = rotation;
            Source = source;
            Primary = primary;
            Size = size;
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            EffectType = (EffectType)m.ReadInt32();
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());

            Rotation = m.ReadSingle();
            Source = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
            Size = m.ReadFloat();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write((int)EffectType);
            m.Write(Position.X);
            m.Write(Position.Y);

            m.Write(Rotation);
            m.Write((int)Source);
            m.Write(Primary);
            m.Write(Size);
        }
    }
}