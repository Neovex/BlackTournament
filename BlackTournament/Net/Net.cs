using System;

namespace BlackTournament.Net
{
    public class Net
    {
        public static readonly Int32 ADMIN_ID = new Random().Next(Int32.MinValue, -1);
        public static readonly Commands<NetMessage> COMMANDS = new Commands<NetMessage>(NetMessage.Handshake, NetMessage.UserConnected, NetMessage.UserDisconnected);
    }
}