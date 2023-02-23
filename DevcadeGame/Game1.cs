using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;

namespace DevcadeGame
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		// Game State
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

		// Colors
		private Color[] menuColors;

		// Keyboard State
		KeyboardState currentKB;
		KeyboardState previousKB;

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
			menuColors[0] = Color.Gold;

			// Menu Coords
			menuSelectorPos = 0;

			base.Initialize();
		}

		/// <summary>
		/// Does any setup prior to the first frame that needs loaded content.
		/// </summary>
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			arial24 = Content.Load<SpriteFont>("arial-24");

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

			// Game Finite State Machine
			#region GameState Update FSM
			switch (gameState)
            {
				// Menu ---------------------------------------------
				case GameState.Menu:

					// change the selected text
					if (currentKB.IsKeyDown(Keys.Down) && previousKB.IsKeyUp(Keys.Down)
						&& menuSelectorPos < 2)
                    {
						menuSelectorPos++;
                    }
					if (currentKB.IsKeyDown(Keys.Up) && previousKB.IsKeyUp(Keys.Up)
						&& menuSelectorPos > 0)
					{
						menuSelectorPos--;
					}

					// Select next gameState
					if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter))
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
					menuColors[menuSelectorPos] = Color.Gold;

					break;

				// Instructions -------------------------------------
				case GameState.Instructions:
					if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter))
					{
						gameState = GameState.Menu;
					}
					break;

				// Game ---------------------------------------------
				case GameState.Game:

					// TODO: PUT THE GAME STUFF HERE

					if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter))
					{
						gameState = GameState.GameOver;
					}
					break;

				// GameOver -----------------------------------------
				case GameState.GameOver:
					if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter))
					{
						gameState = GameState.Menu;
					}
					break;

				// Credits ------------------------------------------
				case GameState.Credits:
					if (currentKB.IsKeyDown(Keys.Enter) && previousKB.IsKeyUp(Keys.Enter))
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
        private void DrawMenu(SpriteBatch sb)
        {
			sb.DrawString(
				arial24,											// Font
				"Connect 7!",										// Text
				new Vector2(windowTileSize, 1 * windowTileSize),	// Position
				Color.Black);                                       // Color

			sb.DrawString(
				arial24,
				"Start Game",
				new Vector2(2 * windowTileSize, 4 * windowTileSize),
				menuColors[0]);

			sb.DrawString(
				arial24,
				"How to Play",
				new Vector2(2 * windowTileSize, 6 * windowTileSize),
				menuColors[1]);
			sb.DrawString(
				arial24,
				"Credits",
				new Vector2(2 * windowTileSize, 8 * windowTileSize),
				menuColors[2]);
		}

		private void DrawInstructions(SpriteBatch sb)
        {

        }

		private void DrawGame(SpriteBatch sb)
        {

        }

        private void DrawGameOver(SpriteBatch sb)
        {

        }

		private void DrawCredits(SpriteBatch sb)
        {

        }
		#endregion
	}
}