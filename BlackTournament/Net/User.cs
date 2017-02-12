using System;

namespace BlackTournament.Net
{
    public class User
    {
        public Int32 Id { get; private set; }
        public String Alias { get; set; }

        internal User(Int32 id, String alias)
        {
            Id = id;
            Alias = alias;
        }
    }
}