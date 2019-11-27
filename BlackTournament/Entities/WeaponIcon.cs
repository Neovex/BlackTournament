using BlackCoat;
using BlackCoat.UI;
using SFML.Graphics;

namespace BlackTournament.Entities
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
                    Position = (4, 5),
                    Margin = (0, 0, 0, 3)
                },
                _Icon = new UIGraphic(core, icon)
                {
                    Position = (3, 4),
                    Visible = false
                },
                _Label = new Label(core, "0-0", 24, Game.DefaultFont)
                {
                    Position = (80, 7),
                    Margin = (0,0,6,0)
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