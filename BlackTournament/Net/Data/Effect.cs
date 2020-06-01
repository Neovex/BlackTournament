using Lidgren.Network;
using SFML.System;
using BlackNet;

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


        public Effect(EffectType type, Vector2f position)
        {
            EffectType = type;
            Position = position;
        }
        public Effect(int id, NetIncomingMessage m) : base(id, m)
        {
        }


        public Effect(EffectType type, Vector2f position, float rotation, PickupType source, bool primary, float size = 0) : this(type, position)
        {
            Rotation = rotation;
            Source = source;
            Primary = primary;
            Size = size;
        }

        protected override void SerializeInternal(NetOutgoingMessage m, bool fullSync)
        {
            m.Write((int)EffectType);
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Rotation);
            m.Write((int)Source);
            m.Write(Primary);
            m.Write(Size);
        }

        protected override void DeserializeInternal(NetIncomingMessage m, bool fullSync)
        {
            EffectType = (EffectType)m.ReadInt32();
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Rotation = m.ReadSingle();
            Source = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
            Size = m.ReadFloat();
        }
    }
}