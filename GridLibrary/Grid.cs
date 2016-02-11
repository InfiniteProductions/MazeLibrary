using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//~add load/save layout here
namespace GridLibrary
{
    public struct GridCell
    {
        public UInt16 x, y;
        public Color color;
    }

    // convenient structure to fill properties with constructor
    public struct GridData
    {
        public UInt16 offsetx,
                    offsety,
                    gridsizeH,
                    gridsizeV,
                    tilesizeH,
                    tilesizeV,
                    actualWidth,
                    actualHeight;
    };

    public class Grid
    {
        public Game game { get; private set; }
        public GraphicsDeviceManager graphics { get; private set; }
        public SpriteBatch spriteBatch { get; private set; }
        public Texture2D texture { get; private set; }

        public GridData g_data { get; set; }

        public UInt16 offsetx { get; private set; }
        public UInt16 offsety { get; private set; }
        public UInt16 gridsizeH { get; private set; }
        public UInt16 gridsizeV { get; private set; }
        public UInt16 tilesizeH { get; private set; }
        public UInt16 tilesizeV { get; private set; }
        public UInt16 actualWidth { get; private set; }
        public UInt16 actualHeight { get; private set; }



        public Grid(Game game, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GridData gridData)
        {
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;

            //spriteBatch = new SpriteBatch(game.GraphicsDevice);

            offsetx = gridData.offsetx;
            offsety = gridData.offsety;
            gridsizeH = gridData.gridsizeH;
            gridsizeV = gridData.gridsizeV;
            tilesizeH = gridData.tilesizeH;
            tilesizeV = gridData.tilesizeV;

            texture = new Texture2D(this.graphics.GraphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White });
        }


        public void updateGrid(UInt16 nbcellsH, UInt16 nbcellsV, UInt16 width, UInt16 height)
        {
            // to update with mazeeditor changes
            tilesizeH = (UInt16)((width - (nbcellsH + 1) * gridsizeH) / nbcellsH);
            tilesizeV = (UInt16)((height - (nbcellsV + 1) * gridsizeV) / nbcellsV);

            // if sizes > screen/window get the closest available
            actualWidth = (UInt16)((nbcellsH * tilesizeH) + (nbcellsH + 1) * gridsizeH);
            actualHeight = (UInt16)((nbcellsV * tilesizeV) + (nbcellsV + 1) * gridsizeV);
        }


        // Draw the background/board and base grid
        // some code need to be modified to fit your needs, like blendstate to draw grid over a beackground picture
        // ~ choose background color 
        // spritebatch as param + tex = they have nothing to do here !
        public void drawBaseGrid(UInt16 nbcellsH, UInt16 nbcellsV, UInt16 width, UInt16 height)
        {
            UInt16 x, y;

            // to update with mazeeditor changes
            tilesizeH = (UInt16)((width - (nbcellsH + 1) * gridsizeH) / nbcellsH);
            tilesizeV = (UInt16)((height - (nbcellsV + 1) * gridsizeV) / nbcellsV);

            // if sizes > screen/window get the closest available
            actualWidth = (UInt16)((nbcellsH * tilesizeH) + (nbcellsH + 1) * gridsizeH);
            actualHeight = (UInt16)((nbcellsV * tilesizeV) + (nbcellsV + 1) * gridsizeV);

            Rectangle background = new Rectangle(offsetx, offsety, actualWidth, actualHeight);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(texture, background, Color.White);

            for (x = 0; x <= nbcellsH; x++)
            {
                Rectangle rectangle = new Rectangle(offsetx + x * (tilesizeH + gridsizeH), offsety, gridsizeH, actualHeight);
                spriteBatch.Draw(texture, rectangle, Color.LightGray);
            }

            for (y = 0; y <= nbcellsV; y++)
            {
                Rectangle rectangle = new Rectangle(offsetx, offsety + y * (tilesizeV + gridsizeV), actualWidth, gridsizeV);
                spriteBatch.Draw(texture, rectangle, Color.LightGray);
            }

            spriteBatch.End();
        }


        // There is still one little issue in this code if gridsize > 1, maybe from the two +1 below
        public UInt16[] getCellScreenCoordinates(UInt16 x, UInt16 y)
        {
            UInt16[] cell = { 0, 0, 0, 0 };
            // from grid coordinates X,Y => get rectangle screen coordinates x,y,x',y'
            // this allow to draw a rectangle right away without any further calculations

            // +1 here, without grid size below = cell only, no border
            cell[0] = (UInt16)(offsetx + x * (tilesizeH + gridsizeH) + 1);
            cell[1] = (UInt16)(offsety + y * (tilesizeV + gridsizeV) + 1);

            cell[2] = (UInt16)(tilesizeH);
            cell[3] = (UInt16)(tilesizeV);

            System.Diagnostics.Debug.Print(string.Format("c={0} {1} {2} {3}", cell[0], cell[1], cell[2], cell[3]));
            return cell;
        }


        public UInt16[] getCellGridCoordinates(UInt16 sx, UInt16 sy)
        {
            UInt16[] gridpos = { 0, 0 };

            gridpos[0] = (UInt16)((sx - offsetx) / (tilesizeH + gridsizeH));
            gridpos[1] = (UInt16)((sy - offsety) / (tilesizeV + gridsizeV));

            return gridpos;
        }


        // those two in game test
        //private void updateCell(UInt16[] cell, Color color)
        //{
        //    UInt16[] screencel = getCellScreenCoordinates(cell[0], cell[1]);

        //    GridCell gcell = new GridCell();
        //    gcell.x = screencel[0];
        //    gcell.y = screencel[1];
        //    gcell.color = color;

        //    CellsToDraw.Add(gcell);
        //}


        //private void DrawUpdatedCells()
        //{
        //    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        //    UInt16 index;
        //    GridCell cell;

        //    for (index = 0; index < CellsToDraw.Count; index++)
        //    {
        //        cell = CellsToDraw[index];
        //        Rectangle rectangle = new Rectangle(cell.x, cell.y, tilesizeH, tilesizeV);
        //        spriteBatch.Draw(texture, rectangle, cell.color);

        //        // to use for instant click/button like display (color removed just after click)
        //        //CellsToDraw.RemoveAt(index);
        //    }

        //    spriteBatch.End();
        //}
    }
}
