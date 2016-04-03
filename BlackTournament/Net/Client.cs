using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;

namespace BlackTournament.Net
{
    class Client : IGameClient, IDisposable
    {
        private IGameServer _ServerChannel;
        private DuplexChannelFactory<IGameServer> _ChannelFactory;

        public event Action<float, float> Moved = (x, y) => { };

        public Client()
        {
        }

        ~Client()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_ServerChannel != null)
            {
                ((IDisposable)_ServerChannel).Dispose();
                _ServerChannel = null;
            }

            if (_ChannelFactory != null)
            {
                ((IDisposable)_ChannelFactory).Dispose();
                _ChannelFactory = null;
            }
        }

        public void Connect(String hostname, uint port)
        {
            var address = new EndpointAddress(String.Format("net.tcp://{0}:{1}", hostname, port));
            var binding = new NetTcpBinding();
            _ChannelFactory = new DuplexChannelFactory<IGameServer>(new InstanceContext(this), binding, address);
            _ServerChannel = _ChannelFactory.CreateChannel();
            _ServerChannel.Subscribe();
        }

        public void Close()
        {
            ((IClientChannel)_ServerChannel).Close();
        }

        public void DoMove(int id, float x, float y)
        {
            _ServerChannel.Move(id, x, y);
        }

        public void Move(int id, float x, float y)
        {
            Moved(x, y);
        }
    }
}
