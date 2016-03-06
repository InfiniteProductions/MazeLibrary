// Grid Example - Tic Tac Toe game
// This is a very simple example of a board game using the grid library
// You can see with it, you don't have to worry at all about board size, cell locations, etc, it's very easy to use and let you focus
// on things that's really matter (display, cpu players, game-play, ...)

// disclaimer: this IS NOT a complete game, it's just an example, feel free to add winnig/loosing condition, scores, match, less dumb computer opponent, menus, buttons, better visual, whatever

// Infinite Productions 05/03/2016

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GridLibrary;

namespace TicTacToe_GridExample
{
    public class TicTacToe : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState KeyboardInput;
        KeyboardState PreviousKeyboardInput;
        MouseState MouseInput;
        MouseState PreviousMouseInput;

        Grid Board;
        GridData gdata;

        Byte[,] gameBoard;

        Texture2D Circle, Cross;
        Rectangle destrec;

        bool currentPlayer;

        Random cpubrain;

        public TicTacToe()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            currentPlayer = true;

            cpubrain = new Random(123456);

            gameBoard = new Byte[3, 3];
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            //this.Window.AllowUserResizing = true;

            IsMouseVisible = true;

            gdata.offsetx = 60;
            gdata.offsety = 60;
            gdata.gridthicknessH = 2;
            gdata.gridthicknessV = 2;
            gdata.tilesizeH = 10;
            gdata.tilesizeV = 10;

            destrec = new Rectangle();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // 64x64 size tex
            Cross = Content.Load<Texture2D>("ttt-cross");
            Circle = Content.Load<Texture2D>("ttt-circle");

            Board = new Grid(this, graphics, spriteBatch, gdata);
            Board.updateGrid(3, 3, 600, 600);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            GC.Collect(0);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseInput = Mouse.GetState();
            KeyboardInput = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || KeyboardInput.IsKeyDown(Keys.Escape))
                Exit();

            if (CheckBoard() == true)
            {
                if (currentPlayer == true)
                {
                    //human
                    if (MouseInput.LeftButton == ButtonState.Pressed && PreviousMouseInput.LeftButton == ButtonState.Released)
                    {
                        UInt16[] cell = Board.getCellGridCoordinates((UInt16)MouseInput.X, (UInt16)MouseInput.Y);

                        if (gameBoard[cell[0], cell[1]] == 0)
                        {
                            gameBoard[cell[0], cell[1]] = 1;
                        }

                        currentPlayer = !currentPlayer;
                    }
                }
                else
                {
                    while (currentPlayer == false)
                    {
                        Byte x = (Byte)cpubrain.Next(0, 3);
                        Byte y = (Byte)cpubrain.Next(0, 3);

                        if (gameBoard[x, y] == 0)
                        {
                            gameBoard[x, y] = 2;
                            currentPlayer = !currentPlayer;
                        }
                    }


                    // super basic dumbness: take the first empty cell
                    //for (Byte y = 0; y < 3 && currentPlayer == false; y++)
                    //{
                    //    for (Byte x = 0; x < 3 && currentPlayer == false; x++)
                    //    {
                    //        if (gameBoard[x, y] == 0)
                    //        {
                    //            gameBoard[x, y] = 2;
                    //            currentPlayer = !currentPlayer;
                    //        }
                    //    }
                    //}

                    // for a more tougher computer opponent, check this: http://neverstopbuilding.com/minimax
                }
            }
            PreviousMouseInput = MouseInput;
            PreviousKeyboardInput = KeyboardInput;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            UInt16[] screenCell;
            Texture2D playerToken = null;
            bool skip = false;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // bad for now: it use its own spritebatch
            Board.drawBaseGrid(3, 3, 600, 600);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            for (Byte y = 0; y < 3; y++)
            {
                for (Byte x = 0; x < 3; x++)
                {
                    screenCell = Board.getCellScreenCoordinates(x,y);

                    destrec.X = screenCell[0];
                    destrec.Y = screenCell[1];
                    destrec.Width = screenCell[2];
                    destrec.Height = screenCell[3];

                    if (gameBoard[x, y] == 1)
                    {
                        playerToken = Cross;
                    }
                    else if (gameBoard[x, y] == 2)
                    {
                        playerToken = Circle;
                    }
                    else
                    {
                        skip = true;
                    }

                    if (!skip)
                        spriteBatch.Draw(playerToken, destrec, Color.White);

                    skip = false;
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        // basic checking to know i f there is any epty cell on the board
        private bool CheckBoard()
        {
            bool canplay = false;

            for (Byte y = 0; y < 3 && canplay == false; y++)
            {
                for (Byte x = 0; x < 3 && canplay == false; x++)
                {
                    if (gameBoard[x, y] == 0)
                    {
                        canplay = true;
                    }
                }
            }

            return canplay;
        }
    }
}
