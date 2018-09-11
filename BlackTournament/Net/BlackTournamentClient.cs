using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using BlackCoat.Network;
using BlackTournament.Systems;
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


        public String MapName { get; private set; }
        public ClientPlayer Player { get; private set; }
        public float PlayerRotation { get; set; }

        public IEnumerable<ClientPlayer> Players => _PlayerLookup.Values;
        public IEnumerable<Pickup> Pickups => _PickupLookup.Values;
        public IEnumerable<Shot> Shots => _ShotLookup.Values;
        public IEnumerable<Effect> Effects => _Effects;
        public override Int32 AdminId => NetIdProvider.ADMIN_ID;


        // Connection Events
        public event Action OnConnect = () => { };
        public event Action OnDisconnect = () => { };

        // Game Events
        public event Action<ClientPlayer> UserJoined = u => { };
        public event Action<ClientPlayer> UserLeft = u => { };

        public event Action<Shot> ShotFired = u => { };
        public event Action<Shot> ShotRemoved = u => { };

        public event Action<ClientPlayer, String> MessageReceived = (u, m) => { };
        public event Action ChangeLevelReceived = () => { };
        public event Action UpdateReceived = () => { };



        public BlackTournamentClient(String userName):base(Game.ID, userName, Net.COMMANDS)
        {
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

        public void Disconnect()
        {
            Disconnect(String.Empty);
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

                case NetMessage.Init:
                    ServerInit(msg);
                break;

                case NetMessage.Update:
                    ServerUpdate(msg);
                break;
            }
        }

        private void TextMessage(NetIncomingMessage msg)
        {
            var player = _PlayerLookup[msg.ReadInt32()];
            var txt = msg.ReadString();
            Log.Info(player.Alias, txt);
            MessageReceived(player, txt);
        }

        private void ChangeLevel(NetIncomingMessage msg)
        {
            MapName = msg.ReadString();
            ChangeLevelReceived();
        }

        private void ServerInit(NetIncomingMessage msg)
        {
            // Add Players
            var entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var newPlayer = new ClientPlayer(msg.ReadInt32(), msg);
                if (newPlayer.Id != Id)
                {
                    newPlayer.Alias = GetAlias(newPlayer.Id);
                    _PlayerLookup.Add(newPlayer.Id, newPlayer);
                }
            }

            // Pickups
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var pickup = new Pickup(msg.ReadInt32(), msg);
                _PickupLookup.Add(pickup.Id, pickup);
            }

            // Shots
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var shot = new Shot(msg.ReadInt32(), msg);
                _ShotLookup.Add(shot.Id, shot);
            }

            // Effects
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                _Effects.Add(new Effect(msg.ReadInt32(), msg));
            }
        }

        private void ServerUpdate(NetIncomingMessage msg)
        {
            // Effect data are pure spawn info packets and exist only for a single frame hence need to be removed 
            _Effects.Clear();

            // Players
            var entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                var id = msg.ReadInt32();
                if(_PlayerLookup.TryGetValue(id, out ClientPlayer player))
                {
                    player.Deserialize(msg);
                }
                else
                {
                    // A new player has joined
                    var newPlayer = new ClientPlayer(id, msg)
                    {
                        Alias = GetAlias(id)
                    };
                    _PlayerLookup.Add(id, newPlayer);
                    UserJoined.Invoke(newPlayer);
                }
            }

            // Pickups
            entityCount = msg.ReadInt32();
            for (int i = 0; i < entityCount; i++)
            {
                _PickupLookup[msg.ReadInt32()].Deserialize(msg);
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

        protected override void UserConnected(NetUser user)
        {
            // Called from server when another user has connected to the current server
            // override is intentionally empty - user adding is handled in ServerUpdate(msg);
        }

        protected override void UserDisconnected(NetUser user)
        {
            UserLeft.Invoke(_PlayerLookup[user.Id]);
            _PlayerLookup.Remove(user.Id);
        }

        protected override void ConnectionValidated(int id, string alias)
        {
            Player = new ClientPlayer(id) { Alias = alias };
            _PlayerLookup = new Dictionary<int, ClientPlayer>();
            _PlayerLookup.Add(id, Player);

            _PickupLookup = new Dictionary<int, Pickup>();
            _ShotLookup = new Dictionary<int, Shot>();
            _Effects = new List<Effect>();

            OnConnect.Invoke();
        }

        protected override void Connected()
        {
            // Not used. A connection is communicated only after validation. See ConnectionValidated(int id, string alias)
        }
        protected override void Disconnected()
        {
            base.Disconnected();
            OnDisconnect.Invoke();
        }
    }
}