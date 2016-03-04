using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GridLibrary;
using MazeLib;


// editshape with mouse (~template: circle, cross, triangle)
// save => data use by/instead of buildbasemaze method
// mouse LB = add cell, RB = remove cell, MMB = chge type
// 0 empty space, 1 = wall, 2= A, 3=B, ...

// autolayout/template:
// circle = need to know the cell located inside the perimeter
// ~ lozange/diamond = middlewidth to/from middleheight

namespace MazeShapeEditor
{
    //public struct GridCell
    //{
    //    public UInt16 x, y;
    //    public Color color;
    //    public Byte value;
    //}

    
    public class MazeEditor : Game
    {
        public Color[] defaultColors = { Color.White, Color.LightSeaGreen, Color.OrangeRed, Color.DeepSkyBlue, Color.Yellow, Color.Black, Color.MediumTurquoise, Color.Red, Color.Red, Color.Red };
        //default color: empty cell, wall, start, exit, ...
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont osdfont;
        String savetext;
        float st_delay;

        public KeyboardState KeyboardInput;
        public KeyboardState PreviousKeyboardInput;
        public MouseState MouseInput;
        public MouseState PreviousMouseInput;

        Grid Ingrid;
        GridData g_data;
        Texture2D texture;

        Layout mazeLayout;

        //List<GridCell> CellsToDraw;
        Byte currentColor;

        // number of cells horizontally & vertically
        Byte layoutWidth, layoutHeight;

        // file index for I/O (from 0 to 9)
        Byte currentLayoutIndex;

        // background rectangle of the displayd grid
        UInt16 backgroundWidth, backgroundHeight;

        bool layoutHasBeenUpdated = false;


        public MazeEditor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //needed ?
            g_data.offsetx = 100;
            g_data.offsety = 80;
            g_data.gridthicknessH = 1;
            g_data.gridthicknessV = 1;
            g_data.tilesizeH = 10;
            g_data.tilesizeV = 10;

            layoutWidth = layoutHeight = 10;
            currentLayoutIndex = 0;

            savetext = string.Empty;
            st_delay = 0f;
            currentColor = 1;

            mazeLayout = new Layout(layoutWidth, layoutHeight);

            //CellsToDraw = new List<GridCell>();
        }


        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            this.Window.AllowUserResizing = true;

            IsMouseVisible = true;

            //layout = new Byte[layoutWidth, layoutHeight];

            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            texture = new Texture2D(graphics.GraphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White });

            Ingrid = new Grid(this, graphics, spriteBatch, g_data);

            osdfont = Content.Load<SpriteFont>("osd");
        }


        protected override void UnloadContent()
        {
            Content.Unload();

            texture.Dispose();
        }


        protected override void Update(GameTime gameTime)
        {
            //PreviousMouseInput = MouseInput;
            MouseInput = Mouse.GetState();
            KeyboardInput = Keyboard.GetState();

            if (KeyboardInput.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (MouseInput.LeftButton == ButtonState.Pressed && PreviousMouseInput.LeftButton == ButtonState.Released)
            {
                UInt16[] cellA = Ingrid.getCellGridCoordinates((UInt16)MouseInput.X, (UInt16)MouseInput.Y);

                // use only value = array index
                //update_cell(cellA, currentColor);
                mazeLayout.updateCell(cellA, currentColor);
            }

            if (MouseInput.RightButton == ButtonState.Pressed && PreviousMouseInput.RightButton == ButtonState.Released)
            {
                UInt16[] cellA = Ingrid.getCellGridCoordinates((UInt16)MouseInput.X, (UInt16)MouseInput.Y);

                //update_cell(cellA, 0);
                mazeLayout.updateCell(cellA, 0);
            }

            for (Keys key = Keys.NumPad0; key <= Keys.NumPad9; key++)
            {
                if (KeyboardInput.IsKeyDown(key) && PreviousKeyboardInput.IsKeyUp(key))
                {
                    currentColor = (Byte)(key - Keys.NumPad0);
                }
            }


            if (KeyboardInput.IsKeyDown(Keys.P) && PreviousKeyboardInput.IsKeyUp(Keys.P))
            {
                currentLayoutIndex--;
            }
            if (KeyboardInput.IsKeyDown(Keys.N) && PreviousKeyboardInput.IsKeyUp(Keys.N))
            {
                currentLayoutIndex++;
            }

            currentLayoutIndex = (Byte)MathHelper.Clamp(currentLayoutIndex, 0, 9);


            if (KeyboardInput.IsKeyDown(Keys.F1) && PreviousKeyboardInput.IsKeyUp(Keys.F1))
            {
                mazeLayout.CreatePattern(0, currentColor);
            }

            if (KeyboardInput.IsKeyDown(Keys.C) && PreviousKeyboardInput.IsKeyUp(Keys.C))
            {
                //CellsToDraw.Clear();
                mazeLayout.ClearCells();
            }

            if (KeyboardInput.IsKeyDown(Keys.A) && PreviousKeyboardInput.IsKeyUp(Keys.A))
            {
                Ingrid.updateGrid(layoutWidth, layoutHeight, 600, 400);
            }

            // reset layout after resize => clean upd cells list NO
            // keep list, adjust cell sizes !
            if (KeyboardInput.IsKeyDown(Keys.U) && PreviousKeyboardInput.IsKeyUp(Keys.U))
            {
                layoutWidth--;
                //CellsToDraw.Clear();

                layoutHasBeenUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.I) && PreviousKeyboardInput.IsKeyUp(Keys.I))
            {
                layoutWidth++;
                //CellsToDraw.Clear();

                layoutHasBeenUpdated = true;
            }

            if (layoutWidth < 3)
                layoutWidth = 3;

            if (KeyboardInput.IsKeyDown(Keys.J) && PreviousKeyboardInput.IsKeyUp(Keys.J))
            {
                layoutHeight--;
                //CellsToDraw.Clear();

                layoutHasBeenUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.K) && PreviousKeyboardInput.IsKeyUp(Keys.K))
            {
                layoutHeight++;
                //CellsToDraw.Clear();

                layoutHasBeenUpdated = true;
            }

            if (layoutHeight < 3)
                layoutHeight = 3;

            if (layoutHasBeenUpdated == true)
            {
                // upd cell list

                mazeLayout.Update(layoutWidth, layoutHeight);

                //tmp => changes nothing
                //Ingrid.updateGrid(layoutWidth, layoutHeight, 600, 400);
                // ==> when cells sizes change => update list of cell to update !! it still holds old coord/sizes

                layoutHasBeenUpdated = false;
            }

            if (KeyboardInput.IsKeyDown(Keys.S) && PreviousKeyboardInput.IsKeyUp(Keys.S))
            {
                //tmpSaveLayout(layoutWidth, layoutHeight, currentLayoutIndex);
                mazeLayout.SaveLayout(currentLayoutIndex);

                savetext = "Layout " + currentLayoutIndex.ToString() + " saved\n";
                st_delay = 3.0f;
            }

            if (KeyboardInput.IsKeyDown(Keys.L) && PreviousKeyboardInput.IsKeyUp(Keys.L))
            {
                //CellsToDraw.Clear();
                mazeLayout.ClearCells();

                System.Diagnostics.Debug.Print("=========== LOAD =============");

                mazeLayout.LoadLayout(currentLayoutIndex);
                
                // two upd, not very good
                Ingrid.updateGrid(mazeLayout.width, mazeLayout.height, 600, 400);
                
                layoutWidth = (Byte)mazeLayout.width;
                layoutHeight = (Byte)mazeLayout.height;


                savetext = "Layout " + currentLayoutIndex.ToString() + " loaded\n";
                st_delay = 3.0f;
            }

            if (st_delay <= 0)
            {
                savetext = "";
            }
            else
            {
                st_delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            PreviousMouseInput = MouseInput;
            PreviousKeyboardInput = KeyboardInput;

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Ingrid.drawBaseGrid(layoutWidth, layoutHeight, 600, 400);

            DrawUpdatedCells();

            String text = string.Format("CellType:{0}\nNumpad0-9 to change type\n\n", currentColor);
            text += string.Format("\nwidht: {0}  height: {1}\n U/I decrease/increase height\n J/K decrease/increase height\n\n", layoutWidth, layoutHeight);
            text += string.Format("\nC: clear layout\nS: save layout\nLayout (P-, N+): {0}\n\n{1}", currentLayoutIndex, savetext);

            spriteBatch.Begin();
            spriteBatch.DrawString(osdfont, text, new Vector2(graphics.PreferredBackBufferWidth - 300, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }


        //part of gridlib now, to remove

        // Draw the background/board and base grid
        // some code need to be modified to fit your needs, like blendstate to draw grid over a beackground picture
        private void draw_base_grid(UInt16 nbcellsH, UInt16 nbcellsV, UInt16 width, UInt16 height)
        {
            UInt16 x, y;

            //need to be in a reset/update grid method
            g_data.tilesizeH = (UInt16)((width - (nbcellsH + 1) * g_data.gridthicknessH) / nbcellsH);
            g_data.tilesizeV = (UInt16)((height - (nbcellsV + 1) * g_data.gridthicknessV) / nbcellsV);

            // if sizes > screen/window get the closest available
            g_data.actualWidth = (UInt16)((nbcellsH * g_data.tilesizeH) + (nbcellsH + 1) * g_data.gridthicknessH);
            g_data.actualHeight = (UInt16)((nbcellsV * g_data.tilesizeV) + (nbcellsV + 1) * g_data.gridthicknessV);

            Rectangle background = new Rectangle(g_data.offsetx, g_data.offsety, g_data.actualWidth, g_data.actualHeight);

            // ~ move out the begin/end sb
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            spriteBatch.Draw(texture, background, Color.White);

            for (x = 0; x <= nbcellsH; x++)
            {
                Rectangle rectangle = new Rectangle(g_data.offsetx + x * (g_data.tilesizeH + g_data.gridthicknessH), g_data.offsety, g_data.gridthicknessH, g_data.actualHeight);
                spriteBatch.Draw(texture, rectangle, Color.LightGray);
            }

            for (y = 0; y <= nbcellsV; y++)
            {
                Rectangle rectangle = new Rectangle(g_data.offsetx, g_data.offsety + y * (g_data.tilesizeV + g_data.gridthicknessV), g_data.actualWidth, g_data.gridthicknessV);
                spriteBatch.Draw(texture, rectangle, Color.LightGray);
            }

            spriteBatch.End();
        }


        // There is still one little issue in this code if gridsize > 1, maybe from the two +1 below
        private UInt16[] _unused_getCellScreenCoordinates(UInt16 x, UInt16 y)
        {
            //System.Diagnostics.Debug.Print(string.Format("lw={0} lh={1}",layoutWidth, layoutHeight));
            System.Diagnostics.Debug.Print(string.Format("gth={0} gtv={1}", g_data.tilesizeH, g_data.tilesizeV));
            //System.Diagnostics.Debug.Print(string.Format("gsh={0} gsv={1}", g_data.gridsizeH, g_data.gridsizeV));

            UInt16[] cell = { 0, 0, 0, 0 };
            // from grid coordinates X,Y => get rectangle screen coordinates x,y,x',y'
            // this allow to draw a rectangle right away without any further calculations

            // +1 here, without grid size below = cell only, no border
            cell[0] = (UInt16)(g_data.offsetx + x * (g_data.tilesizeH + g_data.gridthicknessH) + 1);
            cell[1] = (UInt16)(g_data.offsety + y * (g_data.tilesizeV + g_data.gridthicknessV) + 1);

            cell[2] = (UInt16)(g_data.tilesizeH);
            cell[3] = (UInt16)(g_data.tilesizeV);

            return cell;
        }


        private UInt16[] _unused_getCellGridCoordinates(UInt16 sx, UInt16 sy)
        {
            UInt16[] gridpos = { 0, 0 };

            gridpos[0] = (UInt16)((sx - g_data.offsetx) / (g_data.tilesizeH + g_data.gridthicknessH));
            gridpos[1] = (UInt16)((sy - g_data.offsety) / (g_data.tilesizeV + g_data.gridthicknessV));

            return gridpos;
        }


        private void _unused_update_cell(UInt16[] cell, Byte color)
        {
            GridCell gcell = new GridCell();
            gcell.x = cell[0];
            gcell.y = cell[1];
            gcell.value = color;

            if (color == 255)
                color = 1;

            gcell.color = defaultColors[color];

            //System.Diagnostics.Debug.Print(string.Format("cell x={0}->{2} y={1}->{3}", cell[0], cell[1], gcell.x, gcell.y));

            //CellsToDraw.Add(gcell);
        }


        private void DrawUpdatedCells()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            UInt16 index;
            GridCell cell;

            // mazeLayout.getCellsCount()
            for (index = 0; index < mazeLayout.CellsToDraw.Count; index++)
            {
                cell = mazeLayout.getCellAtIndex(index);
                //cell = mazeLayout.CellsToDraw[index];

                // if cell in the list outside the layout => discarded
                if (cell.x >= layoutWidth || cell.y >= layoutHeight)
                {
                    mazeLayout.removeCellAtIndex(index);
                    //mazeLayout.CellsToDraw.RemoveAt(index);
                }
                else
                {
                    UInt16[] screencel = Ingrid.getCellScreenCoordinates(cell.x, cell.y);

                    Rectangle rectangle = new Rectangle(screencel[0], screencel[1], Ingrid.tilesizeH, Ingrid.tilesizeV); //g_data.tilesizeH, g_data.tilesizeV);
                    spriteBatch.Draw(texture, rectangle, cell.color);

                    // to use for instant click/button like display (color removed just after click)
                    //CellsToDraw.RemoveAt(index);
                }
            }

            spriteBatch.End();
        }


        //private Byte[,] GenerateMazeLayout(UInt16 nbcellsH, UInt16 nbcellsV)
        //{
        //    Byte[,] mazeLayout = new Byte[nbcellsH, nbcellsV];

        //    UInt16 index;
        //    GridCell cell;

        //    //UInt16[] cellcoord;

        //    for (index = 0; index < CellsToDraw.Count; index++)
        //    {
        //        cell = CellsToDraw[index];

        //        //cellcoord = Ingrid.getCellGridCoordinates(cell.x, cell.y);

        //        // on save: issue below: 2 coords = max int16
        //        //mazeLayout[cellcoord[0], cellcoord[1]] = cell.value;
        //        mazeLayout[cell.x, cell.y] = cell.value;
        //    }

        //    return mazeLayout;
        //}


        // ~separate library for IO
        // => layout as input param
        // ~layout class with load/save/init ?
        //private void tmpSaveLayout(UInt16 nbcellsH, UInt16 nbcellsV, Byte index)
        //{
        //    Byte[,] layout = GenerateMazeLayout(nbcellsH, nbcellsV);

        //    using (BinaryWriter bwriter = new BinaryWriter(File.Open("m" + index.ToString() + ".layout", FileMode.Create)))
        //    {
        //        bwriter.Write("MazeLayout V1.0-" + index.ToString());
        //        bwriter.Write(nbcellsH);
        //        bwriter.Write(nbcellsV);

        //        for (UInt16 x = 0; x < nbcellsH; x++)
        //        {
        //            for (UInt16 y = 0; y < nbcellsV; y++)
        //            {
        //                bwriter.Write(layout[x,y]);

        //                //if (layout[x, y] != 0)
        //                //    System.Diagnostics.Debug.Print(string.Format("L[{0},{1}]={2}", x, y, layout[x, y]));
        //            }
        //        }

        //        bwriter.Write("L" + index.ToString() + "end");
        //    }
        //}


        // sep => layout/cell list as output param + update grid outside
        //private void tmpLoadLayout(Byte index)
        //{
        //    if (File.Exists("m" + index.ToString() + ".layout") == true)
        //    {
        //        using (BinaryReader breader = new BinaryReader(File.Open("m" + index.ToString() + ".layout", FileMode.Open)))
        //        {
        //            string fileformatversion = breader.ReadString();
                    
        //            layoutWidth = (Byte)breader.ReadInt16();
        //            layoutHeight = (Byte)breader.ReadInt16();

        //            Ingrid.updateGrid(layoutWidth, layoutHeight, 600, 400);
                    
        //            for (UInt16 x = 0; x < layoutWidth; x++)
        //            {
        //                for (UInt16 y = 0; y < layoutHeight; y++)
        //                {
        //                    Byte dummy = breader.ReadByte();
        //                    if (dummy > 0)
        //                    {
        //                        // set color/value according to read value
        //                        //update_cell(new[] { x, y }, dummy);
        //                        //mazeLayout.updateCell(new[] { x, y }, dummy);
        //                    }
        //                }
        //            }

        //            //System.Diagnostics.Debug.Print(string.Format("Head=[{0}] ch=[{1}] cv=[{2}]", fileformatversion, nbcellsH, nbcellsV));
        //        }
        //    }
        //}
    }
}
