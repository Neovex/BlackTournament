using System;

namespace BlackTournament.Net.Server
{
    public class ServerUser<TChannel> : User where TChannel : class
    {
        public TChannel Connection { get; private set; }

        internal ServerUser(Int32 id, TChannel channel, String alias) : base(id, alias)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            Connection = channel;
        }
    }
}