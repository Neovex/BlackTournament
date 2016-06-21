 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackTournament.Net
{
    public class NetworkManager
    {
        public event Action Connected = () => { };
        public event Action Disconnected = () => { };

        public NetworkManager()
        {

        }

        public void Connect(string host, uint port)
        {
            var _Client = new Client();
            _Client.Connect(host, port); // make async
            //_Client.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
            //TransmitMove(_Client);
            Log.Debug("Connection to", host, ":", port);
        }

        public void Host(uint port)
        {
            var _Server = new Server();
            _Server.Host(port);
            //_Server.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
            //TransmitMove(_Server);
            Log.Debug("Host is listening on", port);
        }

        /*
        private static void TransmitMove(Server server)
        {
            server.BroadcastMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TransmitMove(server));
        }

        private static void TransmitMove(Client client)
        {
            client.DoMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TransmitMove(client));
        }*/

        internal void Disconnect()
        {
            throw new NotImplementedException();
        }

        public string MapName { get; set; }
    }
}