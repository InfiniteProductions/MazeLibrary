using System;
using System.Collections.Generic;

using System.Diagnostics;


// ~random "holes" in maze shape

namespace MazeLib
{
    public enum PickMethod : byte { Newest, Oldest, Random, Cyclic, Kit, Collapse };

    [Flags]
    public enum Direction : byte { North = 0x1, West = 0x2, South = 0x4, East = 0x8 };


    public class Maze
    {
        public Byte[,] maze { get; private set; }
        public Byte[,] blockmaze { get; private set; }

        public UInt16 width { get; private set; }
        public UInt16 length { get; private set; }

        public Byte holes { get; private set; }
        public Byte holesMaxRadius { get; private set; }

        public Byte[] spawnpoint { get; private set; }
        public Byte[] exitpoint { get; private set; }

        public Layout layout { get; private set; }

        public Random random = new Random((int)DateTime.Now.Ticks & (0x0000FFFF));

        public PickMethod pickMethod { get; private set; }

        private UInt16 CyclePick = 0;
        private bool KitCycle = true;   // true = increase, false = decrease

        // create a random maze, with a starting position eventually
        public Maze(UInt16 width, UInt16 length, Layout layout = null, UInt16 startx = 0, UInt16 starty = 0, PickMethod pimet = PickMethod.Newest, Byte holes = 0, byte maxradius = 0)
        {
            Debug.Print(string.Format("C: W={0} L={1}", width, length));

            this.width = width;
            this.length = length;

            this.layout = layout;

            this.holes = holes;
            this.holesMaxRadius = maxradius;

            pickMethod = pimet;

           maze = BuildBaseMaze(this.width, this.length, this.layout, this.holes, this.holesMaxRadius);
           
        }


        public void Reset()
        {
            maze = BuildBaseMaze(width, length, layout, this.holes, this.holesMaxRadius);
        }


        public void UpdateSize(UInt16 width, UInt16 length, Layout layout = null, byte holes = 0, byte maxradius = 0)
        {
            if (width <= 0)
                width = this.width;

            if (length <= 0)
                length = this.length;

            this.width = width;
            this.length = length;
            this.layout = layout;
            this.holes = holes;
            this.holesMaxRadius = maxradius;

            maze = null;
            GC.Collect(0);

            maze = BuildBaseMaze(this.width, this.length, this.layout, this.holes, this.holesMaxRadius);
        }


        // layout here = as now part of the class => remove parameter or update class property
        // "holes" : layout.addholes(number, maxradius)
        public Byte[,] BuildBaseMaze(UInt16 width, UInt16 length, Layout _layout = null, Byte holes = 0, Byte radius = 0)
        {
            Byte[,] maze = new Byte[width, length];
            sbyte xr = -127, yr = -127;

            // Example of solid wall, allowing to shape the maze and make it less looks like a mere "boring square"
            //maze[2, 0] = maze[3, 0] = maze[4, 0] = maze[5, 0] = 255;
            //maze[2, 1] = maze[3, 1] = maze[4, 1] = maze[5, 1] = 255;
            //maze[3, 2] = 255;
            //maze[9, 3] = maze[9, 4] = maze[8, 4] = 255;

            // here need to convert layout value ino maze value (4th upper bits ONLY !)
            // easier to set them in layout directly !

            // WARNING: when modifying grid size => layout may be over the new size !
            if (_layout != null)
            {
                for (UInt16 x = 0; x < _layout.width; x++)
                {
                    for (UInt16 y = 0; y < _layout.height; y++)
                    {
                        // test not needed
                        if (_layout.layout[x, y] != 0)
                        {
                            Byte value = 255; // (Byte)(255 - _layout.layout[x, y] + 1);
                            maze[x, y] = value;
                        }
                    }
                }
            }

            if (holes > 0)
            {
                for (Byte holesCount = 0; holesCount < holes; holesCount++)
                {
                    Byte holex = (Byte)random.Next(width);
                    Byte holey = (Byte)random.Next(length);

                    maze[holex, holey] = 255;

                    Byte blocktiles = (Byte)random.Next(3, 10);
                    //Debug.Print(string.Format("bt={0}", blocktiles));

                    for (Byte index = 0; index < blocktiles; index++)
                    {
                        while (holex + xr < 0 || holex + xr >= maze.GetLength(0))
                        {
                            xr = (sbyte)random.Next(-radius, radius);
                        }

                        while (holey + yr < 0 || holey + yr >= maze.GetLength(1))
                        {
                            yr = (sbyte)random.Next(-radius, radius);
                        }

                        //Debug.Print(string.Format("x={0} y={1}", holex + xr, holey + yr));
                        // if holex + xr outside arrtay range => generate again
                        maze[holex + xr, holey + yr] = 255;

                        xr = -127;
                        yr = -127;
                    }

                    //for each holes: get neighboors cells, "fill" them
                }
            }

            return maze;
        }


        public void GenerateTWMaze_GrowingTree(PickMethod pmethod = PickMethod.Newest)
        {
            List<UInt16[]> cells = new List<UInt16[]>();

            Array DirectionArray = Enum.GetValues(typeof(Direction));
            
            // Pick starting cell
            //Random random = new Random((int)DateTime.Now.Ticks & (0x0000FFFF));

            UInt16 x = (Byte)(random.Next(maze.GetLength(0) - 1) + 1);
            UInt16 y = (Byte)(random.Next(maze.GetLength(1) - 1) + 1);
            Int16 nx;
            Int16 ny;

            cells.Add(new UInt16[2] { x, y });

            while (cells.Count > 0)
            {
                //Debug.Print("=== CELL LOOP BEGIN ===");

                //choose index: 0 = older, cells.count = newest
                Int16 index = (Int16)chooseIndex((UInt16)cells.Count, pmethod);
                UInt16[] cell_picked = cells[index];

                //use x,y as coord of current cell !
                x = cell_picked[0];
                y = cell_picked[1];
                //Debug.Print(string.Format("picked cell=[{0},{1}] Cells[{2}]", x, y, cells.Count));
                
                Direction[] tmpdir = RandomizeDirection();

                foreach (Direction way in tmpdir)  //shuffled dir
                //foreach (Direction way in DirectionArray) //    Enum.GetValues(typeof(Direction)))    //sequential dir, for test
                {
                    //Debug.Print(string.Format("DIRLOOP= Explore way {0}", way.ToString()));

                    //take a random dir
                    //Direction way = chooseARandomDirection();

                    //get an unvisited neighbor
                    // "go" to the random dir, if cell == 0 proceed
                    SByte[] move = DoAStep(way);

                    nx = (Int16)(x + move[0]);
                    ny = (Int16)(y + move[1]);

                    //Debug.Print(string.Format("chosen cell [{0}][{1}]", nx, ny));
                    //Debug.Print(string.Format("nx={0} ny={1}", nx, ny));

                    //if (nx >= 0 && ny >= 0 && nx < maze.GetLength(0) && ny < maze.GetLength(1))
                    //{
                    //    Debug.Print(string.Format("maze[nx,ny]={0}", maze[nx, ny]));
                    //}

                    // code need to be slowed down to generate good mazes !
                    // random method is bad, time dependant !!!
                    //for (int dumy = 0; dumy < 500000; dumy++)
                    //{ }

                    if (nx >= 0 && ny >= 0 && nx < maze.GetLength(0) && ny < maze.GetLength(1) && maze[nx, ny] == 0)
                    {
                        //Debug.Print(string.Format("xy=[{0},{1}] Unvisited cell [nx,ny]={2},{3}", x, y, nx, ny));

                        //Debug.Print(string.Format("B maze[x,y]={0} maze[nx,ny]={1}", maze[x, y], maze[nx, ny]));
                        maze[x, y] |= (byte)way;
                        maze[nx, ny] |= (byte)OppositeDirection(way);
                        //Debug.Print(string.Format("A maze[x,y]={0} maze[nx,ny]={1}", maze[x, y], maze[nx, ny]));

                        //Debug.Print(string.Format("add cell [{0}][{1}]", nx, ny));
                        cells.Add(new UInt16[2] { (UInt16)nx, (UInt16)ny });
                        //Debug.Print(string.Format("Cells #={0}", cells.Count));

                        index = -1;
                        // need to break the foreach loop here !!
                        break;
                    }
                    else
                    {
                        //Debug.Print("out of maze cells or already processed");
                    }
                }
                //**end dir loop
                //Debug.Print("==END DIR LOOP==");

                //delete this cell from list if none found
                if (index != -1)
                {
                    UInt16[] cell_removed = cells[index];

                    cells.RemoveAt(index);
                    //Debug.Print(string.Format("Cells {0} [{1},{2}] removed, #={3}", index, cell_removed[0], cell_removed[1], cells.Count));
                }

                //Debug.Print("=== CELL LOOP END ===");
            }
        }



        public void cell(UInt16 x, UInt16 y, byte value = 0)
        {
            if (x <= this.width && y <= this.length)
            {
                maze[x, y] = value;
            }
        }

        
        public void dumpMaze()
        {
            if (maze != null)
            {
                for (UInt16 y = 0; y < maze.GetLength(1); y++)
                {
                    string xline = string.Empty;

                    for (UInt16 x = 0; x < maze.GetLength(0); x++)
                    {
                        xline += ' ' + maze[x, y].ToString();
                    }

                    Debug.Print(string.Format("M[{0}]={1}", y, xline));
                }
            }
        }


        public UInt16 chooseIndex(UInt16 max, PickMethod pickmet)
        {
            UInt16 index = 0;
            //bool skip = false;

            switch (pickmet)
            {
                case PickMethod.Cyclic:
                    CyclePick = (UInt16)((CyclePick + 1) % max);
                    index = CyclePick;
                    break;

                case PickMethod.Random:
                    // use class random object ! ~rand1/rand2 (1=use class object, 2=create a new one every time !)
                    //Random random = new Random((int)DateTime.Now.Ticks & (0x0000FFFF));
                    index = (UInt16)(random.Next(max - 1));
                    break;

                case PickMethod.Oldest:
                    index = 0;
                    break;

                case PickMethod.Kit:
                    // Kit from KinghtRaider show, cycle from 0 to max then max to 0 and so on back and forth
                    // instead of the other cyclic method above which do a 0 to max, 0 to max, 0 to max, ...
                    if (CyclePick < max - 1 && KitCycle == true)
                    {
                        CyclePick++;
                        
                    }
                    
                    if (CyclePick > 0 && KitCycle == false)
                    {
                        CyclePick--;
                    }

                    if (CyclePick >= max - 1)
                        KitCycle = false;
                    if (CyclePick <= 0)
                        KitCycle = true;

                    index = CyclePick;
                    //Debug.Print(string.Format("idx={0} (max={1})", index, max));
                    break;

                case PickMethod.Collapse:
                    //todo
                case PickMethod.Newest:
                default:
                    if (max >= 1)
                    {
                        index = (UInt16)(max - 1);
                    }
                    else
                    {
                        index = 0;
                    }

                    break;
            }
            return index;
        }


        public Direction chooseARandomDirection()
        {
            Direction randir;

            //Random random = new Random((int)DateTime.Now.Ticks & (0x0000FFFF));

            var EnumToArray = Enum.GetValues(typeof(Direction));
            Byte tmp1 = (Byte)(random.Next(EnumToArray.Length - 1));
            randir = (Direction)EnumToArray.GetValue(tmp1);

            Debug.Print(string.Format("Dir=[{0}]", randir));

            return randir;
        }


        // comes from http://www.dotnetperls.com/fisher-yates-shuffle
        private void Shuffle<T>(T[] array)
        {
            //Random _random = new Random();

            int n = array.Length;

            for (int i = 0; i < n; i++)
            {
                int r = i + (int)(random.NextDouble() * (n - i));
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }


        // create an array from enum in random order
        public Direction[] RandomizeDirection()
        {
            Direction[] randir;

            Array tmparray = Enum.GetValues(typeof(Direction));

            randir = (Direction[])tmparray;

            Shuffle<Direction>(randir);

            return randir;
        }


        public Direction OppositeDirection(Direction forward)
        {
            // an unknow/invalid dir may be helpful
            Direction opposite = Direction.North;

            switch (forward)
            {
                case Direction.North:
                    opposite = Direction.South;
                    break;
                case Direction.South:
                    opposite = Direction.North;
                    break;
                case Direction.East:
                    opposite = Direction.West;
                    break;
                case Direction.West:
                    opposite = Direction.East;
                    break;
            }

            return opposite;
        }



        public SByte[] DoAStep(Direction facingDirection)
        {
            // an unknow/invalid dir may be helpful
            SByte[] step = { 0, 0 };

            switch (facingDirection)
            {
                case Direction.North:
                    step[0] = 0;
                    step[1] = -1;
                    break;
                case Direction.South:
                    step[0] = 0;
                    step[1] = 1;
                    break;
                case Direction.East:
                    step[0] = 1;
                    step[1] = 0;
                    break;
                case Direction.West:
                    step[0] = -1;
                    step[1] = 0;
                    break;
            }

            return step;
        }


        // convert a line-wise maze to ablock-wise maze
        public Byte[,] LineToBlock()
        {
            Byte[,] blockmaze;

            if (maze == null || maze.GetLength(0) <= 1 && maze.GetLength(1) <= 1)
            {
                return null;
            }

            blockmaze = new Byte[2 * maze.GetLength(0) + 1, 2 * maze.GetLength(1) + 1];

            for (UInt16 wall = 0; wall < 2 * maze.GetLength(1) + 1; wall++)
            {
                blockmaze[0, wall] = 1;
            }

            for (UInt16 wall = 0; wall < 2 * maze.GetLength(0) + 1; wall++)
            {
                blockmaze[wall, 0] = 1;
            }

            for (UInt16 y = 0; y < maze.GetLength(1); y++)
            {
                for (UInt16 x = 0; x < maze.GetLength(0); x++)
                {
                    blockmaze[2 * x + 1, 2 * y + 1] = 0;

                    //Debug.Print(string.Format("M[{0},{1}]={2} & dir={3}", x, y, maze[x, y], maze[x, y] & (Byte)Direction.East));
                    if ((maze[x, y] & (Byte)Direction.East) != 0)
                    {
                        blockmaze[2 * x + 2, 2 * y + 1] = 0; // B
                    }
                    else
                    {
                        blockmaze[2 * x + 2, 2 * y + 1] = 1;
                    }

                    if ((maze[x, y] & (Byte)Direction.South) != 0)
                    {
                        blockmaze[2 * x + 1, 2 * y + 2] = 0; // C
                    }
                    else
                    {
                        blockmaze[2 * x + 1, 2 * y + 2] = 1;
                    }

                    blockmaze[2 * x + 2, 2 * y + 2] = 1;
                }
            }

            // need improvement, obviously
            this.blockmaze = blockmaze;

            return blockmaze;
        }


        // Scale a maze by a given (integer) factor
        // so instead of having a 1 tile sized blokc mazes, you could play with bigger size for not only Pac-Man style degree of freedom
        // NOTE: work on blockwise maze ONLY !
        public Byte[,] scaleMaze(Byte Scale)
        {
            if (blockmaze == null)
            {
                // build a blockwise maze first
                return null;
            }

            Byte[,] biggerMaze = new Byte[blockmaze.GetLength(0) * Scale, blockmaze.GetLength(1) * Scale];

            for (UInt16 y = 0; y < blockmaze.GetLength(1); y++)
            {
                for (UInt16 x = 0; x < blockmaze.GetLength(0); x++)
                {
                    //System.Diagnostics.Debug.Print(string.Format("Block [{0},{1}] = {2}", x,y,blockmaze[x,y]));

                    for (UInt16 sy = 0; sy < Scale; sy++)
                    {
                        for (UInt16 sx = 0; sx < Scale; sx++)
                        {
                            biggerMaze[Scale * x + sx, Scale * y + sy] = blockmaze[x, y];
                            //System.Diagnostics.Debug.Print(string.Format("BIG Block [{0},{1}] = {2}", Scale * x + sx, Scale * y + sy, blockmaze[x, y]));
                        }
                    }
                }
            }

            return biggerMaze;
        }


        // for big maze: cut/damage walls randomly: straight wall => parts have collapsed : holes, rounder corners, ...
        public Byte[,] erodeMaze(float ErosionFactor)
        {
            Byte[,] oldMaze = new Byte[maze.GetLength(0), maze.GetLength(1)];

            return oldMaze;
        }


        // maybe later
        private void loadMaze()
        {
        }


        private void saveMaze()
        {
        }


        //~ maze pack/unpack
        // 4 Bytes = 1 int(32)
        // or even 8 b = 1 int (if only binary values are used) => save memory for blockwise maze
    }
}
