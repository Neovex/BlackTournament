using System;
using System.Runtime.Serialization;

namespace BlackTournament.Net.Contract
{
    [DataContract]
    public class User
    {
        [DataMember]
        public Int32 Id { get; set; }
        [DataMember]
        public String Alias { get; set; }

        public User()
        {
        }

        public User(Int32 id, String alias)
        {
            Id = id;
            Alias = alias;
        }
    }
}