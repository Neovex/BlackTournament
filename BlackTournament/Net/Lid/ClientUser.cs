using System;

namespace BlackTournament.Net.Lid
{
    public class ClientUser
    {
        public Int32 Id { get; private set; }
        public String Alias { get; set; }

        internal ClientUser(Int32 id, String alias)
        {
            Id = id;
            Alias = alias;
        }
    }
}