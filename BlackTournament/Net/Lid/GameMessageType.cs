using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.Net.Lid
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