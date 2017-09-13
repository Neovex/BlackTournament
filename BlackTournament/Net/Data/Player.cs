using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public abstract class Player : NetEntityBase
    {
        private Vector2f _Position;
        public virtual Vector2f Position
        {
            get { return _Position; }
            protected set
            {
                _Position = value;
                IsDirty = true;
            }
        }

        private float _Rotation;
        public virtual float Rotation
        {
            get { return _Rotation; }
            protected set
            {
                _Rotation = value;
                IsDirty = true;
            }
        }

        private float _Health;
        public virtual float Health
        {
            get { return _Health; }
            protected set
            {
                _Health = value;
                IsDirty = true;
            }
        }

        private float _Shield;
        public virtual float Shield
        {
            get { return _Shield; }
            protected set
            {
                _Shield = value;
                IsDirty = true;
            }
        }

        private int _Score;
        public virtual int Score
        {
            get { return _Score; }
            protected set
            {
                _Score = value;
                IsDirty = true;
            }
        }

        private PickupType _CurrentWeapon;
        public virtual PickupType CurrentWeapon
        {
            get { return _CurrentWeapon; }
            protected set
            {
                if (!OwnedWeapons.Contains(value)) OwnedWeapons.Add(value);
                _CurrentWeapon = value;
                IsDirty = true;
            }
        }

        public List<PickupType> OwnedWeapons { get; private set; } = new List<PickupType>();


        public Player(int id) : base(id) // Server CTOR
        {
            CurrentWeapon = PickupType.Drake;
        }
        public Player(int id, NetIncomingMessage m) : base(id, m) // Client CTOR (data will be automatically deserialized)
        {
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Rotation);
            m.Write((int)Health); // send Health & shield as int - the float values are only important for the server anyway
            m.Write((int)Shield);
            m.Write(Score);
            m.Write((int)CurrentWeapon);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Position = new Vector2f(m.ReadSingle(), m.ReadSingle());
            Rotation = m.ReadSingle();
            Health = m.ReadInt32();
            Shield = m.ReadInt32();
            Score = m.ReadInt32();
            CurrentWeapon = (PickupType)m.ReadInt32();
        }
    }
}