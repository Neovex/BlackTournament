using System;
using Lidgren.Network;
using SFML.System;
using BlackCoat.Collision.Shapes;
using BlackCoat.Collision;

namespace BlackTournament.Net.Data
{
    public class Pickup : NetEntityBase
    {
        private Boolean _Active;
        private float _RespawnTime;


        public Boolean Active
        {
            get { return _Active; }
            set
            {
                _Active = value;
                _Dirty = true;
                ActiveStateChanged.Invoke(this);
            }
        }

        public Vector2f Position { get; private set; }
        public PickupType Type { get; private set; }
        public int Amount { get; private set; }
        public float RespawnTime { get; private set; }
        public CollisionShape Collision { get; private set; }


        public event Action<Pickup> ActiveStateChanged = p => { };


        public Pickup(int id, Vector2f position, PickupType type, int amount, float respawnTime, CollisionSystem collisionSystem) : base(id)
        {
            Active = true;
            Position = position;
            Type = type;
            Amount = amount;
            RespawnTime = _RespawnTime = respawnTime;
            CreateCollision(collisionSystem);
        }

        public Pickup(int id, NetIncomingMessage m) : base(id, m)
        {
        }

        public void Update(float deltaT)
        {
            if (_Active) return;
            _RespawnTime -= deltaT;
            if (_RespawnTime > 0) return;
            Active = true;
            _RespawnTime = RespawnTime;
        }

        protected override void SerializeInternal(NetOutgoingMessage m, bool fullSync)
        {
            m.Write(Active);
            if (fullSync)
            {
                m.Write(Position.X);
                m.Write(Position.Y);
                m.Write((int)Type);
            }
        }

        protected override void DeserializeInternal(NetIncomingMessage m, bool fullSync)
        {
            Active = m.ReadBoolean();
            if (fullSync)
            {
                Position = new Vector2f(m.ReadSingle(), m.ReadSingle());
                Type = (PickupType)m.ReadInt32();
            }
        }

        private void CreateCollision(CollisionSystem collisionSystem)
        {
            switch (Type)
            {
                case PickupType.SmallHealth:
                case PickupType.SmallShield:
                    Collision = new RectangleCollisionShape(collisionSystem, Position, new Vector2f(20,40));
                    break;
                case PickupType.BigHealth:
                case PickupType.BigShield:
                    Collision = new CircleCollisionShape(collisionSystem, Position, 20);
                    break;
                case PickupType.Drake:
                case PickupType.Hedgeshock:
                case PickupType.Thumper:
                case PickupType.Titandrill:
                    Collision = new RectangleCollisionShape(collisionSystem, Position, new Vector2f(60, 40));
                    break;
            }
        }
    }
}