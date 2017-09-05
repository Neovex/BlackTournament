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
    public class Shot : NetEntityBase
    {
        private CollisionSystem _CollisionSystem;
        private ICollisionShape _Collision;
        private Vector2f _MovementVector;


        public Player Owner { get; private set; }
        public Vector2f Position { get; private set; }
        public float Direction { get; private set; }
        public PickupType SourceWeapon { get; private set; }
        public bool Primary { get; private set; }

        public float Length { get; private set; }
        public float Speed { get; private set; }
        public float TTL { get; private set; }
        public Boolean Alive => TTL > 0;


        public Shot(CollisionSystem collisionSystem, Player owner, int id, PickupType sourceWeapon, bool primary):base(id)
        {
            _CollisionSystem = collisionSystem;
            Owner = owner;
            Position = owner.Position;
            Direction = owner.Rotation;
            SourceWeapon = sourceWeapon;
            Primary = primary;

            _MovementVector = VectorExtensions.VectorFromAngle(Direction);

            switch (sourceWeapon)
            {
                case PickupType.Drake:
                    Speed = primary ? 800 : 400;
                    TTL = 2;
                break;

                case PickupType.Hedgeshock:
                    Length = 280;
                    Speed = 300;
                    _Collision = new LineCollisionShape(_CollisionSystem, Position, VectorExtensions.VectorFromAngle(Direction, Length));
                    TTL = 0.8f;
                break;

                case PickupType.Thumper:
                    Speed = 500;
                    if (!primary) _Collision = new CircleCollisionShape(_CollisionSystem, Position, 70);
                    TTL = 5;
                break;

                case PickupType.Titandrill:
                    Length = primary ? 100 : 100000;
                    _Collision = new LineCollisionShape(_CollisionSystem, Position, VectorExtensions.VectorFromAngle(Direction, Length));
                    TTL = 0.5f;
                break;

                default: throw new Exception("Invalid projectile initialization");
            }
        }

        public Shot(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public void Update(float deltaT)
        {
            Position += _MovementVector * Speed * deltaT;
            TTL -= deltaT;
        }

        public void Kill()
        {
            TTL = 0;
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
                        if (other.Collide(_Collision)) return _CollisionSystem.Intersections(Position, Direction, other);
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
                        if (other.Collide(_Collision)) return _CollisionSystem.Intersections(Position, Direction, other);
                    }
                    break;


                case PickupType.Titandrill:
                    return _CollisionSystem.Intersections(Position, Direction, other);
            }
            return new Vector2f[0];
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Direction = m.ReadFloat();
            Length = m.ReadFloat();
            SourceWeapon = (PickupType)m.ReadInt32();
            Primary = m.ReadBoolean();
            Speed = m.ReadFloat();
            TTL = m.ReadFloat();
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Direction);
            m.Write(Length);
            m.Write((int)SourceWeapon);
            m.Write(Primary);
            m.Write(Speed);
            m.Write(TTL);
        }
    }
}