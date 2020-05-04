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
                _Dirty = true;
            }
        }

        private float _Rotation;
        public virtual float Rotation
        {
            get { return _Rotation; }
            protected set
            {
                _Rotation = value;
                _Dirty = true;
            }
        }

        private float _Health;
        public virtual float Health
        {
            get { return _Health; }
            protected set
            {
                _Health = value;
                _Dirty = true;
            }
        }

        private float _Shield;
        public virtual float Shield
        {
            get { return _Shield; }
            protected set
            {
                _Shield = value;
                _Dirty = true;
            }
        }

        private int _Score;
        public virtual int Score
        {
            get { return _Score; }
            protected set
            {
                _Score = value;
                _Dirty = true;
            }
        }

        private PickupType _CurrentWeaponType;
        public virtual PickupType CurrentWeaponType
        {
            get { return _CurrentWeaponType; }
            protected set
            {
                _CurrentWeaponType = value;
                _Dirty = true;
            }
        }
        public Boolean IsAlive => Health > 0;

        public Dictionary<PickupType, Weapon> Weapons { get; } = new Dictionary<PickupType, Weapon>();
        public virtual int Ping { get; protected set; }


        public Player(int id) : base(id)
        {
        }

        protected override void SerializeInternal(NetOutgoingMessage m, bool fullSync)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Rotation);
            m.Write(Score);

            if (fullSync)
            {
                m.Write(Health);
                m.Write(Shield);
                m.Write((int)CurrentWeaponType);

                m.Write(Weapons.Count);
                foreach (var weapon in Weapons.Values)
                {
                    m.Write((int)weapon.WeaponType);
                    weapon.Serialize(m);
                }
            }
            m.Write(Ping);
        }

        protected override void DeserializeInternal(NetIncomingMessage m, bool fullSync)
        {
            Position = new Vector2f(m.ReadSingle(), m.ReadSingle());
            Rotation = m.ReadSingle();
            Score = m.ReadInt32();

            if (fullSync)
            {
                Health = m.ReadSingle();
                Shield = m.ReadSingle();
                CurrentWeaponType = (PickupType)m.ReadInt32();

                var ownedWeapons = m.ReadInt32();
                if (ownedWeapons < Weapons.Count) Weapons.Clear();
                for (int i = 0; i < ownedWeapons; i++)
                {
                    var type = (PickupType)m.ReadInt32();
                    if (!Weapons.ContainsKey(type)) Weapons[type] = new Weapon(type);
                    Weapons[type].Deserialize(m);
                }
            }
            Ping = m.ReadInt32();
        }
    }
}