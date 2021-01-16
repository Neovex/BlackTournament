using System;
using System.Linq;

namespace BlackTournament
{
    public static class Programm
    {
        private static Game _BlackTournament;

        [STAThread] // Required for the Light-Editors Save/Load dialog
        public static void Main(string[] args)
        {
            _BlackTournament = new Game();
#if !DEBUG
            Failsafe.Enable(e => Log.Fatal(e));
#endif
            _BlackTournament.Run(String.Join(" ", args ?? Enumerable.Empty<String>()));
            _BlackTournament = null;
        }
    }
}