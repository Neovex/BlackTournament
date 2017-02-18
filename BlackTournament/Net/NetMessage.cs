using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.Net
{
    public enum NetMessage
    {
        // Server/Client Infrastructure
        Handshake,
        UserConnected,
        UserDisconnected,

        // 2 Way
        TextMessage,
        ChangeLevel,

        // Client Only
        Update,

        // Server Only
        ProcessGameAction,
        StopServer
    }
}