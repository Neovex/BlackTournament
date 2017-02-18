using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    class Player : NetEntityBase
    {
        public override EntityType EntityType { get { return EntityType.Player; } }

        public Single X { get; set; }
        public Single Y { get; set; }
        public Single R { get; set; }
        public Int32 Health { get; set; }
        public Int32 Shield { get; set; }
        public PickupType CurrentWeapon { get; set; }
        public Int32 Score { get; set; }

        public List<PickupType> OwnedWeapons { get; set; }


        public Player(int id) : base(id) // Server CTOR
        {
            Health = 100;
            OwnedWeapons = new List<PickupType>();
            OwnedWeapons.Add(PickupType.Drake);
        }
        public Player(NetIncomingMessage m) : base(m) // Client CTOR (data will be automatically deserialized)
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