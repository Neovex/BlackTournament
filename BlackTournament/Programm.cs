using System;
using System.Linq;

namespace BlackTournament
{
    public static class Programm
    {
        [STAThread] // Required for the Light-Editors Save/Load dialog
        public static void Main(string[] args)
        {
            var blackTournament = new Game();
#if !DEBUG
            Failsafe.Enable(e => Log.Fatal(e));
#endif
            blackTournament.Run(String.Join(" ", args ?? Enumerable.Empty<String>()));
            blackTournament = null;
        }
    }
}