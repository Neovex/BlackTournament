using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using BlackCoat;
using BlackCoat.Network;
using BlackTournament.Tmx;
using BlackTournament.InputMaps;
using BlackTournament.Net.Data;

namespace BlackTournament.Net
{
    class BlackTournamentServer : ManagedServer<NetMessage>
    {
        private Core _Core;
        private GameLogic _Logic;
        private Single _UpdateImpulse;


        public ServerInfo Info { get; private set; }


        public BlackTournamentServer(Core core) : base(Game.ID, Net.COMMANDS)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
        }


        // CONTROL
        private bool IsAdmin(int id) => id == AdminId;
        protected override string ValidateName(string name) => name == "System" ? "Banana" + _Core.Random.Next() : base.ValidateName(name);

        public bool HostGame(int port, ServerInfo info)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            Log.Debug("Trying to host", Info.Name, "on", port);
            var map = LoadMapFromMapname(Info.Map);
            if (map == null)
            {
                LastError = $"Failed to host {Info.Map} on {port} reason: unknown map";
                Log.Error(LastError);
            }
            else if (Host(Info.Name, port))
            {
                Log.Debug("Host active");
                ChangeLevel(map);
                return true;
            }
            return false;
        }

        private TmxMapper LoadMapFromMapname(string mapName)
        {
            var mapper = new TmxMapper();
            if (mapper.Load(mapName, _Core.CollisionSystem)) return mapper;
            LastError = $"Failed to load level {mapName}";
            Log.Warning(LastError);
            return null;
        }

        private void ChangeLevel(TmxMapper map)
        {
            // Init Game Logic
            if(_Logic != null) _Logic.GameMessage -= HandleGameLogicMessage;
            _Logic = new GameLogic(_Core, map);
            _Logic.GameMessage += HandleGameLogicMessage;

            // Add users
            foreach (var user in ConnectedUsers) _Logic.AddPlayer(user);
            Broadcast(NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
        }

        // Update from core->game->server.Update()
        internal void Update(float deltaT)
        {
            // Process Incoming Net Traffic
            ProcessMessages();

            if (!Running) return;

            // Update Server Data
            _Logic.Update(deltaT);

            // Update Client Data (~60Hz)
            _UpdateImpulse += deltaT;
            if (_UpdateImpulse >= Net.UPDATE_IMPULSE)
            {
                _UpdateImpulse = 0;
                Broadcast(NetMessage.Update, m => _Logic.Serialize(m, false), NetDeliveryMethod.UnreliableSequenced);
                /* Check: to save traffic we could not send weapon info to all players
                foreach (var user in ConnectedUsers)
                {
                   Send(user.Connection, NetMessage.UpdatePlayer, m => _Logic.SerializePlayer(m, user.Id), NetDeliveryMethod.UnreliableSequenced); 
                }
                */
            }

            if (_Logic.MatchCicle == MatchCicle.Complete)
            {
                StopServer("Match Complete");
            }
        }
        private void HandleGameLogicMessage(string msg)
        {
            BroadcastTextMessage(ServerId, msg);
        }

        protected override void UserConnected(ServerUser<NetConnection> user)
        {
            Info.CurrentPlayers++;
            _Logic.AddPlayer(user);
            Send(user.Connection, NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
            Send(user.Connection, NetMessage.Init, m => _Logic.Serialize(m, true));
        }

        protected override void UserDisconnected(ServerUser<NetConnection> user)
        {
            Info.CurrentPlayers--;
            _Logic.RemovePlayer(user);
        }

        // INCOMMING
        protected override void HandleDiscoveryRequest(NetOutgoingMessage msg)
        {
            if (Running) Info.Serialize(msg); // respond with server info
        }
        protected override void ProcessIncommingData(NetMessage subType, NetIncomingMessage msg)
        {
            switch (subType)
            {
                case NetMessage.TextMessage:
                    BroadcastTextMessage(msg.ReadInt32(), msg.ReadString());
                    break;

                case NetMessage.ChangeLevel:
                    ChangeLevel(msg.ReadInt32(), msg.ReadString());
                    break;

                case NetMessage.ProcessGameAction:
                    _Logic.ProcessGameAction(msg.ReadInt32(), (GameAction)msg.ReadInt32(), msg.ReadBoolean());
                    break;

                case NetMessage.Rotate:
                    _Logic.RotatePlayer(msg.ReadInt32(), msg.ReadSingle());
                    break;

                case NetMessage.StopServer:
                    StopServer(msg.ReadInt32());
                    break;
            }
        }

        // MESSAGE HANDLERS
        private void BroadcastTextMessage(int id, string message)
        {
            Broadcast(NetMessage.TextMessage, m =>
            {
                m.Write(id);
                m.Write(message);
            });
        }

        private void StopServer(int id)
        {
            if (IsAdmin(id))
            {
                _Logic.GameMessage -= HandleGameLogicMessage;
                StopServer("Server stopped by admin");
            }
            else
            {
                var message = $"Blocked stop server command from {id}: {_ConnectedClients.FirstOrDefault(c => c.Id == id)?.Alias ?? null}";
                Log.Warning(message);
                Send(_ConnectedClients.First(c => c.Id == AdminId).Connection, NetMessage.TextMessage, m =>
                {
                    m.Write(ServerId);
                    m.Write(message);
                });
            }
        }

        private void ChangeLevel(int id, string mapName)
        {
            if (IsAdmin(id))
            {
                var map = LoadMapFromMapname(mapName);
                if (map != null) ChangeLevel(map);
                else
                {
                    LastError = $"Could comply to admin command: change level {mapName}";
                    Log.Error(LastError);
                    Send(_ConnectedClients.First(c => c.Id == AdminId).Connection, NetMessage.TextMessage, m =>
                    {
                        m.Write(ServerId);
                        m.Write(LastError);
                    });
                }
            }
            else
            {
                var message = $"Blocked change level command from {id}: {_ConnectedClients.FirstOrDefault(c => c.Id == id)?.Alias ?? null} to level {mapName}";
                Log.Warning(message);
                Send(_ConnectedClients.First(c => c.Id == AdminId).Connection, NetMessage.TextMessage, m =>
                {
                    m.Write(ServerId);
                    m.Write(message);
                });
            }
        }
    }
}