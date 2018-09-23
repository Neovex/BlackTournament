using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;
using BlackCoat;
using BlackCoat.Collision;

namespace BlackTournament.Net.Data
{
    public class Shot : NetEntityBase
    {
        private Vector2f _MovementVector;
        private Action<Vector2f> _UpdatePosition;
        private float _Direction;

        public float Speed { get; private set; }
        public float Damage { get; private set; }
        public float BlastRadius { get; private set; }
        public float TTL { get; private set; }
        public bool Alive => TTL > 0;

        public PickupType SourceWeapon { get; private set; }
        public bool Primary { get; private set; }

        public Vector2f Position { get; private set; }
        public Vector2f LastPosition { get; private set; }
        public float Direction { get => _Direction; set { _Direction = value; _MovementVector = Create.Vector2fFromAngle(value); } }
        public ICollisionShape Collision { get; }

        public bool IsExplosive => BlastRadius != 0;
        public bool IsBouncy => !Primary && (SourceWeapon == PickupType.Thumper || SourceWeapon == PickupType.Hedgeshock);
        public bool IsPenetrating => !Primary && SourceWeapon == PickupType.Hedgeshock;
        public bool Exploded { get; set; }

        public Shot(int id, float direction, float speed, float damage, float blastRadius, float ttl, PickupType sourceWeapon, bool primary, Vector2f position, Action<Vector2f> updatePosition = null, ICollisionShape collision = null) : base(id)
        {
            _UpdatePosition = updatePosition;

            Speed = speed;
            Damage = damage;
            BlastRadius = blastRadius;
            TTL = ttl;

            SourceWeapon = sourceWeapon;
            Primary = primary;

            Position = position;
            Direction = direction;
            Collision = collision;
        }

        public Shot(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public void Update(float deltaT)
        {
            TTL -= deltaT;
            LastPosition = Position;
            Position += _MovementVector * Speed * deltaT;
            _UpdatePosition?.Invoke(Position); // To update collision shape position.
        }

        public void Destroy()
        {
            TTL = 0;
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Direction = m.ReadFloat();
            SourceWeapon = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
            TTL = m.ReadFloat();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Direction);
            m.Write((int)SourceWeapon);
            m.Write(Primary);
            m.Write(TTL);
        }

        internal void Reset()
        {
            Position = LastPosition;
            _UpdatePosition?.Invoke(Position); // To update collision shape position.
        }
    }
}