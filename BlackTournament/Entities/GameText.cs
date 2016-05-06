using BlackCoat.Entities;
using BlackCoat;

namespace BlackTournament.Entities
{
    public class GameText : TextItem
    {
        public GameText(Core core):base(core)
        {
            Font = Game.DefaultGameFont;
        }
    }
}