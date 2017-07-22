using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Server;
using BlackTournament.Systems;
using Lidgren.Network;
using SFML.System;
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


        public event Action<ServerPlayer> Spawn = p => { };
        public event Action<ServerPlayer, Boolean> ShotFired = (pl, pr) => { };


        public ServerPlayer(ServerUser<NetConnection> user, CollisionSystem collisionSystem) : base(user.Id)
        {
            User = user;
            Input = new HashSet<GameAction>();
            Dead = true;
            Collision = new CircleCollisionShape(collisionSystem, Position, _COLLISION_RADIUS);
        }

        public void Move (Vector2f position)
        {
            Position = position;
        }
        public void Rotate(float rotation)
        {
            Rotation = rotation;
        }

        public void Fragg()
        {
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
        }

        public void ShootPrimary()
        {
            if (Dead)
            {
                if (RespawnTimeout <= 0) Spawn.Invoke(this);
            }
            else
            {
                // TODO : add ammo handling
                ShotFired.Invoke(this, true);
            }
        }
        public void ShootSecundary()
        {
            // TODO : add ammo handling
            ShotFired.Invoke(this, false);
        }

        public void Respawn(Vector2f spawnPosition)
        {
            // Reset Player
            Collision.Position = Position = spawnPosition;
            Dead = false;
            Health = 100;
            Shield = 0;

            OwnedWeapons.Clear();
            CurrentWeapon = PickupType.Drake;
        }

        public void SwitchWeapon(int direction)
        {
            var index = OwnedWeapons.IndexOf(CurrentWeapon) + direction;
            if (index == -1) index = OwnedWeapons.Count - 1;
            else if (index == OwnedWeapons.Count) index = 0;
            CurrentWeapon = OwnedWeapons[index];
        }

        public void GivePickup(PickupType pickup, int amount) // move this into pickup
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
                    CurrentWeapon = pickup;
                    break;
            }
        }

        internal void GotShot(Shot shot)
        {
            // gefällt mir nicht
            throw new NotImplementedException();
        }
    }
}