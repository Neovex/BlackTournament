using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Threading.Tasks;
using BlackTournament.Net.Contract;
using BlackTournament.System;
using BlackCoat;

namespace BlackTournament.Net
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameServer : Server<IGameClient>, IGameServer, IDisposable
    {
        private Core _Core;
        private Dictionary<int, int> _Score;

        private int _TextX;
        private int _TextY;


        public string CurrentMap { get; set; }
        public int AdminId { get; set; }


        public GameServer(Core core)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));
            _Core = core;
            _Score = new Dictionary<int, int>();
        }

        protected override int GetNextFreeClientID()
        {
            if(_ConnectedClients.Count == 0)
            {
                return AdminId = _Core.Random.Next(int.MinValue, -1);
            }
            return base.GetNextFreeClientID();
        }

        //override protected void HandleClientDisconnect(object sender, EventArgs e)
        //{
        //    base.HandleClientDisconnect(sender, e); // ???
        //}

        public void HostGame(string map, uint port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(GameServer));
            ChangeLevel(Server.ID, map);
            Host(port);

            // Test
            H1(null);
        }

        private void H1(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(0, 200, 1, f => Broadcast(c => c.UpdatePosition(0, f, 0, 0)), BlackCoat.Animation.InterpolationType.Linear, H2);
        }
        private void H2(BlackCoat.Animation.Animation a)
        {
            _Core.AnimationManager.Run(200, 0, 2, f => Broadcast(c => c.UpdatePosition(0, f, 0, 0)), BlackCoat.Animation.InterpolationType.InQuad, H1);
        }

        private bool IsAdmin(int id)
        {
            return id == Server.ID || id == AdminId;
        }


        // Update from core->game
        internal void Update(float deltaT)
        {
            // TODO!!
        }


        // WCF CONTRACT METHODS:

        public string GetLevel()
        {
            return CurrentMap;
        }

        public void ChangeLevel(int id, string map)
        {
            if (IsAdmin(id))
            {
                // TODO : validate & load level data
                CurrentMap = map;
                _Score = _Score.ToDictionary(kvp => kvp.Key, kvp => 0);
                Broadcast(client => client.ChangeLevel(map));
            }
        }

        public void StopServer(int id)
        {
            if (IsAdmin(id)) StopServer();
        }

        public void ProcessGameAction(int id, GameAction action) // CSH
        {
            // idea: before the full system just move simple block by 1 px for update client testing
            //Broadcast(client => client.Shoot(id));
        }
    }
}