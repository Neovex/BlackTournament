using BlackCoat;
using BlackCoat.UI;
using SFML.Graphics;

namespace BlackTournament.Entities
{
    class WeaponIcon : UIContainer
    {
        private bool _Active;
        private int _Primary;
        private int _Secundary;
        private UIGraphic _Icon;
        private Label _Label;


        public bool Active
        {
            get => _Active;
            set { _Active = value; UpdateBackground(); }
        }
        public int Primary
        {
            get => _Primary;
            set { _Primary = value; UpdateLabel(); }
        }
        public int Secundary
        {
            get => _Secundary;
            set { _Secundary = value; UpdateLabel(); }
        }


        public WeaponIcon(Core core, Texture icon, Texture outline) : base(core)
        {
            _Background.OutlineThickness = 1;
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


        private void UpdateBackground()
        {
            _Background.OutlineThickness = Active ? 1 : 0;
            _Icon.Visible = Active;
        }
        private void UpdateLabel()
        {
            _Label.Text = $"{_Primary}-{_Secundary}";
        }
    }
}