using System;

namespace BlackTournament.Net.Lid
{
    public class ServerUser<TChannel> : ClientUser where TChannel : class
    {
        public TChannel Connection { get; private set; }

        internal ServerUser(Int32 id, TChannel channel, String alias) : base(id, alias)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            Connection = channel;
        }
    }
}