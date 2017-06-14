using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using SFML.System;

namespace BlackTournament.Net.Data
{
    public abstract class Player : NetEntityBase
    {
        public override EntityType EntityType { get { return EntityType.Player; } }

        public Vector2f Position { get; protected set; }
        public Single Rotation { get; set; }
        public Int32 Health { get; protected set; }
        public Int32 Shield { get; protected set; }
        public Int32 Score { get; protected set; }

        public List<PickupType> OwnedWeapons { get; private set; }  = new List<PickupType>();

        private PickupType _CurrentWeapon;
        public PickupType CurrentWeapon
        {
            get { return _CurrentWeapon; }
            protected set
            {
                if (!OwnedWeapons.Contains(value)) OwnedWeapons.Add(value);
                _CurrentWeapon = value;
            }
        }


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
            m.Write(Health);
            m.Write(Shield);
            m.Write((int)CurrentWeapon);
            m.Write(Score);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            Position = new Vector2f(m.ReadSingle(), m.ReadSingle());
            Rotation = m.ReadSingle();
            Health = m.ReadInt32();
            Shield = m.ReadInt32();
            CurrentWeapon = (PickupType)m.ReadInt32();
            Score = m.ReadInt32();
        }
    }
}