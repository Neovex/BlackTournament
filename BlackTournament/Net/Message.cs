using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.Net
{
    public enum GameMessageType
    {
        Handshake,
        UserConnected,
        UserDisconnected,

        Message,
        LoadMap,
        UpdatePosition
    }
}