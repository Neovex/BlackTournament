using System;
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
        public Single RespawnTimeout { get; private set; }
        public CircleCollisionShape Collision { get; private set; }
        public ServerWeapon CurrentWeapon { get => (ServerWeapon)Weapons[CurrentWeaponType]; set => Weapons[CurrentWeaponType] = value; }
        public Vector2f WeaponSpawn => Position + Create.Vector2fFromAngle(Rotation + 16, 35);


        public event Action<ServerPlayer, Boolean> ShotFired = (pl, pr) => { };


        public ServerPlayer(ServerUser<NetConnection> user, CollisionSystem collisionSystem) : base(user.Id) // TODO : replace collision system with actual collision shape
        {
            User = user;
            Input = new HashSet<GameAction>();
            Collision = new CircleCollisionShape(collisionSystem, Position, _COLLISION_RADIUS);
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
            if (!IsAlive) return;


            Shield -= damage * 0.7f;
            if (Shield < 0)
            {
                Health += Shield;
                Shield = 0;
            }
            Health -= damage * 0.3f;
            if (!IsAlive) Fragg();
        }
        public void Fragg()
        {
            CurrentWeapon.Release();
            Health = Shield = 0;
            RespawnTimeout = _DEFAULT_RESPAWN_TIME;
        }

        public void Update(float deltaT)
        {
            if (!IsAlive) RespawnTimeout -= deltaT;

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

            CurrentWeapon.Update(deltaT);
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
            if (!IsAlive) return;
            if (activate) CurrentWeapon.Fire(primary);
            else CurrentWeapon.Release();
        }

        public void Respawn(Vector2f spawnPosition)
        {
            // Reset Player
            Collision.Position = Position = spawnPosition;
            Health = 100;
            Shield = 0;

            foreach (var weapon in Weapons.Values)
            {
                ((ServerWeapon)weapon).ShotFired -= HandleWeaponFired;
            }
            Weapons.Clear();
            GivePickup(PickupType.Drake); // Initial Weapon
        }

        public void SwitchWeapon(int direction)
        {
            if (!IsAlive) return;

            var newWeaponType = CurrentWeapon.WeaponType;
            do
            {
                var index = WeaponData.IndexOf(newWeaponType) + direction;
                if (index == -1) newWeaponType = PickupType.Titandrill;
                else newWeaponType = WeaponData.GetTypeByIndex(index);
            }
            while (CurrentWeapon.WeaponType != newWeaponType && (!Weapons.ContainsKey(newWeaponType) || Weapons[newWeaponType].Empty));

            if (CurrentWeapon.WeaponType != newWeaponType)
            {
                CurrentWeapon.Release();
                CurrentWeaponType = newWeaponType;
            }
        }

        public void GivePickup(PickupType pickup, int amount = 0) // TODO : move this into pickup, grant write access via method or prop & fix amount stuff
        {
            switch (pickup)
            {
                case PickupType.SmallHealth:
                    Health += amount != 0 ? amount : 2;
                    break;
                case PickupType.SmallShield:
                    Shield += amount != 0 ? amount : 2;
                    break;
                case PickupType.BigHealth:
                    Health += amount != 0 ? amount : 50;
                    break;
                case PickupType.BigShield:
                    Shield += amount != 0 ? amount : 50;
                    break;
                case PickupType.Drake:
                case PickupType.Hedgeshock:
                case PickupType.Thumper:
                case PickupType.Titandrill:
                    CurrentWeaponType = pickup;
                    if(Weapons.ContainsKey(CurrentWeaponType)) CurrentWeapon.ShotFired -= HandleWeaponFired;
                    CurrentWeapon = new ServerWeapon(CurrentWeaponType);
                    CurrentWeapon.ShotFired += HandleWeaponFired;
                    break;
            }

            if (Health > 100) Health = 100;
            if (Shield > 100) Shield = 100;
        }

        private void HandleWeaponFired(bool primary)
        {
            ShotFired.Invoke(this, primary);
            if (CurrentWeapon.Empty) SwitchWeapon(-1);
        }
    }
}