using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GridLibrary;
using MazeLib;
using System;

namespace MazeDisplay_Test
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont osdfont;

        private KeyboardState KeyboardInput;
        private KeyboardState PreviousKeyboardInput;

        Grid Ingrid;
        GridData gdata;
        Maze2D dmaze;
        Maze aMazIng;

        PickMethod picking;
        PickMethod[] methodArray;
        byte currMethod = 0;
        byte mazeW, mazeH;
        UInt16 backgroundsW, backgroundsH;
        byte holesCount, holesMaxRadius;

        Byte layoutIndex;

        String[] methodNames = new string[] { "Newest", "Oldest", "Random", "Cyclic", "Kit", "Collapse" };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            this.Window.AllowUserResizing = true;

            IsMouseVisible = true;

            methodArray = (PickMethod[])Enum.GetValues(typeof(PickMethod));

            picking = PickMethod.Newest;
            mazeW = 20;
            mazeH = 18;
            backgroundsW = 500;
            backgroundsH = 400;
            holesCount = 0;
            holesMaxRadius = 0;

            layoutIndex = 0;

            aMazIng = new Maze(mazeW, mazeH, 0, 0, PickMethod.Cyclic);
            aMazIng.GenerateTWMaze_GrowingTree(picking);

            aMazIng.dumpMaze();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            osdfont = Content.Load<SpriteFont>("osd");

            gdata.offsetx = 100;
            gdata.offsety = 80;
            gdata.gridsizeH = 1;
            gdata.gridsizeV = 1;
            gdata.tilesizeH = 10;
            gdata.tilesizeV = 10;

            Ingrid = new Grid(this, graphics, spriteBatch, gdata);

            dmaze = new Maze2D(this, Ingrid, Ingrid.texture);

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardInput = Keyboard.GetState();
            bool mazeSizeUpdated = false;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || KeyboardInput.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (KeyboardInput.IsKeyDown(Keys.R) && PreviousKeyboardInput.IsKeyUp(Keys.R))
            {
                aMazIng.Reset();
                aMazIng.GenerateTWMaze_GrowingTree(methodArray[currMethod]);
            }


            if (KeyboardInput.IsKeyDown(Keys.P) && PreviousKeyboardInput.IsKeyUp(Keys.P))
            {
                currMethod++;
                currMethod %= (byte)methodArray.Length;
            }

            if (KeyboardInput.IsKeyDown(Keys.NumPad7) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad7))
            {
                mazeW--;
                backgroundsW -= 5;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad9) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad9))
            {
                mazeW++;
                backgroundsW += 5;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad2) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad2))
            {
                mazeH--;
                backgroundsH -= 5;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad8) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad8))
            {
                mazeH++;
                backgroundsH += 5;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad6) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad6))
            {
                holesCount++;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad4) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad4))
            {
                holesCount--;
                if (holesCount < 0)
                    holesCount = 0;

                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad3) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad3))
            {
                holesMaxRadius++;
                mazeSizeUpdated = true;
            }
            if (KeyboardInput.IsKeyDown(Keys.NumPad1) && PreviousKeyboardInput.IsKeyUp(Keys.NumPad1))
            {
                holesMaxRadius--;
                if (holesMaxRadius < 0)
                    holesMaxRadius = 0;

                mazeSizeUpdated = true;
            }

            if (KeyboardInput.IsKeyDown(Keys.U) && PreviousKeyboardInput.IsKeyUp(Keys.U))
            {
                layoutIndex--;
            }
            if (KeyboardInput.IsKeyDown(Keys.I) && PreviousKeyboardInput.IsKeyUp(Keys.I))
            {
                layoutIndex++;
            }

            layoutIndex = (Byte)MathHelper.Clamp(layoutIndex, 0, 9);

            if (KeyboardInput.IsKeyDown(Keys.L) && PreviousKeyboardInput.IsKeyUp(Keys.L))
            {
                //load layout
                mazeSizeUpdated = true;
            }


            if (mazeSizeUpdated == true)
            {
                aMazIng.UpdateSize(mazeW, mazeH, holesCount, holesMaxRadius);

                // need to update total grid size too (+x cells = +n pixels)
                aMazIng.Reset();
                aMazIng.GenerateTWMaze_GrowingTree(methodArray[currMethod]);
            }

            PreviousKeyboardInput = KeyboardInput;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // should use maze size (need a link between both objects)
            Ingrid.drawBaseGrid(mazeW, mazeH, backgroundsW, backgroundsH);

            dmaze.DrawMaze(aMazIng.maze);

            String text = string.Format("Controls:\nR - reset\nP - change picking method\n\n  Pickmethod [{0}]\n\n" +
                "\nU/I - decrease/increase layout index\nL - load layout number [{1}]\n" +
                "\nNumpad keys:\n7 - decrease cells W\n9 - increase cells W" +
                "\n2 - decrease cells H\n8 - increase cells H" +
                "\n6 - increase holes count\n4 - decrease holes count" +
                "\n3 - increase holes max radius\n1 - decrease holes max radius"
                , methodNames[currMethod], layoutIndex);
            // increase/dec number of "holes"
            // inc/dec max radius of holes
            text += string.Format("\n\n  sizeW={0} sizeH={1}", mazeW, mazeH);
            text += string.Format("\n  Holes#={0} MaxRadius={1}", holesCount, holesMaxRadius);

            spriteBatch.Begin();
            spriteBatch.DrawString(osdfont, text, new Vector2(graphics.PreferredBackBufferWidth - 300, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
