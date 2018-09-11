﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Systems;
using Lidgren.Network;
using SFML.System;
using BlackCoat;
using BlackCoat.Network;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;

namespace BlackTournament.Net.Data
{
    class ServerPlayer : Player
    {
        private const int _SPEED = 400;
        private const float _DEFAULT_RESPAWN_TIME = 5.0f;
        private const float _COLLISION_RADIUS = 30f;

        public ServerUser<NetConnection> User { get; private set; }
        public HashSet<GameAction> Input { get; private set; }
        public Boolean Dead { get; set; }
        public Single RespawnTimeout { get; private set; }
        public CircleCollisionShape Collision { get; private set; }
        public Dictionary<PickupType,Weapon> Weapons { get; private set; }
        public Weapon Weapon => Weapons[CurrentWeaponType];
        public Vector2f WeaponSpawn => Position + Create.Vector2fFromAngle(Rotation + 16, 35);


        public event Action<ServerPlayer, Boolean> ShotFired = (pl, pr) => { };


        public ServerPlayer(ServerUser<NetConnection> user, CollisionSystem collisionSystem) : base(user.Id) // TODO : replace collision system with actual collision shape
        {
            User = user;
            Input = new HashSet<GameAction>();
            Dead = true;
            Collision = new CircleCollisionShape(collisionSystem, Position, _COLLISION_RADIUS);
            Weapons = new Dictionary<PickupType, Weapon>();
            GivePickup(PickupType.Drake); // Initial Weapon
        }

        public void Move (Vector2f position)
        {
            Position = position;
        }
        public void Rotate(float rotation)
        {
            Rotation = rotation;
        }


        public void DamagePlayer(float damage)
        {
            if (Dead) return;
            Shield -= damage;
            if (Shield < 0)
            {
                Health += Shield;
                Shield = 0;
            }
            if (Health <= 0) Fragg();
        }
        public void Fragg()
        {
            Weapon.Release();
            Dead = true;
            Health = Shield = 0;
            RespawnTimeout = _DEFAULT_RESPAWN_TIME;

            // mayhaps add K/D stuff here
        }

        public void Update(float deltaT)
        {
            if (Dead) RespawnTimeout -= deltaT;

            float x = 0, y = 0;
            foreach (var action in Input)
            {
                switch (action)
                {
                    case GameAction.MoveUp:
                        y = -_SPEED * deltaT;
                        break;
                    case GameAction.MoveDown:
                        y = _SPEED * deltaT;
                        break;
                    case GameAction.MoveLeft:
                        x = -_SPEED * deltaT;
                        break;
                    case GameAction.MoveRight:
                        x = _SPEED * deltaT;
                        break;
                }
            }
            Collision.Position += new Vector2f(x, y);

            Weapon.Update(deltaT);
        }

        public void ShootPrimary(bool activate)
        {
            Shoot(activate, true);
        }
        public void ShootSecundary(bool activate)
        {
            Shoot(activate, false);
        }
        private void Shoot(bool activate, bool primary)
        {
            if (Dead) return;
            if (activate) Weapon.Fire(primary);
            else Weapon.Release();
        }

        public void Respawn(Vector2f spawnPosition)
        {
            // Reset Player
            Collision.Position = Position = spawnPosition;
            Dead = false;
            Health = 100;
            Shield = 0;

            foreach (var weapon in Weapons.Values)
            {
                weapon.ShotFired -= HandleWeaponFired;
            }
            Weapons.Clear();
            OwnedWeapons.Clear();
            GivePickup(PickupType.Drake); // Initial Weapon
        }

        public void SwitchWeapon(int direction)
        {
            if (Dead) return;
            Weapon.Release();

            var index = OwnedWeapons.IndexOf(CurrentWeaponType) + direction;
            if (index < 0) index = OwnedWeapons.Count - 1;
            CurrentWeaponType = OwnedWeapons[index % OwnedWeapons.Count];
        }

        public void GivePickup(PickupType pickup, int amount = 1) // TODO : move this into pickup, grant write access via method or prop & fix amount stuff
        {
            switch (pickup)
            {
                case PickupType.SmallHealth:
                    Health += 15;
                    break;
                case PickupType.BigHealth:
                    Health += 40;
                    break;
                case PickupType.SmallShield:
                    Shield += 15;
                    break;
                case PickupType.BigShield:
                    Shield += 40;
                    break;
                case PickupType.Drake:
                case PickupType.Hedgeshock:
                case PickupType.Thumper:
                case PickupType.Titandrill:
                    CurrentWeaponType = pickup;
                    if(Weapons.ContainsKey(pickup)) Weapons[pickup].ShotFired -= HandleWeaponFired;
                    Weapons[pickup] = WeaponData.CreateWeaponFrom(pickup);
                    Weapon.ShotFired += HandleWeaponFired;
                    break;
            }

            if (Health > 100) Health = 100;
            if (Shield > 100) Shield = 100;
        }

        private void HandleWeaponFired(bool primary)
        {
            ShotFired.Invoke(this, primary);
        }
    }
}