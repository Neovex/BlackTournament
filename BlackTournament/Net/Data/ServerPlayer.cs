using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Server;
using BlackTournament.Systems;
using Lidgren.Network;

namespace BlackTournament.Net.Data
{
    class ServerPlayer : Player
    {
        private const int _SPEED = 400;

        public ServerUser<NetConnection> User { get; private set; }
        public List<GameAction> Input { get; private set; }

        public ServerPlayer(ServerUser<NetConnection> user) : base(user.Id)
        {
            User = user;
            Input = new List<GameAction>();
        }

        public void Update(float deltaT)
        {
            foreach (var action in Input)
            {
                switch (action)
                {
                    case GameAction.MoveUp:
                        Y -= _SPEED * deltaT;
                        break;
                    case GameAction.MoveDown:
                        Y += _SPEED * deltaT;
                        break;
                    case GameAction.MoveLeft:
                        X -= _SPEED * deltaT;
                        break;
                    case GameAction.MoveRight:
                        X += _SPEED * deltaT;
                        break;

                    case GameAction.ShootPrimary:
                        Log.Debug(action);
                        break;
                    case GameAction.ShootSecundary:
                        Log.Debug(action);
                        break;
                }
            }
        }

        public void SwitchWeapon(int direction)
        {
            var index = OwnedWeapons.IndexOf(CurrentWeapon) + direction;
            if (index == -1) index = OwnedWeapons.Count - 1;
            else if (index == OwnedWeapons.Count) index = 0;
            CurrentWeapon = OwnedWeapons[index];
        }
    }
}