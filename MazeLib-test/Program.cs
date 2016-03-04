using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using MazeLib;

namespace MazeLib_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Maze maze1 = new Maze(10, 10, null, 5, 3);

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
        }
    }
}
