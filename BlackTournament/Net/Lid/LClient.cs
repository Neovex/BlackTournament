using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.System;
using Lidgren.Network;

namespace BlackTournament.Net.Lid
{
    public class LClient:IDisposable
    {
        private NetClient _Client;


        public virtual int Id { get; protected set; }
        public virtual bool Connected { get; protected set; }
        public virtual string Host { get; protected set; }
        public virtual int Port { get; protected set; }
        public virtual String UserName { get; protected set; }

        public string MapName { get; private set; }
        public bool IsAdmin { get { return Id < 0; } }



        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionFailed = () => { };
        public event Action ConnectionLost = () => { };
        public event Action ConnectionClosed = () => { };

        public event Action<int, float, float, float> UpdatePositionReceived = (id, x, y, a) => { };
        public event Action<int> ShotReceived = id => { };
        public event Action<string> ChangeLevelReceived = lvl => { };



        public LClient(String host, int port, String userName)
        {
            if (String.IsNullOrWhiteSpace(host)) throw new ArgumentException(nameof(host));
            if (String.IsNullOrWhiteSpace(userName)) throw new ArgumentException(nameof(userName));

            Host = host;
            Port = port;
            UserName = userName;
        }

        public void Connect()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Game.NET_ID);
            _Client = new NetClient(config);
            _Client.Start();
            _Client.Connect(Host, Port, _Client.CreateMessage(UserName));
        }


        public void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = _Client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Error:
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Log.Debug(_Client.Status, msg.SenderConnection.Status);
                        if (msg.SenderConnection.Status == NetConnectionStatus.Connected)
                        {
                            ConnectionEstablished.Invoke();
                            SendMessage("map");
                        }
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        break;
                    case NetIncomingMessageType.Data:
                        switch ((GameMessageType)msg.ReadInt32())
                        {
                            case GameMessageType.LoadMap:
                                MapName = msg.ReadString();
                                ChangeLevelReceived.Invoke(MapName);
                                break;
                            case GameMessageType.UpdatePosition:
                                UpdatePositionReceived.Invoke(0, msg.ReadSingle(), 0, 0);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Receipt:
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        break;
                    case NetIncomingMessageType.NatIntroductionSuccess:
                        break;
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                        break;
                    default:
                        break;
                }
                _Client.Recycle(msg);
            }
        }

        public void Dispose()
        {
           // TODO:
        }
        public void ProcessGameAction(GameAction action)
        {
        }

        public void StopServer()
        {
            // request server shutdown
        }

        public void Disconnect()
        {
            _Client.Disconnect("Disconnect");
        }

        public virtual void SendMessage(string txt)
        {
            var msg = _Client.CreateMessage();
            msg.Write((int)GameMessageType.Message);
            msg.Write(txt);
            _Client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }
    }
}