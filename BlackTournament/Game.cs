using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;

namespace BlackTournament
{
    public class Game
    {
        public static void Main(string[] args)
        {
            var core = new Core(Core.DefaultDevice, true);
            core.Run();
        }
    }
}