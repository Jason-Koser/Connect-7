﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;
using System.IO.IsolatedStorage;
using System.Threading.Tasks.Sources;

namespace DevcadeGame
{
    public class Game1 : Game
    {
        // Andrew Ebersole & Jason Koser
        // 2.23.23
        // Connect 7 the game

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game State Enum
        private enum GameState
        {
            Menu,
            Game,
            GameOver,
            Instructions,
            Credits
        }
        private GameState gameState;

        // Window Dimensions
        private int windowHeight;
        private int windowWidth;
        private int windowTileSize;

        // Menu Selector
        private int menuSelectorPos;

        // Texture 2Ds
        Texture2D singleColor;

        // Fonts
        SpriteFont arial24;
        SpriteFont arial10;

        // Colors
        private Color[] menuColors;

        // Keyboard State
        KeyboardState currentKB;
        KeyboardState previousKB;

        private int selectorColumn;
        private Meatball currentMeatball;
        private Meatball[,] board; //main board of column, row

        //Anti-Magic numbers
        private int rows;
        private int cols;

        // Game Stuff
        private int p1Score;
        private int p2Score;

        // Textures
        Texture2D whiteMeatball;
        Texture2D redMeatball;
        Texture2D tile;

        // Meatball Spinner
        double meatballSpinner;

        /// <summary>
        /// Game constructor
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        /// <summary>
        /// Does any setup prior to the first frame that doesn't need loaded content.
        /// </summary>
        protected override void Initialize()
        {
            Input.Initialize(); // Sets up the input library

            // Set window size if running debug (in release it will be fullscreen)
            #region
#if DEBUG
            _graphics.PreferredBackBufferWidth = 420;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
#else
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();
#endif
            #endregion

            // Set gamestate
            gameState = GameState.Menu;

            // Window Dimensions
            windowHeight = _graphics.PreferredBackBufferHeight;
            windowWidth = _graphics.PreferredBackBufferWidth;
            windowTileSize = windowWidth / 7;

            // Create single Color Texture
            singleColor = new Texture2D(GraphicsDevice, 1, 1);
            singleColor.SetData(new Color[] { Color.White });

            // Menu Colors
            menuColors = new Color[5];
            for (int i = 0; i < menuColors.Length; i++)
            {
                menuColors[i] = Color.Black;
            }
            menuColors[0] = Color.DarkGoldenrod;

            // Menu Coords
            menuSelectorPos = 0;

            p1Score = 0;
            p2Score = 0;

            // Create grid of meatballs
            rows = 7;
            cols = 14;

            // MeatballSpinner
            meatballSpinner = 0f;

            board = new Meatball[7, 14];
            base.Initialize();
        }

        /// <summary>
        /// Does any setup prior to the first frame that needs loaded content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            arial24 = Content.Load<SpriteFont>("arial-24");
            arial10 = Content.Load<SpriteFont>("Arial-10");

            redMeatball = Content.Load<Texture2D>("redMeat");
            whiteMeatball = Content.Load<Texture2D>("whiteMeat");
            tile = Content.Load<Texture2D>("Tile");

            // Current Meatball
            currentMeatball = new Meatball(redMeatball, 0, 0, 0, 0);
        }

        /// <summary>
        /// Your main update loop. This runs once every frame, over and over.
        /// </summary>
        /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.Update(); // Updates the state of the input library

            // Exit when both menu buttons are pressed (or escape for keyboard debuging)
            // You can change this but it is suggested to keep the keybind of both menu
            // buttons at once for gracefull exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
                Input.GetButton(2, Input.ArcadeButtons.Menu)))
            {
                Exit();
            }

            currentKB = Keyboard.GetState();

            // GameState Finite State Machine
            #region GameState Update FSM
            switch (gameState)
            {
                // Menu ---------------------------------------------
                case GameState.Menu:

                    // change the selected text
                    if (currentKB.IsKeyDown(Keys.Down) && previousKB.IsKeyUp(Keys.Down)
                        || Input.GetButton(1, Input.ArcadeButtons.StickDown)
                        || Input.GetButton(2, Input.ArcadeButtons.StickDown))
                    {
                        if (menuSelectorPos < 2)
                        {
                            menuSelectorPos++;
                        }
                        else if (menuSelectorPos == 2)
                        {
                            menuSelectorPos = 0;
                        }
                    }
                    if (currentKB.IsKeyDown(Keys.Up) && previousKB.IsKeyUp(Keys.Up)
                        || Input.GetButton(1, Input.ArcadeButtons.StickUp)
                        || Input.GetButton(2, Input.ArcadeButtons.StickUp))
                    {
                        if (menuSelectorPos > 0)
                        {
                            menuSelectorPos--;
                        }
                        else if (menuSelectorPos == 0)
                        {
                            menuSelectorPos = 2;
                        }
                    }

                    // Select next gameState
                    if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter)
                        || PressAnyButton(1)
                        || PressAnyButton(2))
                    {
                        switch (menuSelectorPos)
                        {
                            case 0:
                                gameState = GameState.Game;
                                break;

                            case 1:
                                gameState = GameState.Instructions;
                                break;

                            case 2:
                                gameState = GameState.Credits;
                                break;
                        }
                    }

                    // Change selected text to gold all others to black
                    for (int i = 0; i < menuColors.Length; i++)
                    {
                        menuColors[i] = Color.Black;
                    }
                    menuColors[menuSelectorPos] = Color.DarkGoldenrod;

                    break;

                // Instructions -------------------------------------
                case GameState.Instructions:
                    if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter)
                        || PressAnyButton(1)
                        || PressAnyButton(2))
                    {
                        gameState = GameState.Menu;
                    }
                    break;

                // Game ---------------------------------------------
                case GameState.Game:

                    meatballSpinner += 1f * gameTime.ElapsedGameTime.TotalSeconds;
                    // TODO: PUT THE GAME STUFF HERE
                    if (currentKB.IsKeyDown(Keys.Left) && previousKB.IsKeyUp(Keys.Left)
                        && selectorColumn > 0)
                    {
                        selectorColumn--;
                    }
                    if (currentKB.IsKeyDown(Keys.Right) && previousKB.IsKeyUp(Keys.Right)
                        && selectorColumn < 6)
                    {
                        selectorColumn++;
                    }

                    // Quit game
                    if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter)
                        || Input.GetButton(1, Input.ArcadeButtons.Menu)
                        || Input.GetButton(2, Input.ArcadeButtons.Menu))
                    {
                        gameState = GameState.GameOver;
                    }
                    break;

                // GameOver -----------------------------------------
                case GameState.GameOver:
                    if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter)
                        || PressAnyButton(1)
                        || PressAnyButton(2))
                    {
                        gameState = GameState.Menu;
                    }
                    break;

                // Credits ------------------------------------------
                case GameState.Credits:
                    if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter)
                        || PressAnyButton(1)
                        || PressAnyButton(2))
                    {
                        gameState = GameState.Menu;
                    }
                    break;
            }
            #endregion

            previousKB = currentKB;
            base.Update(gameTime);
        }

        /// <summary>
        /// Your main draw loop. This runs once every frame, over and over.
        /// </summary>
        /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

            // Draw Finite State Machine
            #region GameState Draw FSM
            switch (gameState)
            {
                // Menu ---------------------------------------------
                case GameState.Menu:
                    DrawMenu(_spriteBatch);
                    break;

                // Instructions -------------------------------------
                case GameState.Instructions:
                    DrawInstructions(_spriteBatch);

                    break;

                // Game ---------------------------------------------
                case GameState.Game:
                    DrawGame(_spriteBatch);
                    break;

                // GameOver -----------------------------------------
                case GameState.GameOver:
                    DrawGameOver(_spriteBatch);
                    break;

                // Credits ------------------------------------------
                case GameState.Credits:
                    DrawCredits(_spriteBatch);
                    break;
            }
            #endregion

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Draw GameState Methods
        /// <summary>
        /// Draws all the text for the menu
        /// </summary>
        /// <param name="sb">Sprite batch is used by monogame to draw sprites and text</param>
        private void DrawMenu(SpriteBatch sb)
        {
            sb.DrawString(
                arial24,                                            // Font
                "Connect 7!",                                       // Text
                new Vector2(2 * windowTileSize, 4 * windowTileSize),// Position
                Color.Black);                                       // Color

            sb.DrawString(
                arial24,
                "Start Game",
                new Vector2(1 * windowTileSize, 7 * windowTileSize),
                menuColors[0]);

            sb.DrawString(
                arial24,
                "How to Play",
                new Vector2(1 * windowTileSize, 8 * windowTileSize),
                menuColors[1]);
            sb.DrawString(
                arial24,
                "Credits",
                new Vector2(1 * windowTileSize, 9 * windowTileSize),
                menuColors[2]);
        }

        /// <summary>
        /// Draws all the text for instructions screen
        /// </summary>
        /// <param name="sb">Sprite batch is used by monogame to draw sprites and text</param>
        private void DrawInstructions(SpriteBatch sb)
        {
            sb.DrawString(
                arial24,                                            // Font
                "Instructions",                                     // Text
                new Vector2(2 * windowTileSize, 3 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Use left and right to select" +
                "\na column. click any button " +
                "\nto confirm placement. the " +
                "\nfirst player to get 7 in a" +
                "\nrow wins. If neither player " +
                "\ndoes then the player with" +
                "\nthe most points wins",                               //Text
                new Vector2(0.5f * windowTileSize, 5 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Points",                                           // Text
                new Vector2(2 * windowTileSize, 10 * windowTileSize),// Position
                Color.Black);

            sb.DrawString(
                arial24,                                            // Font
                "Connect 4 - 1 Point" +
                "\nConnect 5 - 2 Points" +
                "\nConnect 6 - 5 points" +
                "\nConnect 7 - Win",                                // Text
                new Vector2(1 * windowTileSize, 11 * windowTileSize),// Position
                Color.Black);                                       //Color

        }

        /// <summary>
        ///  Draws the assest and text during the game
        /// </summary>
        /// <param name="sb">Sprite batch is used by monogame to draw sprites and text</param>
        private void DrawGame(SpriteBatch sb)
        {
            // Draw Scores
            sb.DrawString(
                arial24,
                $"Beef: {p1Score}",
                new Vector2(windowTileSize * 0.1f,
                windowTileSize * 0.1f),
                Color.IndianRed);
            sb.DrawString(
                arial24,
                $"Chicken: {p2Score}",
                new Vector2(windowTileSize * 3.6f,
                windowTileSize * 0.1f),
                Color.SlateGray);

            // Draw Meatballs
            sb.Draw(currentMeatball.Texture,
                new Rectangle((int)(windowTileSize * selectorColumn + windowTileSize * 0.5f),
                (int)(windowTileSize * 1.5f),
                (int)(windowTileSize * 0.8f),
                (int)(windowTileSize * 0.8f)),
                null,
                Color.White,
                (float)meatballSpinner,
                new Vector2(currentMeatball.Texture.Width / 2,currentMeatball.Texture.Height / 2),
                SpriteEffects.None,
                0);

            // Draw Tiles
            for (int row = 0; row < 14; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    sb.Draw(
                        tile,
                        new Rectangle(windowTileSize * col,
                        windowTileSize * 2 + windowTileSize * row,
                        windowTileSize,
                        windowTileSize),
                        Color.White);
                }
            }

            // Draw Bottom Rect
            sb.Draw(
                singleColor,
                new Rectangle(0,
                windowTileSize * 16,
                windowWidth,
                windowTileSize),
                Color.Black);

            // Draw Bottom text
            sb.DrawString(
                arial10,
                "Use Stick to move, A1 to drop, and Left Menu to exit",
                new Vector2(windowTileSize * 0.1f,
                windowTileSize * 16.1f),
                Color.White);
        }

        /// <summary>
        /// Draws the text for game over screen and displays who won
        /// </summary>
        /// <param name="sb">Sprite batch is used by monogame to draw sprites and text</param>
        private void DrawGameOver(SpriteBatch sb)
        {
            sb.DrawString(
                arial24,                                            // Font
                "Game Over!",                                       // Text
                new Vector2(2 * windowTileSize, 3 * windowTileSize),// Position
                Color.Black);                                       //Color

            string winner = "";
            Color winnerColor = new Color();
            if (p1Score > p2Score)
            {
                winner = "Beef Won!";
                winnerColor = Color.IndianRed;
            } else if (p1Score < p2Score)
            {
                winner = "Chicken Won!";
                winnerColor = Color.SlateGray;
            } else
            {
                winner = "Its a tie :(";
                winnerColor = Color.Black;
            }

            sb.DrawString(
                arial24,                                                // Font
                winner,                                          //Text
                new Vector2(0.5f * windowTileSize, 5 * windowTileSize), // Position
                winnerColor);                                       //Color
        }

        /// <summary>
        /// Draws the text for credits to show who worked on the game or who created assets used
        /// </summary>
        /// <param name="sb">Sprite batch is used by monogame to draw sprites and text</param>
        private void DrawCredits(SpriteBatch sb)
        {
            sb.DrawString(
                arial24,                                            // Font
                "Credits",                                     // Text
                new Vector2(2 * windowTileSize, 3 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Code",                                           // Text
                new Vector2(2 * windowTileSize, 5 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Jason Koser" +
                "\nAndrew Ebersole",                               // Text
                new Vector2(1 * windowTileSize, 6 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Art",                                           // Text
                new Vector2(2 * windowTileSize, 8 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Fill in later",                               // Text
                new Vector2(1 * windowTileSize, 9 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Sound",                                           // Text
                new Vector2(2 * windowTileSize, 11 * windowTileSize),// Position
                Color.Black);                                       //Color

            sb.DrawString(
                arial24,                                            // Font
                "Fill in later",                               // Text
                new Vector2(1 * windowTileSize, 12 * windowTileSize),// Position
                Color.Black);                                       //Color
        }
        #endregion

        /// <summary>
        /// Used to tell if any of the any of the buttons have been pressed 
        /// </summary>
        /// <param name="playerNum"> The player whose buttons to check </param>
        /// <returns> true if any button has been pressed false if not </returns>
        private bool PressAnyButton(int playerNum)
        {
            if (Input.GetButton(playerNum, Input.ArcadeButtons.A1)
                || Input.GetButton(playerNum, Input.ArcadeButtons.A2)
                || Input.GetButton(playerNum, Input.ArcadeButtons.A3)
                || Input.GetButton(playerNum, Input.ArcadeButtons.A4)
                || Input.GetButton(playerNum, Input.ArcadeButtons.B1)
                || Input.GetButton(playerNum, Input.ArcadeButtons.B2)
                || Input.GetButton(playerNum, Input.ArcadeButtons.B3)
                || Input.GetButton(playerNum, Input.ArcadeButtons.B4))
            {
                return true;
            }
            return false;
        }

        private bool dropMeatball(Meatball meatball, int column)
        {
            // go through the rows of the board in the given column to find the first null spot
            for (int row = 0; row < rows; row++)
            {
                if (board[column, row] == null)
                {
                    board[column, row] = meatball;
                    addScore(meatball, column, row);
                    return true;
                }
            }
            return false;
        }//end of drop meatball

        private void addScore(Meatball meatball, int currentCol, int currentRow)
        {
            int newCol = currentCol;
            int newRol = currentRow;
            int inARow = 1;

            // Calculate top right and bottom left
            // Calculate top right
            while (true)
            {
                // we dont want to go out of bounds
                if (currentCol == cols || currentRow == rows)
                {
                    break;
                }

                // if the next isnt a meatball we break
                newCol++;
                newRol++;
                if (!board[newCol, newRol].Equals(meatball))
                {
                    break;
                }

                inARow++;
            }
        }
        // Calculate bottom left
    } // end of class
}//end of namespace