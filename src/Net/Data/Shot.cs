﻿using System;
using Lidgren.Network;
using SFML.System;
using BlackNet;
using BlackCoat;
using BlackCoat.Collision;

namespace BlackTournament.Net.Data
{
    public class Shot : NetEntityBase
    {
        private float _Direction;
        private Vector2f _MovementVector;
        private readonly Action<Vector2f> _UpdatePosition;


        public Vector2f Position { get; private set; }
        public float Direction
        {
            get => _Direction;
            set 
            {
                _Direction = value;
                _MovementVector = Create.Vector2fFromAngle(value);
            }
        }

        public PickupType SourceWeapon { get; private set; }
        public bool Primary { get; private set; }
        public int Owner { get; }

        public float Speed { get; }
        public float Damage { get; }
        public float BlastRadius { get;}
        
        public float TTL { get; private set; }

        public ICollisionShape Collision { get; }

        public bool Alive => TTL > 0;
        public bool IsExplosive => BlastRadius != 0;
        public bool IsBouncy => !Primary && (SourceWeapon == PickupType.Thumper || SourceWeapon == PickupType.Hedgeshock);
        public bool IsPenetrating => !Primary && SourceWeapon == PickupType.Hedgeshock;
        public bool Exploded { get; set; }


        public Shot(Vector2f weaponSpawn, float orientation, PickupType weapon, bool primary, int owner,
                    float speed, float damage, float blastRadius, float ttl, 
                    Action<Vector2f> updatePosition = null, ICollisionShape collision = null)
        {
            Position = weaponSpawn;
            Direction = orientation;

            SourceWeapon = weapon;
            Primary = primary;
            Owner = owner;

            Speed = speed;
            Damage = damage;
            BlastRadius = blastRadius;
            TTL = ttl;

            _UpdatePosition = updatePosition;
            Collision = collision;
        }
        public Shot(int id, NetIncomingMessage m) : base(id, m)
        {
        }


        public void Update(float deltaT)
        {
            TTL -= deltaT;
            Position += _MovementVector * Speed * deltaT;
            _UpdatePosition?.Invoke(Position); // To update collision shape position.
            _Dirty = true;
        }

        public void Destroy()
        {
            TTL = 0;
        }

        protected override void SerializeInternal(NetOutgoingMessage m, bool fullSync)
        {
            m.Write(Position.X);
            m.Write(Position.Y);
            m.Write(Direction);
            if (fullSync)
            {
                m.Write((int)SourceWeapon);
                m.Write(Primary);
            }
            m.Write(TTL);
        }

        protected override void DeserializeInternal(NetIncomingMessage m, bool fullSync)
        {
            Position = new Vector2f(m.ReadFloat(), m.ReadFloat());
            Direction = m.ReadFloat();
            if (fullSync)
            {
                SourceWeapon = (PickupType)m.ReadInt32();
                Primary = m.ReadBoolean();
            }
            TTL = m.ReadFloat();
        }
    }
}