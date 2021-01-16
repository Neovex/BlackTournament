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
        UpdatePlayer,

        // Server Only
        ProcessGameAction,
        RotatePlayer,
        StopServer
    }
}