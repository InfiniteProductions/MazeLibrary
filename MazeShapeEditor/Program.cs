using System;

namespace MazeShapeEditor
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new MazeEditor())
                game.Run();
        }
    }
#endif
}
