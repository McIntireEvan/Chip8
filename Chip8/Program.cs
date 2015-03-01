#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Chip8
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Chip8())
                game.Run();
        }
    }
#endif
}