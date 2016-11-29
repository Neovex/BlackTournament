using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using BlackTournament.Net.Contract;

namespace BlackTournament.Net
{
    public class Client : IClient, IDisposable
    {
        protected String _UserName;
        protected IGameServer _ServerChannel;
        protected DuplexChannelFactory<IGameServer> _ChannelFactory;

        public virtual int Id { get; protected set; }
        public virtual bool Connected { get; protected set; }
        public virtual string Host { get; protected set; }
        public virtual uint Port { get; protected set; }
        public virtual IEnumerable<User> Users { get; protected set; }
        public virtual String UserName
        {
            get { return _UserName; }
            set { if(Connected) _UserName = _ServerChannel.ChangeUserName(value); }
        }


        public event Action ConnectionEstablished = () => { };
        public event Action ConnectionFailed = () => { };
        public event Action ConnectionLost = () => { };
        public event Action ConnectionClosed = () => { };

        public event Action<int, string> ClientConnectedReceived = (id, name) => { };
        public event Action<int> ClientDisconnectedReceived = (id) => { };
        public event Action<int, string> MessageReceived = (id, msg) => { };



        public Client(String host, UInt32 port, String userName)
        {
            Host = host;
            Port = port;
            _UserName = userName;
        }

        ~Client()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (_ServerChannel != null)
            {
                try
                {
                    ((IDisposable)_ServerChannel).Dispose();
                }
                catch (Exception e)
                {
                    Log.Warning("Client channel dispose error:", e);
                }
                _ServerChannel = null;
            }

            if (_ChannelFactory != null)
            {
                try
                {
                    ((IDisposable)_ChannelFactory).Dispose();
                }
                catch (Exception e)
                {
                    Log.Warning("Client channel factory dispose error:", e);
                }
                _ChannelFactory = null;
            }
        }

        public virtual void Connect()// fixme 4 async
        {
            var address = new EndpointAddress(String.Format("net.tcp://{0}:{1}", Host, Port));
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            _ChannelFactory = new DuplexChannelFactory<IGameServer>(new InstanceContext(this), binding, address);
            _ServerChannel = _ChannelFactory.CreateChannel();

            var result = _ServerChannel.Subscribe(_UserName);
            Connected = result.Success;
            if (Connected)
            {
                // Update Client Info
                Id = result.Id;
                _UserName = result.Alias;
                Users = result.Users;

                // Register to connection events
                var channel = _ServerChannel as ICommunicationObject;
                channel.Closed += HandleServerConnectionClosed;
                channel.Faulted += HandleServerConnectionLost;

                // Done
                Log.Debug("Connected to", Host, ":", Port, "as", _UserName);
                ConnectionEstablished.Invoke();
            }
            else
            {
                Log.Debug("Failed to connect to", Host, ":", Port, "as", _UserName);
                ConnectionFailed.Invoke();
            }
        }

        private void HandleServerConnectionClosed(object sender, EventArgs e)
        {
            if (Connected) HandleServerConnectionLost(sender, e);
        }

        private void HandleServerConnectionLost(object sender, EventArgs e)
        {
            Connected = false;
            ConnectionLost.Invoke();
        }

        public virtual void Disconnect()
        {
            Connected = false;
            ((IClientChannel)_ServerChannel).Close();
            ConnectionClosed.Invoke();
        }


        public virtual void ClientConnected(int id, string name)
        {
            ClientConnectedReceived.Invoke(id, name);
        }

        public virtual void ClientDisconnected(int id)
        {
            ClientDisconnectedReceived.Invoke(id);
        }

        public virtual void Message(int id, string msg)
        {
            MessageReceived.Invoke(id, msg);
        }
        public virtual void SendMessage(string msg)
        {
            _ServerChannel.Message(msg);
        }
    }
}