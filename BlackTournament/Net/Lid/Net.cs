using System;

namespace BlackTournament.Net.Lid
{
    public class Net
    {
        public static readonly Int32 ADMIN_ID = new Random().Next(int.MinValue, -1);
        public static readonly Commands<GameMessageType> COMMANDS = new Commands<GameMessageType>(GameMessageType.Handshake, GameMessageType.UserConnected, GameMessageType.UserDisconnected);
    }
}