using System;
using System.Runtime.Serialization;

namespace BlackTournament.Net.Contract
{
    [DataContract]
    public class SubscriptionResult
    {
        [DataMember]
        public Boolean Success { get; set; }
        [DataMember]
        public Int32 Id { get; set; }
        [DataMember]
        public String Alias { get; set; }
        [DataMember]
        public User[] Users { get; set; }


        public SubscriptionResult()
        {
            Success = false;
            Id = Server.ERROR;
            Alias = null;
        }

        public SubscriptionResult(int id, string alias, User[] users)
        {
            Success = true;
            Id = id;
            Alias = alias;
            Users = users;
        }
    }
}