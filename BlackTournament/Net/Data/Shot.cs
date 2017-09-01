using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    class Shot : NetEntityBase
    {
        private CollisionSystem _CollisionSystem;
        private ICollisionShape _Collision;

        public Player Owner { get; private set; }
        public Vector2f Start { get; private set; }
        public float Direction { get; private set; }
        public float Length { get; private set; }
        public PickupType SourceWeapon { get; private set; }
        public bool Primary { get; private set; }
        public float Speed { get; private set; }
        public Vector2f Position { get; private set; }
        public float TTL { get; private set; }

        //public ICollisionShape Collision { get; }


        public Shot(CollisionSystem collisionSystem, Player owner, int id, PickupType sourceWeapon, bool primary):base(id)
        {
            _CollisionSystem = collisionSystem;
            Owner = owner;
            Start = owner.Position;
            SourceWeapon = sourceWeapon;
            Primary = primary;

            switch (sourceWeapon)
            {
                case PickupType.Drake:
                    Speed = primary ? 80 : 40;
                break;

                case PickupType.Hedgeshock:
                    Length = 280;
                    Speed = 30;
                    _Collision = new LineCollisionShape(_CollisionSystem, Start, VectorExtensions.VectorFromAngle(Direction, Length));
                break;

                case PickupType.Thumper:
                    Speed = 50;
                    if (!primary) _Collision = new CircleCollisionShape(_CollisionSystem, Start, 70);
                break;

                case PickupType.Titandrill:
                    Length = primary ? 100 : 100000;
                    _Collision = new LineCollisionShape(_CollisionSystem, Start, VectorExtensions.VectorFromAngle(Direction, Length));
                break;

                default: throw new Exception("Invalid projectile initialization");
            }
        }


        public void Update(float deltaT)
        {
            Position *= Speed * deltaT;
        }

        public Vector2f[] GetIntersections(ICollisionShape other)
        {
            switch (SourceWeapon)
            {
                case PickupType.Drake:
                    if (other.Collide(Position)) return new[] { Position };
                break;

                case PickupType.Hedgeshock:
                    if (Primary)
                    {
                        if (other.Collide(_Collision)) return _CollisionSystem.Intersections(Start, Direction, other);
                    }
                    else
                    {
                        if (other.Collide(Position)) return new[] { Position };
                    }
                break;

                case PickupType.Thumper:
                    if (Primary)
                    {
                        if (other.Collide(Position)) return new[] { Position };
                    }
                    else
                    {
                        if (other.Collide(_Collision)) return _CollisionSystem.Intersections(Start, Direction, other);
                    }
                    break;


                case PickupType.Titandrill:
                    return _CollisionSystem.Intersections(Start, Direction, other);
            }
            return new Vector2f[0];
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Start = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Direction = m.ReadFloat();
            Length = m.ReadFloat();
            SourceWeapon = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
            Speed = m.ReadFloat();
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Start.X);
            m.Write(Start.Y);
            m.Write(Direction);
            m.Write(Length);
            m.Write((int)SourceWeapon);
            m.Write(Primary);
            m.Write(Speed);
            m.Write(Position.X);
            m.Write(Position.Y);
        }
    }
}