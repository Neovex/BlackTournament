using System;

namespace BlackTournament.Net.Lid
{
    public class User<TChannel> where TChannel : class
    {
        public Int32 Id { get; private set; }
        public TChannel Connection { get; private set; }
        public String Alias { get; set; }

        internal User(Int32 id, TChannel channel, String alias)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            Id = id;
            Connection = channel;
            Alias = alias;
        }
    }
}