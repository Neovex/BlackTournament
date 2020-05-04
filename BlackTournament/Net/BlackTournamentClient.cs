using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using BlackCoat.Network;
using BlackTournament.InputMaps;
using BlackTournament.Net.Data;

namespace BlackTournament.Net
{
    public class BlackTournamentClient : ManagedClient<NetMessage>
    {
        private Dictionary<int, ClientPlayer> _PlayerLookup;
        private Dictionary<int, Pickup> _PickupLookup;
        private Dictionary<int, Shot> _ShotLookup;
        private List<Effect> _Effects;
        private Single _UpdateImpulse;


        public float GameTime { get; private set; }
        public String MapName { get; private set; }
        public ClientPlayer Player { get; private set; }
        public float PlayerRotation { get; set; }


        public int PlayerCount => _PlayerLookup.Count;
        public IEnumerable<ClientPlayer> Players => _PlayerLookup.Values;
        public IEnumerable<Pickup> Pickups => _PickupLookup.Values;
        public IEnumerable<Shot> Shots => _ShotLookup.Values;
        public IEnumerable<Effect> Effects => _Effects;


        // Connection Events
        public event Action OnConnect = () => { };
        public event Action OnDisconnect = () => { };

        // Game Events
        public event Action<ClientPlayer> UserJoined = u => { };
        public event Action<ClientPlayer> UserLeft = u => { };

        public event Action<Shot> ShotFired = u => { };
        public event Action<Shot> ShotRemoved = u => { };

        public event Action<Boolean, String> MessageReceived = (u, m) => { };
        public event Action ChangeLevelReceived = () => { };
        public event Action UpdateReceived = () => { };



        public BlackTournamentClient(String userName):base(Game.ID, userName, Net.COMMANDS)
        {
            _PlayerLookup = new Dictionary<int, ClientPlayer>();
            _PickupLookup = new Dictionary<int, Pickup>();
            _ShotLookup = new Dictionary<int, Shot>();
            _Effects = new List<Effect>();
        }


        // CONTROL
        internal void Update(float deltaT)
        {
            // Process incoming data
            ProcessMessages();

            if (IsConnected)
            {
                // Update Rotation on Server (~60Hz)
                _UpdateImpulse += deltaT;
                if (_UpdateImpulse >= Net.UPDATE_IMPULSE && Player != null)
                {
                    _UpdateImpulse = 0;
                    UpdatePlayerRotation(NetDeliveryMethod.UnreliableSequenced);
                }
            }
        }

        public void Disconnect()
        {
            Disconnect(String.Empty);
        }

        internal String GetAlias(int id)
        {
            return _ConnectedClients.First(u => u.Id == id).Alias;
        }


        // OUTGOING
        public void ProcessGameAction(GameAction action, Boolean activate)
        {
            var method = NetDeliveryMethod.UnreliableSequenced;
            if(action == GameAction.ShootPrimary || action == GameAction.ShootSecundary)
            {
                method = NetDeliveryMethod.ReliableOrdered;
                UpdatePlayerRotation(method);
            }
            Send(NetMessage.ProcessGameAction, m => { m.Write(Id); m.Write((int)action); m.Write(activate); }, method);
        }

        private void UpdatePlayerRotation(NetDeliveryMethod method)
        {
            Send(NetMessage.Rotate, m => { m.Write(Id); m.Write(PlayerRotation); }, method);
        }

        public void SendMessage(String txt)
        {
            Send(NetMessage.TextMessage, m => { m.Write(Id); m.Write(txt); });
        }

        public void StopServer()
        {
            Send(NetMessage.StopServer, m => m.Write(Id));
        }


        // INCOMMING
        protected override void DataReceived(NetMessage message, NetIncomingMessage msg)
        {
            switch (message)
            {
                case NetMessage.TextMessage:
                    TextMessage(msg);
                break;

                case NetMessage.ChangeLevel:
                    ChangeLevel(msg);
                break;

                case NetMessage.Update:
                    ServerUpdate(msg);
                break;

                case NetMessage.UpdatePlayer:
                    _PlayerLookup[msg.ReadInt32()].Deserialize(msg);
                break;
            }
        }

        private void TextMessage(NetIncomingMessage msg)
        {
            var id = msg.ReadInt32();
            var message = msg.ReadString();
            _PlayerLookup.TryGetValue(id, out ClientPlayer player);
            if (player != null) message = $"{player.Alias}: {message}";
            MessageReceived(id == ServerId, message);
        }

        private void ChangeLevel(NetIncomingMessage msg)
        {
            MapName = msg.ReadString();
            ChangeLevelReceived();
        }

        private void ServerUpdate(NetIncomingMessage msg)
        {
            // Time
            GameTime = msg.ReadSingle();

            // Effect data are pure spawn info packets and exist only for a single frame hence need to be removed 
            _Effects.Clear();

            // Players
            var entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                _PlayerLookup[msg.ReadInt32()].Deserialize(msg);
            }

            // Pickups
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var id = msg.ReadInt32();
                if (_PickupLookup.TryGetValue(id, out Pickup pickup))
                {
                    pickup.Deserialize(msg);
                }
                else
                {
                    pickup = new Pickup(id, msg);
                    _PickupLookup.Add(pickup.Id, pickup);
                }
            }

            // Shots
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var id = msg.ReadInt32();
                if (_ShotLookup.TryGetValue(id, out Shot shot))
                {
                    shot.Deserialize(msg);
                    if (!shot.Alive)
                    {
                        _ShotLookup.Remove(shot.Id);
                        ShotRemoved.Invoke(shot);
                    }
                }
                else
                {
                    shot = new Shot(id, msg);
                    if (shot.Alive)
                    {
                        _ShotLookup.Add(shot.Id, shot);
                        ShotFired.Invoke(shot);
                    }
                }
            }

            // Effects
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                _Effects.Add(new Effect(msg.ReadInt32(), msg));
            }

            // Inform client game logic that an update has been received
            UpdateReceived.Invoke();
        }

        protected override void ConnectionValidated(int id, string alias) // We successfully connected to a server
        {
            Player = new ClientPlayer(id, alias);
            _PlayerLookup.Add(id, Player);
            OnConnect.Invoke();
        }

        protected override void Disconnected() // We got disconnected from the server
        {
            OnDisconnect.Invoke();
            _PlayerLookup.Clear();
            _PickupLookup.Clear();
            _ShotLookup.Clear();
            _Effects.Clear();
        }

        protected override void UserConnected(NetUser user) // Another user connected to the server we are connected to
        {
            var newPlayer = new ClientPlayer(user.Id, user.Alias);
            _PlayerLookup.Add(user.Id, newPlayer);
            UserJoined.Invoke(newPlayer);
        }

        protected override void UserDisconnected(NetUser user) // Someone disconnected from the server we are connected to
        {
            UserLeft.Invoke(_PlayerLookup[user.Id]);
            _PlayerLookup.Remove(user.Id);
        }

        protected override void DiscoveryResponseReceived(NetIncomingMessage msg)
        {
            // A game client does not handle network discovery
        }
    }
}