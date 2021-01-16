using BlackCoat;
using BlackCoat.UI;
using SFML.System;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackTournament.UI
{
    class ComboBoxDialog : OffsetContainer // TODO : move to engine
    {
        public event Action<String> ItemSelected = s => { };


        private TextBox _Target;


        public Color HighlightColor { get; set; }

        public ComboBoxDialog(Core core, TextBox target, IEnumerable<String> items) : base(core, Orientation.Vertical)
        {
            _Target = target;

            _Background.OutlineColor = Color.Black;
            _Background.OutlineThickness = 1;
            BackgroundColor = Color.White;
            HighlightColor =  Color.Red;

            foreach (var item in items)
            {
                var b = new Button(_Core, _Target.InnerSize)
                {
                    InitReleased = ButtonClicked,
                    InitFocusGained = ButtonFocusGained,
                    InitFocusLost = ButtonFocusLost,
                    Tag = item,
                    Init = new []
                    {
                        new AlignedContainer(_Core, Alignment.CenterLeft)
                        {
                            Init = new UIComponent[]
                            {
                                new Label(_Core, item, font:Game.DefaultFont)
                                {
                                    CharacterSize = target.CharacterSize,
                                    
                                    TextColor = Color.Black,
                                    Padding = new FloatRect(5,0,0,0)
                                }
                            }
                        }
                    }
                };
                Add(b);
            }
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            Position = _Target.GlobalPosition + new Vector2f(0, _Target.InnerSize.Y);
        }

        private void ButtonClicked(UIComponent button)
        {
            button.CloseDialog();
            _Target.Text = button.Tag.ToString();
            ItemSelected.Invoke(button.Tag.ToString());
        }

        private void ButtonFocusGained(UIComponent button)
        {
            button.BackgroundColor = HighlightColor;
        }
        private void ButtonFocusLost(UIComponent button)
        {
            button.BackgroundColor = BackgroundColor;
        }
    }
}
