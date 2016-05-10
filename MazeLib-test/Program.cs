using System;
using System.Diagnostics;

using MazeLib;

namespace MazeLib_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Maze maze1 = new Maze(5, 5, null, 0, 0);

            maze1.GenerateTWMaze_GrowingTree();
            maze1.dumpMaze();

            // better integration for it, like generate maze1.blockVersion array
            // + display
            Byte[,] blockmaze = maze1.LineToBlock();


            for (UInt16 y = 0; y < blockmaze.GetLength(1); y++)
            {
                string xline = string.Empty;

                for (UInt16 x = 0; x < blockmaze.GetLength(0); x++)
                {
                    xline += ' ' + blockmaze[x, y].ToString();
                }

                Debug.Print(string.Format("BM[{0}]={1}", y, xline));
            }

            Byte[,] bigm = maze1.scaleMaze(3);

            for (UInt16 y = 0; y < bigm.GetLength(1); y++)
            {
                string xline = string.Empty;

                for (UInt16 x = 0; x < bigm.GetLength(0); x++)
                {
                    xline += ' ' + bigm[x, y].ToString();
                }

                Debug.Print(string.Format("BigM[{0}]={1}", y, xline));
            }
        }
    }
}
