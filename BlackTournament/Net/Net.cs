using System;
using BlackNet;

namespace BlackTournament.Net
{
    public static class Net
    {
        public const String DEFAULT_HOST = "localhost";
        public const Int32 DEFAULT_PORT = 21239;
        public const Single UPDATE_IMPULSE = 1f / 60;

        public static readonly Commands<NetMessage> COMMANDS = new Commands<NetMessage>(NetMessage.Handshake, NetMessage.UserConnected, NetMessage.UserDisconnected);
    }
}