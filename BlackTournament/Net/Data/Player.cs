using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    public abstract class Player : NetEntityBase
    {
        public override EntityType EntityType { get { return EntityType.Player; } }

        public Single X { get; protected set; }
        public Single Y { get; protected set; }
        public Single R { get; set; }
        public Int32 Health { get; protected set; }
        public Int32 Shield { get; protected set; }
        public Int32 Score { get; protected set; }

        public List<PickupType> OwnedWeapons { get; private set; } = new List<PickupType>();

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
            Health = 100;
            CurrentWeapon = PickupType.Drake;
        }
        public Player(int id, NetIncomingMessage m) : base(id, m) // Client CTOR (data will be automatically deserialized)
        {
        }

        protected override void SerializeInternal(NetOutgoingMessage m)
        {
            m.Write(X);
            m.Write(Y);
            m.Write(R);
            m.Write(Health);
            m.Write(Shield);
            m.Write((int)CurrentWeapon);
            m.Write(Score);
        }

        public override void Deserialize(NetIncomingMessage m)
        {
            X = m.ReadSingle();
            Y = m.ReadSingle();
            R = m.ReadSingle();
            Health = m.ReadInt32();
            Shield = m.ReadInt32();
            CurrentWeapon = (PickupType)m.ReadInt32();
            Score = m.ReadInt32();
        }
    }
}