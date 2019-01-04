using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using BlackCoat;
using BlackCoat.UI;
using System.Net;
using SFML.Graphics;

namespace BlackTournament.UI
{
    class ServerInfo : Canvas
    {
        private static ServerInfo LastInfo;
        private SfxManager _Sfx;

        public override bool DockX { get => true; set => base.DockX = value; }
        public IPEndPoint Endpoint { get; private set; }

        public event Action<ServerInfo> Checked = i => { };
        public Action<ServerInfo> InitChecked { set => Checked += value; }


        public ServerInfo(Core core, SfxManager sfx, (IPEndPoint Endpoint, string Servername) serverData) : base(core)
        {
            _Sfx = sfx ?? throw new ArgumentNullException(nameof(sfx));
            Endpoint = serverData.Endpoint;
            CanFocus = true;
            Margin = new FloatRect(5, 5, 5, 0);

            Init = new UIComponent[]
            {
                new Label(_Core, serverData.Servername, 16, Game.DefaultFont),
                new AlignedContainer(_Core, Alignment.TopRight)
                {
                    Init = new UIComponent[]
                    {
                        new Label(_Core, serverData.Endpoint.ToString(), 16, Game.DefaultFont)
                    }
                }
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

        protected override void Destroy(bool disposing)
        {
            if (LastInfo == this) LastInfo = null;
            base.Destroy(disposing);
        }
    }
}