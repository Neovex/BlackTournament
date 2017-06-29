using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

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
                IsDirty = true;
                ActiveStateChanged.Invoke(this);
            }
        }

        public override EntityType EntityType { get { return EntityType.Pickup; } }

        public Vector2f Position { get; private set; }
        public PickupType Type { get; private set; }
        public int Amount { get; private set; }
        public float RespawnTime { get; private set; }


        public event Action<Pickup> ActiveStateChanged = p => { };


        public Pickup(int id, Vector2f position, PickupType type, int amount, float respawnTime) : base(id)
        {
            Active = true;
            Position = position;
            Type = type;
            Amount = amount;
            RespawnTime = _RespawnTime = respawnTime;
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

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Active);
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Amount);
            m.Write((int)Type);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Active = m.ReadBoolean();
            Position = new Vector2f(m.ReadSingle(), m.ReadSingle());
            Amount = m.ReadInt32();
            Type = (PickupType)m.ReadInt32();
        }
    }
}