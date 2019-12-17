using BlackCoat;
using BlackCoat.UI;
using System;
using System.Collections.Generic;
using System.Linq;

using SFML.Graphics;

namespace BlackTournament.UI
{
    class InGameMenu : AlignedContainer
    {
        public event Action Quit = () => { };

        public InGameMenu(Core core, TextureLoader texLoader, SfxManager sfx) : base(core, Alignment.Center)
        {
            Visible = false;
            _Background.OutlineColor = Color.Red;
            _Background.OutlineThickness = 1;

            BackgroundColor = Color.Black;
            BackgroundAlpha = 0.6f;

            Add(new OffsetContainer(_Core, Orientation.Vertical, 5,
                        new BigButton(_Core, texLoader, sfx, "Resume")
                        {
                            Margin = (10,10,10,10),
                            InitReleased = b=> Visible = false
                        },
                        new BigButton(_Core, texLoader, sfx, "Quit")
                        {
                            Margin = (10,10,10,10),
                            InitReleased = b => Quit()
                        }
                    )
                );


            Texture = texLoader.Load(Files.Menue_Bg2);
            Texture.Repeated = true;
            TextureRect = new IntRect((0, 0), InnerSize.ToVector2i());
        }
    }
}