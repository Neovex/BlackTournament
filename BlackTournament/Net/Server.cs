using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Threading.Tasks;

namespace BlackTournament.Net
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    class Server : IGameServer, IDisposable
    {
        private Dictionary<IContextChannel, IGameClient> _ConnectedClients = new Dictionary<IContextChannel, IGameClient>();
        public ServiceHost _ServiceHost = null;


        public Boolean IncludeExceptionDetailInFaults { get; set; }

        public event Action<float, float> Moved = (x, y) => { };


        public Server()
        {
        }

        ~Server()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_ServiceHost == null) return;
            ((IDisposable)_ServiceHost).Dispose();
            _ServiceHost = null;
        }

        public void Host(uint port)
        {
            var adress = new Uri(string.Format("net.tcp://{0}:{1}", Dns.GetHostName(), port));
            var binding = new NetTcpBinding();
            var contract = typeof(IGameServer);

            _ServiceHost = new ServiceHost(this, adress);
            _ServiceHost.AddServiceEndpoint(contract, binding, String.Empty);
            _ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = IncludeExceptionDetailInFaults;
            _ServiceHost.Open();
        }
        
        public bool Subscribe()
        {
            try
            {
                var channel = OperationContext.Current.Channel;
                if (!_ConnectedClients.ContainsKey(channel))
                {
                    channel.Closed += new EventHandler(HandleClientDisconnect);
                    channel.Faulted += new EventHandler(HandleClientDisconnect);

                    var callback = OperationContext.Current.GetCallbackChannel<IGameClient>();
                    _ConnectedClients.Add(channel, callback);
                }
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        private void HandleClientDisconnect(object sender, EventArgs e)
        {
            var channel = sender as IContextChannel;
            if (_ConnectedClients.ContainsKey(channel))
            {
                _ConnectedClients.Remove(channel);
            }
        }

        public void BroadcastMove(int id, float x, float y)
        {
            Parallel.ForEach(_ConnectedClients, client=>
            //foreach (var client in _ConnectedClients) //passt soweit aber üblicher enumerations bug bei disconnect
	        {
                if (client.Key.State == CommunicationState.Opened)
                {
                    client.Value.Move(id, x, y); //fixe timing issue
                }
            });
        }

        public void Move(int id, float x, float y)
        {
            Moved(x, y);
        }
    }
}