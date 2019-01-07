using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using BlackCoat;
using BlackCoat.Network;
using BlackTournament.Tmx;
using BlackTournament.Systems;
using BlackTournament.Net.Data;
using System.Net;

namespace BlackTournament.Net
{
    class BlackTournamentServer : ManagedServer<NetMessage>
    {
        private Core _Core;
        private GameLogic _Logic;
        private Single _UpdateImpulse;


        public string LastError { get; private set; }
        public override int AdminId => NetIdProvider.ADMIN_ID;
        public override int NextClientId => NetIdProvider.NEXT_ID;

        public ServerInfo Info { get; set; }


        public BlackTournamentServer(Core core) : base(Game.ID, Net.COMMANDS)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
        }


        // CONTROL
        private bool IsAdmin(int id) => id == AdminId;

        public bool HostGame(int port, ServerInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Log.Debug("Trying to host", info.Name, "on", port);
            var map = LoadMapFromMapname(info.Map);
            if (map == null)
            {
                LastError = $"Failed to host {info.Map} on {port} reason: unknown map";
                Log.Error(LastError);
                return false;
            }

            Host(info.Name, port);
            Info = info;
            Log.Debug("Host active");
            ChangeLevel(map);
            return true;
        }

        private TmxMapper LoadMapFromMapname(string mapName)
        {
            var mapper = new TmxMapper(() => NextClientId);
            if (mapper.Load(mapName, _Core.CollisionSystem)) return mapper;
            LastError = $"Failed to load level {mapName}";
            Log.Warning(LastError);
            return null;
        }

        private void ChangeLevel(TmxMapper map)
        {
            if (_Logic != null) _Logic.PlayerGotPickup -= SendPlayerPickup;

            _Logic = new GameLogic(_Core, map);
            _Logic.PlayerGotPickup += SendPlayerPickup;

            Info.Map = map.Name;

            // Add users
            foreach (var user in ConnectedUsers) _Logic.AddPlayer(user);
            Broadcast(NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
        }

        // Update from core->game->server.Update()
        internal void Update(float deltaT)
        {
            // Process Incoming Net Traffic
            ProcessMessages();

            if (_Logic == null) return;

            // Update Server Data
            _Logic.Update(deltaT);

            // Update Client Data (~60Hz)
            _UpdateImpulse += deltaT;
            if (_UpdateImpulse >= Net.UPDATE_IMPULSE)
            {
                _UpdateImpulse = 0;
                Broadcast(NetMessage.Update, m => _Logic.Serialize(m, false), NetDeliveryMethod.UnreliableSequenced);
            }
        }

        protected override void UserConnected(ServerUser<NetConnection> user)
        {
            _Logic.AddPlayer(user);
            Send(user.Connection, NetMessage.ChangeLevel, m => m.Write(_Logic.MapName));
            Send(user.Connection, NetMessage.Init, m => _Logic.Serialize(m, true));
        }

        protected override void UserDisconnected(ServerUser<NetConnection> user)
        {
            _Logic.RemovePlayer(user);
        }

        // INCOMMING
        protected override void ProcessIncommingData(NetMessage subType, NetIncomingMessage msg)
        {
            switch (subType)
            {
                case NetMessage.TextMessage:
                    SendMessage(msg.ReadInt32(), msg.ReadString());
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
        protected override void HandleDiscoveryRequest(NetOutgoingMessage msg)
        {
            Info.Serialize(msg); // respond with server info
        }


        // OUTGOING
        private void SendPlayerPickup(ServerUser<NetConnection> user, PickupType pickup)
        {
            Send(user.Connection, NetMessage.Pickup, m => m.Write((int)pickup));
        }


        // MAPPED HANDLERS
        private void SendMessage(int id, string message)
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
                StopServer(String.Empty);
                Info = null;
                // TODO : check if dropped players will be removed by other events or if this needs to be done here manually
                _Logic.PlayerGotPickup -= SendPlayerPickup;
                _Logic = null;
            }
        }

        private void ChangeLevel(int id, string mapName)
        {
            if (IsAdmin(id))
            {
                var map = LoadMapFromMapname(mapName);
                if (map != null) ChangeLevel(map);
            }
            else
            {
                Log.Warning("Blocked change level request from", id, _ConnectedClients.FirstOrDefault(c => c.Id == id)?.Alias, "to level", mapName);
            }
        }
    }
}