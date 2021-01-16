using System;

using SFML.Graphics;
using SFML.System;

using BlackCoat;
using BlackCoat.UI;
using BlackCoat.AssetHandling;

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
                            Margin = new FloatRect(10,10,10,10),
                            InitReleased = b=> Visible = false
                        },
                        new BigButton(_Core, texLoader, sfx, "Quit")
                        {
                            Margin = new FloatRect(10,10,10,10),
                            InitReleased = b => Quit()
                        }
                    )
                );


            Texture = texLoader.Load(Files.Menue_Bg2);
            Texture.Repeated = true;
            TextureRect = new IntRect(new Vector2i(0, 0), InnerSize.ToVector2i());
        }
    }
}