using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GridLibrary;
using MazeLib;

namespace MazeLib
{
    public class Maze2D
    {
        public Game game { get; private set; }
        public GraphicsDeviceManager graphics { get; private set; }
        public SpriteBatch spriteBatch { get; private set; }
        
        public Grid grid { get; private set; }

        private Texture2D texture;

        public Maze2D(Game game, Grid _grid, Texture2D _texture)
        {
            this.game = game;
            grid = _grid;
            texture = _texture;
        }


        // currently: bad: draw in fact a passage and 3 walls around (N = W S E walls, N empty !)
        // needed: draw a wall where ?
        // cells: contains dirs (ex: NS = passage N<->S, walls W/E ==> draw walls for each dir not in cells)
        private void drawAWall(UInt16 x, UInt16 y, Direction direction)
        {
            // draw wall from cell [x,y] dir : N/W/E/S
            UInt16[] screencel = grid.getCellScreenCoordinates(x, y);

            Rectangle northwall = Rectangle.Empty;
            Rectangle eastwall = Rectangle.Empty;
            Rectangle southhwall = Rectangle.Empty;
            Rectangle westwall = Rectangle.Empty;
            Color[] wallcolor = new Color[4] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent };

            // to move in draw itself, maybe
            grid.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Grid H/V *3 = settings
            if ((direction & Direction.North) == 0)
            {
                northwall = new Rectangle(screencel[0], screencel[1] - grid.gridsizeV, grid.tilesizeH, grid.gridsizeV * 3);
                wallcolor[0] = Color.Black; //Color.Blue;
            }

            if ((direction & Direction.West) == 0)
            {
                eastwall = new Rectangle(screencel[0] - grid.gridsizeH, screencel[1], grid.gridsizeH * 3, grid.tilesizeV);
                wallcolor[1] = Color.Black; //Color.GreenYellow;
            }

            if ((direction & Direction.South) == 0)
            {
                southhwall = new Rectangle(screencel[0], screencel[1] + grid.tilesizeV - grid.gridsizeV, grid.tilesizeH, grid.gridsizeV * 3);
                wallcolor[2] = Color.Black; //Color.Red;
            }

            if ((direction & Direction.East) == 0)
            {
                westwall = new Rectangle(screencel[0] - grid.gridsizeH + grid.tilesizeH, screencel[1], grid.gridsizeH * 3, grid.tilesizeV);
                wallcolor[3] = Color.Black;
            }

            grid.spriteBatch.Draw(texture, northwall, wallcolor[0]);
            grid.spriteBatch.Draw(texture, eastwall, wallcolor[1]);
            grid.spriteBatch.Draw(texture, southhwall, wallcolor[2]);
            grid.spriteBatch.Draw(texture, westwall, wallcolor[3]);

            grid.spriteBatch.End();
        }


        private void drawABlockedCell(UInt16 x, UInt16 y)
        {
            UInt16[] screencel = grid.getCellScreenCoordinates(x, y);

            Rectangle Block = new Rectangle(screencel[0], screencel[1], grid.tilesizeH, grid.tilesizeV);

            grid.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            grid.spriteBatch.Draw(texture, Block, Color.LightGray);
            grid.spriteBatch.End();
        }


        public void DrawMaze(Byte[,] mazeToDraw)
        {
            for (UInt16 y = 0; y < mazeToDraw.GetLength(1); y++)
            {
                for (UInt16 x = 0; x < mazeToDraw.GetLength(0); x++)
                {
                    // here: if cell = 255 => fill cell with a gray color
                    if (mazeToDraw[x, y] == 255)
                    {
                        //Console.WriteLine(string.Format("[{0},{1}] = block", x,y));
                        drawABlockedCell(x, y);
                    }
                    else
                    {
                        foreach (Direction way in Enum.GetValues(typeof(Direction)))
                        {
                            // x-y swapped no change
                            //drawAWall(y, x, (Direction)mazeToDraw[y,x]);
                            drawAWall(x, y, (Direction)mazeToDraw[x, y]);
                        }
                    }
                }
            }
        }
    }
}
