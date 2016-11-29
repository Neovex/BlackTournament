using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Threading.Tasks;
using BlackTournament.Net.Contract;

namespace BlackTournament.Net
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameServer : Server<IGameClient>, IGameServer, IDisposable
    {
        public String CurrentMap { get; set; }
        public int AdminId { get; set; }


        public GameServer()
        {
        }

        public string GetLevel()
        {
            return CurrentMap;
        }

        public void HostGame(string map, uint port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server));
            CurrentMap = map;
            Host(port);
        }

        override public SubscriptionResult Subscribe(string name)
        {
            var result = base.Subscribe(name);
            return result;
        }

        override protected void HandleClientDisconnect(object sender, EventArgs e)
        {
            base.HandleClientDisconnect(sender, e); // ???
        }

        public void Shoot(int id)
        {
            Broadcast(client => client.Shoot(id));
        }

        public void UpdatePosition(int id, float x, float y, float angle)
        {
            Broadcast(client => client.UpdatePosition(id, x, y, angle));
        }

        public void ChangeLevel(int id, string lvl)
        {
            if (id == Server.ID || id == AdminId) Broadcast(client => client.ChangeLevel(lvl));
        }
    }
}