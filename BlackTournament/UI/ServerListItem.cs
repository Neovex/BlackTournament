using System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.UI;
using BlackCoat.AssetHandling;
using BlackTournament.Net.Data;

namespace BlackTournament.UI
{
    class ServerListItem : DistributionContainer
    {
        private static ServerListItem LastInfo;
        private SfxManager _Sfx;

        public ServerInfo ServerInfo { get; private set; }

        public event Action<ServerListItem> Checked = i => { };
        public Action<ServerListItem> InitChecked { set => Checked += value; }


        public ServerListItem(Core core, SfxManager sfx, ServerInfo serverInfo) : base(core, Orientation.Horizontal)
        {
            _Sfx = sfx ?? throw new ArgumentNullException(nameof(sfx));
            ServerInfo = serverInfo;
            CanFocus = true;
            Margin = new FloatRect(5, 5, 5, 0);

            Init = new UIComponent[]
            {
                new Label(_Core, serverInfo.Name, 16, Game.DefaultFont),
                new Label(_Core, serverInfo.Map, 16, Game.DefaultFont),
                new Label(_Core, $"{serverInfo.CurrentPlayers} / {serverInfo.MaxPlayers}", 16, Game.DefaultFont),
                new Label(_Core, $"{serverInfo.Ping} ms", 16, Game.DefaultFont),
                new Label(_Core, serverInfo.EndPoint.ToString(), 16, Game.DefaultFont)
            };
            ResizeToFitContent();
        }

        protected override void InvokeFocusGained()
        {
            _Sfx.Play(Files.Sfx_Highlight);
            BackgroundAlpha = 0.5f;
            base.InvokeFocusGained();
        }
        protected override void InvokeFocusLost()
        {
            if (LastInfo != this) BackgroundAlpha = 0;
            base.InvokeFocusLost();
        }

        private void Uncheck()
        {
            BackgroundAlpha = 0;
        }

        protected override void HandleInputConfirm()
        {
            _Sfx.Play(Files.Sfx_Highlight);
            LastInfo?.Uncheck();
            LastInfo = LastInfo == this ? null : this;
            Checked.Invoke(LastInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (LastInfo == this) LastInfo = null;
            base.Dispose(disposing);
        }
    }
}