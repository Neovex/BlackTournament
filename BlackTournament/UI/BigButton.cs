using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.UI;
using BlackTournament.Properties;

namespace BlackTournament.UI
{
    public class BigButton : Button
    {
        private static readonly Color NormalColor = new Color(150, 150, 0);
        private static readonly Color ActiveColor = new Color(50, 250, 0);

        private TextureLoader _Loader;
        private SfxManager _Sfx;
        private Label _Label;


        public BigButton(Core core, TextureLoader loader, SfxManager sfx, String text) : base(core, null)
        {
            _Loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _Sfx = sfx ?? throw new ArgumentNullException(nameof(sfx));
            Texture = _Loader.Load(Files.Menue_Button, false, true);
            Resize(Texture.Size.ToVector2f());
            _Sfx.AddToLibrary(Files.Sfx_Highlight, Settings.Default.SfxVolume);
            _Sfx.AddToLibrary(Files.Sfx_Select, Settings.Default.SfxVolume);

            Init = new UIComponent[]
            {
                new AlignedContainer(_Core, Alignment.Center)
                {
                    Init = new UIComponent[]
                    {
                        _Label = new Label(_Core, text, font:Game.DefaultFont)
                        {
                            CharacterSize = 18,
                            TextColor = NormalColor,
                            Margin = new FloatRect(0,0,0,5)
                        }
                    }
                }
            };
        }

        protected override void InvokeFocusGained()
        {
            _Sfx.Play(Files.Sfx_Highlight);
            _Label.TextColor = ActiveColor;
            _Label.Style = Text.Styles.Bold;
            base.InvokeFocusGained();
        }

        protected override void InvokeFocusLost()
        {
            _Label.TextColor = NormalColor;
            _Label.Style = Text.Styles.Regular;
            Texture = _Loader.Load(Files.Menue_Button, false, true);
            base.InvokeFocusLost();
        }

        protected override void InvokePressed()
        {
            Texture = _Loader.Load(Files.Menue_Button_Active, false, true);
            base.InvokePressed();
        }

        protected override void InvokeReleased()
        {
            _Sfx.Play(Files.Sfx_Select);
            Texture = _Loader.Load(Files.Menue_Button, false, true);
            _Core.AnimationManager.Wait(0.6f, base.InvokeReleased);
        }
        protected override void InvokeEnabledChanged()
        {
            Alpha = Enabled ? 1 : 0.5f;
            base.InvokeEnabledChanged();
        }
    }
}