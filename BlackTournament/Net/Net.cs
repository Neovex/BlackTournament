using System;

namespace BlackTournament.Net
{
    public class Net
    {
        public const String DEFAULT_HOST = "localhost";
        public const Int32 DEFAULT_PORT = 2123;
        public const Single UPDATE_IMPULSE = 1f / 60;

        public static readonly Int32 ADMIN_ID = new Random().Next(Int32.MinValue, -1);
        public static readonly Commands<NetMessage> COMMANDS = new Commands<NetMessage>(NetMessage.Handshake, NetMessage.UserConnected, NetMessage.UserDisconnected);
    }
}