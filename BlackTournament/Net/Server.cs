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
    public abstract class Server
    {
        public const int ID = 0;
        public const int ERROR = -1;
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Server<TClient> : IServer, IDisposable where TClient:class,IClient
    {
        protected List<ConnectedClient<TClient>> _ConnectedClients;
        protected ServiceHost _ServiceHost = null;
        protected Int32 _ClientIdProvider = 100;


        public Boolean IncludeExceptionDetailInFaults { get; set; }
        public Boolean Open { get { return _ServiceHost.State == CommunicationState.Opened; } }
        public Boolean Disposed { get; private set; }


        public Server()
        {
            _ConnectedClients = new List<ConnectedClient<TClient>>();
        }
        ~Server()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Disposed = true;
            if (_ServiceHost == null) return;
            try
            {
                ((IDisposable)_ServiceHost).Dispose();
            }
            catch (Exception e)
            {
                Log.Warning("Server dispose error:", e);
            }
            _ServiceHost = null;
        }

        public virtual void Host(uint port)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TClient>));
            var adress = new Uri(String.Format("net.tcp://{0}:{1}", Dns.GetHostName(), port));
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            var contract = typeof(IGameServer);

            _ServiceHost = new ServiceHost(this, adress);
            _ServiceHost.AddServiceEndpoint(contract, binding, String.Empty);
            _ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = IncludeExceptionDetailInFaults;
            _ServiceHost.Open();
            Log.Debug("Host is listening on", port);
        }

        protected virtual void HandleClientDisconnect(object sender, EventArgs e)
        {
            var connectant = GetClientFor(sender as IContextChannel);
            Log.Debug("Connection Lost to", connectant?.Alias, "-", connectant?.Id);
            if (connectant == null) return;
            _ConnectedClients.Remove(connectant);
            Broadcast(client => client.ClientDisconnected(connectant.Id));
        }

        protected virtual void Broadcast(Action<TClient> operation, Func<ConnectedClient<TClient>, bool> filter = null)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TClient>));
            if (filter == null) filter = c => true; 
            Parallel.ForEach(_ConnectedClients.Where(filter), client => //passt soweit aber üblicher enumerations bug bei disconnect
            {
                if (client.IsConnected) operation.Invoke(client.CallbackChannel as TClient); // todo: secure
            });
        }

        public virtual void StopServer()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(Server<TClient>));
            Broadcast(c => c.Message(Server.ID, "Server is shutting down"));

            Parallel.ForEach(_ConnectedClients, client => //passt soweit aber üblicher enumerations bug bei disconnect
            {
                if (client.IsConnected) client.Channel.Close(TimeSpan.FromMilliseconds(80));
            });

            _ServiceHost?.Close(TimeSpan.FromMilliseconds(100));
        }

        protected virtual string ValidateName(string name)
        {
            return name;
        }

        protected virtual int GetNextFreeClientID()
        {
            return ++_ClientIdProvider;
        }

        protected virtual bool IsConnected(int id)
        {
            return GetClientFor(id) != null;
        }
        protected virtual bool IsConnected(IContextChannel channel)
        {
            return GetClientFor(channel, true) != null;
        }

        protected virtual ConnectedClient<TClient> GetClientFor(int id)
        {
            return _ConnectedClients.FirstOrDefault(c => c.Id == id);
        }
        protected virtual ConnectedClient<TClient> GetClientFor(IContextChannel channel, Boolean skipValidation = false)
        {
            var client = _ConnectedClients.FirstOrDefault(c => c.Channel == channel);
            if (client == null && !skipValidation) channel.Abort(); // invalid connection detected -> kick
            return client;
        }

        // INTERFACE MEMBERS
        public virtual SubscriptionResult Subscribe(String name)
        {
            try
            {
                var channel = OperationContext.Current.Channel;
                if (!IsConnected(channel))
                {
                    channel.Closed += HandleClientDisconnect;
                    channel.Faulted += HandleClientDisconnect;

                    name = ValidateName(name);
                    var connectand = new ConnectedClient<TClient>(GetNextFreeClientID(), channel, OperationContext.Current.GetCallbackChannel<TClient>(), name);

                    Broadcast(client => client.ClientConnected(connectand.Id, name));
                    var users = _ConnectedClients.Cast<User>().ToArray();
                    _ConnectedClients.Add(connectand);
                    return new SubscriptionResult(connectand.Id, connectand.Alias, users);
                }

                Log.Warning("Client", name, "created a ghost connection");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (_ConnectedClients.Count == 0) throw;
            }
            return new SubscriptionResult();
        }

        public string ChangeUserName(string name)
        {
            var sender = GetClientFor(OperationContext.Current.Channel);
            if (sender == null) return null;
            sender.Alias = name;
            // TODO : update name on all connected clients
            return sender.Alias;
        }

        public virtual void Message(string msg)
        {
            var sender = GetClientFor(OperationContext.Current.Channel);
            if (sender != null) Broadcast(client => client.Message(sender.Id, msg));
        }
    }

    public class ConnectedClient<T> : User where T : class,IClient
    {
        public T CallbackChannel { get; private set; }
        public IContextChannel Channel { get; private set; }
        public Boolean IsConnected { get { return Channel.State == CommunicationState.Opened; } }

        internal ConnectedClient(int id, IContextChannel channel, T callbackChannel, String alias)
        {
            Id = id;
            Channel = channel;
            CallbackChannel = callbackChannel;
            Alias = alias;
        }
    }
}