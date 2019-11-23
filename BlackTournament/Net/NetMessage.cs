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
        Init,
        Update,

        // Server Only
        ProcessGameAction,
        StopServer,
        Rotate
    }
}