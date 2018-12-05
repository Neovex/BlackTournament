using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.UI;

namespace BlackTournament.UI
{
    public class BigButton : Button
    {
        private static readonly Color NormalColor = new Color(150, 150, 0);
        private static readonly Color ActiveColor = new Color(50, 250, 0);

        private TextureLoader _Loader;
        private Label _Label;


        public BigButton(Core core, TextureLoader loader, String text) : base(core, null)
        {
            _Loader = loader ?? throw new ArgumentNullException(nameof(loader));
            Texture = _Loader.Load(Files.Menue_Button, false, true);
            SetSize(Texture.Size.ToVector2f());

            Init = new UIComponent[]
            {
                new AnchoredContainer(_Core, Anchor.Center)
                {
                    Init = new UIComponent[]
                    {
                        _Label = new Label(_Core, text, Game.DefaultFont)
                        {
                            CharacterSize = 18,
                            TextColor = NormalColor,
                            Padding = new FloatRect(0,0,0,3)
                        }
                    }
                }
            };
        }

        protected override void InvokeFocusGained()
        {
            _Label.TextColor = ActiveColor;
            base.InvokeFocusGained();
        }

        protected override void InvokeFocusLost()
        {
            _Label.TextColor = NormalColor;
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
            Texture = _Loader.Load(Files.Menue_Button, false, true);
            base.InvokeReleased();
        }
    }
}