using System;

namespace TicTacToe_GridExample
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TicTacToe())
                game.Run();
        }
    }
#endif
}
