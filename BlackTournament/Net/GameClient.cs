using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using BlackTournament.Net.Contract;

namespace BlackTournament.Net
{
    public class GameClient : Client, IGameClient, IDisposable
    {
        public string MapName { get; private set; }

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
        public void SendShot()
        {
            _ServerChannel.Shoot(Id);
        }

        public void UpdatePosition(int id, float x, float y, float angle)
        {
            UpdatePositionReceived.Invoke(id, x, y, angle);
        }
        public void SendUpdatePosition(int id, float x, float y, float angle)
        {
            _ServerChannel.UpdatePosition(id, x, y, angle);
        }

        public void ChangeLevel(string lvl)
        {
            MapName = lvl;
            ChangeLevelReceived.Invoke(MapName);
        }
    }
}