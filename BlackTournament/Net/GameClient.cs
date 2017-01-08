using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using BlackTournament.Net.Contract;
using BlackTournament.System;

namespace BlackTournament.Net
{
    public class GameClient : Client, IGameClient, IDisposable
    {
        public string MapName { get; private set; }
        public bool IsAdmin { get { return Id < 0; } }

        public event Action<int, float, float, float> UpdatePositionReceived = (id, x, y, a) => { };
        public event Action<int> ShotReceived = id => { };
        public event Action<string> ChangeLevelReceived = lvl => { };

        public GameClient(String host, uint port, String userName) : base(host, port, userName)
        {
        }


        public override void Connect()
        {
            base.Connect();
            MapName = _ServerChannel.GetLevel();
        }

        public void Shoot(int id)
        {
            ShotReceived(id);
        }

        public void UpdatePosition(int id, float x, float y, float angle)
        {
            UpdatePositionReceived.Invoke(id, x, y, angle);
        }

        public void StopServer()
        {
            _ServerChannel.StopServer(Id);
        }

        public void ProcessGameAction(GameAction action)
        {
            _ServerChannel.ProcessGameAction(Id, action);
        }

        public void ChangeLevel(string lvl)
        {
            MapName = lvl;
            ChangeLevelReceived.Invoke(MapName);
        }
    }
}