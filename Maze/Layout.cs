using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GridLibrary;

namespace MazeLib
{
    public class Layout
    {
        public Color[] defaultColors = { Color.White, Color.LightSeaGreen, Color.OrangeRed, Color.DeepSkyBlue, Color.Yellow, Color.Black, Color.MediumTurquoise, Color.Red, Color.Red, Color.Red };
        //default color: empty cell, wall, start, exit, ...
        
        private const string layoutVersionString = "MazeLayout V1.0-";

        public Byte[,] layout { get; private set; }
        
        public UInt16 width { get; private set; }
        public UInt16 height { get; private set; }

        public List<GridCell> CellsToDraw;


        public Layout(UInt16 nbcellsH = 10, UInt16 nbcellsV = 10)
        {
            width = nbcellsH;
            height = nbcellsV;

            layout = new Byte[width, height];

            CellsToDraw = new List<GridCell>();
        }


        public void Update(UInt16 newWidth, UInt16 newHeight)
        {
            width = newWidth;
            height = newHeight;

            layout = new Byte[newWidth, newHeight];
        }


        public void GenerateMazeLayout()
        {
            UInt16 index;
            GridCell cell;

            for (index = 0; index < CellsToDraw.Count; index++)
            {
                cell = CellsToDraw[index];
                layout[cell.x, cell.y] = cell.value;
            }
        }


        public void SaveLayout(Byte index)
        {
            GenerateMazeLayout();

            using (BinaryWriter bwriter = new BinaryWriter(File.Open("m" + index.ToString() + ".layout", FileMode.Create)))
            {
                bwriter.Write(layoutVersionString + index.ToString());
                bwriter.Write(width);
                bwriter.Write(height);

                for (UInt16 x = 0; x < width; x++)
                {
                    for (UInt16 y = 0; y < height; y++)
                    {
                        bwriter.Write(layout[x, y]);

                        //if (layout[x, y] != 0)
                        //    System.Diagnostics.Debug.Print(string.Format("L[{0},{1}]={2}", x, y, layout[x, y]));
                    }
                }

                bwriter.Write("L" + index.ToString() + "end");
            }
        }


        // sep => layout/cell list as output param + update grid outside
        public void LoadLayout(Byte index)
        {
            if (File.Exists("m" + index.ToString() + ".layout") == true)
            {
                using (BinaryReader breader = new BinaryReader(File.Open("m" + index.ToString() + ".layout", FileMode.Open)))
                {
                    string fileformatversion = breader.ReadString();

                    width = (Byte)breader.ReadInt16();
                    height = (Byte)breader.ReadInt16();
                    System.Diagnostics.Debug.Print(string.Format("L w={0} h={1}", width, height));

                    //Ingrid.updateGrid(layoutWidth, layoutHeight, 600, 400);

                    for (UInt16 x = 0; x < width; x++)
                    {
                        for (UInt16 y = 0; y < height; y++)
                        {
                            Byte dummy = breader.ReadByte();
                            if (dummy > 0)
                            {
                                // set color/value according to read value
                                updateCell(new[] { x, y }, dummy);
                            }
                        }
                    }

                    //System.Diagnostics.Debug.Print(string.Format("Head=[{0}] ch=[{1}] cv=[{2}]", fileformatversion, nbcellsH, nbcellsV));
                }
            }
        }


        public void ClearCells()
        {
            CellsToDraw.Clear();
        }


        public GridCell getCellAtIndex(UInt16 index)
        {
            return CellsToDraw[index];
        }


        public void removeCellAtIndex(UInt16 index)
        {
            CellsToDraw.RemoveAt(index);
        }


        public void updateCell(UInt16[] cell, Byte color)
        {
            GridCell gcell = new GridCell();
            gcell.x = cell[0];
            gcell.y = cell[1];
            gcell.value = color;

            if (color == 255)
                color = 1;

            gcell.color = defaultColors[color];

            //System.Diagnostics.Debug.Print(string.Format("cell x={0}->{2} y={1}->{3}", cell[0], cell[1], gcell.x, gcell.y));

            CellsToDraw.Add(gcell);
        }
    }
}
