using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.UI;

namespace BlackTournament.UI
{
    class WeaponIcon : UIContainer
    {
        private UIGraphic _Icon;
        private Label _Label;


        public WeaponIcon(Core core, Texture icon, Texture outline) : base(core)
        {
            _Background.OutlineThickness = 0;
            _Background.OutlineColor = new Color(255, 255, 0, 150);
            
            BackgroundColor = Color.Black;
            BackgroundAlpha = 0.35f;

            Add(
                new UIGraphic(core, outline)
                {
                    Position = new Vector2f(4, 5),
                    Margin = new FloatRect(0, 0, 0, 3)
                },
                _Icon = new UIGraphic(core, icon)
                {
                    Position = new Vector2f(3, 4),
                    Visible = false
                },
                _Label = new Label(core, "0-0", 24, Game.DefaultFont)
                {
                    Position = new Vector2f(80, 7),
                    Margin = new FloatRect(0,0,6,0)
                }
            );

            InvokeSizeChanged();
        }

        public void UpdateIcon(bool active, int primary, int secundary)
        {
            _Background.OutlineThickness = active ? 1 : 0;
            _Icon.Visible = primary + secundary != 0;
            _Label.Text = $"{primary}-{secundary}";
        }
    }
}