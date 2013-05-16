using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;

namespace WindowsGame2.Screens
{

    class OptionsMenuScreen : AbstractScreen
    {
        #region Fields

        private int PlayersCount { get; set; }

        private Texture2D _backgroundTexture;

        private Texture2D _title;
        private Rectangle _titlePosition;

        private Texture2D[] _postIts;
        private Texture2D _doneSelecting;
        private Texture2D _readyToPlay;

        private Texture2D _backMainMenu;
        private Rectangle _backMainMenuPosition;

        private Rectangle[] _2PostitsPositions;
        private Rectangle[] _3PostitsPositions;
        private Rectangle[] _4PostitsPositions;

        private Rectangle[][] _PostitsPositions;

        private Rectangle[] _2StringsPositions;
        private Rectangle[] _3StringsPositions;
        private Rectangle[] _4StringsPositions;

        private Rectangle[][] _StringsPositions;

        private bool[] _didSelectColor;

        private Color[] _availableColors;
        private int[] _selectedColors;

        private Texture2D[] _availableCars;
        private int[] _selectedCars;

        private Rectangle[] _2CarsPositions;
        private Rectangle[] _3CarsPositions;
        private Rectangle[] _4CarsPositions;

        private Rectangle[][] _CarsPositions;

        private SoundManager _soundManager;

        // Handle Game Pad Actions
        InputAction LeftPad;
        InputAction RightPad;
        InputAction UpPad;
        InputAction DownPad;
        InputAction BackAction;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
        {
            LeftPad = new InputAction(
                new Buttons[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft },
                new Keys[] { Keys.Left },
                true
                );
            RightPad = new InputAction(
                    new Buttons[] { Buttons.DPadRight, Buttons.LeftThumbstickRight },
                    new Keys[] { Keys.Right },
                    true
                    );
            UpPad = new InputAction(
                    new Buttons[] { Buttons.DPadUp, Buttons.LeftThumbstickUp },
                    new Keys[] { Keys.Up },
                    true);
            DownPad = new InputAction(
                    new Buttons[] { Buttons.DPadDown, Buttons.LeftThumbstickDown },
                    new Keys[] { Keys.Down },
                    true);
            BackAction = new InputAction(
                    new Buttons[] { Buttons.B, Buttons.Back },
                    new Keys[] { Keys.Escape },
                    true);
        }

        public void ShowOptions(int playersCount)
        {
            PlayersCount = playersCount;
            ResetOptions();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            ContentManager Content = GameServices.GetService<ContentManager>();
            _soundManager = GameServices.GetService<SoundManager>();

            _backgroundTexture = Content.Load<Texture2D>("Images/bgNew");
            _title = Content.Load<Texture2D>("Images/PimpScreen/pimp_screen_title");
            
            _postIts = new Texture2D[4];
            _doneSelecting = Content.Load<Texture2D>("Images/PimpScreen/done_selecting");
            _readyToPlay = Content.Load<Texture2D>("Images/PimpScreen/ready_to_play");
            _backMainMenu = Content.Load<Texture2D>("Images/PimpScreen/back_main_menu");

            _postIts[0] = Content.Load<Texture2D>("Images/PimpScreen/player1_postit");
            _postIts[1] = Content.Load<Texture2D>("Images/PimpScreen/player2_postit");
            _postIts[2] = Content.Load<Texture2D>("Images/PimpScreen/player3_postit");
            _postIts[3] = Content.Load<Texture2D>("Images/PimpScreen/player4_postit");

            GraphicsDevice device = GameServices.GetService<GraphicsDevice>();
            Viewport view = ScreenManager.GraphicsDevice.Viewport;
            int width = view.Width / 2;
            int height = view.Height / 2;
            int horizontalOffset = 200;
            int horizontalSize = 500;
            int verticalSize = 520;

            _titlePosition = new Rectangle(width - 450, 40, 900, 150);
            _backMainMenuPosition = new Rectangle(50, height*2 - 120, 450, 100);

            // Initialize post it positions
            _2PostitsPositions = new Rectangle[2];
            _3PostitsPositions = new Rectangle[3];
            _4PostitsPositions = new Rectangle[4];

            _2PostitsPositions[0] = new Rectangle(width - horizontalOffset - horizontalSize, height - verticalSize/2, horizontalSize, verticalSize);
            _2PostitsPositions[1] = new Rectangle(width + horizontalOffset, height - verticalSize/2, horizontalSize, verticalSize);

            horizontalOffset = 300;

            _3PostitsPositions[0] = new Rectangle(width - horizontalOffset - horizontalSize, height - verticalSize/2, horizontalSize, verticalSize);
            _3PostitsPositions[1] = new Rectangle(width - horizontalSize/2, height - verticalSize/2, horizontalSize, verticalSize);
            _3PostitsPositions[2] = new Rectangle(width + horizontalOffset, height - verticalSize/2, horizontalSize, verticalSize);

            horizontalOffset = 30;
            horizontalSize = 420;
            verticalSize = 440;

            _4PostitsPositions[0] = new Rectangle(width - (horizontalOffset + horizontalSize)*2, height - verticalSize/2, horizontalSize, verticalSize);
            _4PostitsPositions[1] = new Rectangle(width - horizontalOffset - horizontalSize, height - verticalSize/2, horizontalSize, verticalSize);
            _4PostitsPositions[2] = new Rectangle(width + horizontalOffset, height - verticalSize/2, horizontalSize, verticalSize);
            _4PostitsPositions[3] = new Rectangle(width + 2*horizontalOffset + horizontalSize, height - verticalSize/2, horizontalSize, verticalSize);

            _PostitsPositions = new Rectangle[3][];
            _PostitsPositions[0] = _2PostitsPositions;
            _PostitsPositions[1] = _3PostitsPositions;
            _PostitsPositions[2] = _4PostitsPositions;

            // Initialize strings positions
            int verticalOffset = 30;

            _2StringsPositions = new Rectangle[2];
            _3StringsPositions = new Rectangle[3];
            _4StringsPositions = new Rectangle[4];

            _StringsPositions = new Rectangle[3][];
            _StringsPositions[0] = _2StringsPositions;
            _StringsPositions[1] = _3StringsPositions;
            _StringsPositions[2] = _4StringsPositions;

            Rectangle postit;
            for (int i = 0; i < _StringsPositions.Length; i++)
            {
                for (int j = 0; j < _StringsPositions[i].Length; j++)
                {
                    postit = _PostitsPositions[i][j];
                    _StringsPositions[i][j] = new Rectangle(postit.Left, postit.Bottom + verticalOffset, 350, 70);
                }
            }

            // Initialize cars positions
            _2CarsPositions = new Rectangle[2];
            _3CarsPositions = new Rectangle[3];
            _4CarsPositions = new Rectangle[4];

            _CarsPositions = new Rectangle[3][];
            _CarsPositions[0] = _2CarsPositions;
            _CarsPositions[1] = _3CarsPositions;
            _CarsPositions[2] = _4CarsPositions;

            for (int i = 0; i < _CarsPositions.Length; i++)
            {
                for (int j = 0; j < _CarsPositions[i].Length; j++)
                {
                    postit = _PostitsPositions[i][j];
                    _CarsPositions[i][j] = new Rectangle(postit.Center.X - 50, postit.Center.Y - 25, 100, 50);
                }
            }

            // Initialize player selections
            _didSelectColor = new bool[4];

            // Initialize colors
            _availableColors = new Color[10];
            _availableColors[0] = new Color(0,0.4f,1,1);
            _availableColors[1] = Color.Red;
            _availableColors[2] = Color.Green;
            _availableColors[3] = Color.DarkKhaki;
            _availableColors[4] = Color.Purple;
            _availableColors[5] = Color.LightSeaGreen;
            _availableColors[6] = Color.Brown;
            _availableColors[7] = Color.Orange;
            _availableColors[8] = Color.GreenYellow;
            _availableColors[9] = Color.DarkOliveGreen;

            _selectedColors = new int[4];
            _selectedColors[0] = 0;
            _selectedColors[1] = 0;
            _selectedColors[2] = 0;
            _selectedColors[3] = 0;

            // Initialize cars
            _availableCars = new Texture2D[3];
            _availableCars[0] = Content.Load<Texture2D>("Cars/standardCar");
            _availableCars[1] = Content.Load<Texture2D>("Cars/spiderCar");
            _availableCars[2] = Content.Load<Texture2D>("Cars/pickUpCar"); 

            _selectedCars = new int[4];
            _selectedCars[0] = 0;
            _selectedCars[1] = 0;
            _selectedCars[2] = 0;
            _selectedCars[3] = 0;
        }

        #endregion

        #region Handle Input

        private void StartSandboxScreen()
        {
            var cars = new List<Tuple<Texture2D, Color>>();
            Texture2D selectedCar;
            Color selectedColor;
            Rectangle[] positions = _CarsPositions[PlayersCount-2];
            for (int i = 0; i < PlayersCount; i++)
            {
                selectedCar = _availableCars[_selectedCars[i]];
                selectedColor = _availableColors[_selectedColors[i]];

                cars.Add(new Tuple<Texture2D, Color>(selectedCar, selectedColor));
            }
            ScreenManager.AddScreen(new SandboxScreen(cars, _CarsPositions[PlayersCount-2]), null);
            ScreenManager.ShowScreen<SandboxScreen>();
        }

        private void StartGameCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //ScreenManager.GetScreen<LoadingScreen>().SetReady();
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            base.HandleInput(gameTime, input);
            PlayerIndex playerIndex;

            if (IsEveryoneReady())
            {
                //ScreenManager.ShowScreen<LoadingScreen>();
                //ScreenManager.GetScreen<LoadingScreen>().SetBusy();
                
                StartSandboxScreen();

                return;
            }

            for (int i = 0; i < PlayersCount; i++)
            {
                // Go back to the Main Menu
                if (BackAction.Evaluate(input, (PlayerIndex)i, out playerIndex))
                {
                    ScreenManager.ShowScreen<MainMenuScreen>();
                    return;
                }
                // Ignore everything if color has been selected already
                if (_didSelectColor[i])
                {
                    continue;
                }
                // Select the car and colors
                if (input.DidTouchButton(Buttons.A, i) || input.DidTouchKey(Keys.A, i))
                {
                    if (CanSelectColor(i, _selectedColors[i]))
                    {
                        _didSelectColor[i] = true;
                    }
                    else
                    {
                        //Console.WriteLine("Putative problem: " + i);
                    }
                }

                if (RightPad.Evaluate(input, (PlayerIndex)i, out playerIndex))
                {
                    _selectedCars[i] = (_selectedCars[i] == _availableCars.Length - 1) ? 0 : _selectedCars[i] + 1;
                }
                if (LeftPad.Evaluate(input, (PlayerIndex)i, out playerIndex))
                {
                    _selectedCars[i] = (_selectedCars[i] == 0) ? _availableCars.Length - 1 : _selectedCars[i] - 1;
                }
                if (UpPad.Evaluate(input, (PlayerIndex)i, out playerIndex))
                {
                    _selectedColors[i] = (_selectedColors[i] == _availableColors.Length - 1) ? 0 : _selectedColors[i] + 1;
                }
                if (DownPad.Evaluate(input, (PlayerIndex)i, out playerIndex))
                {
                    _selectedColors[i] = (_selectedColors[i] == 0) ? _availableColors.Length - 1 : _selectedColors[i] - 1;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            ScreenManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            ScreenManager.SpriteBatch.End();

            ScreenManager.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, ScreenManager.scaleMatrix);
            ScreenManager.SpriteBatch.Draw(_title, _titlePosition, null, Color.White);
            for (int i = 0; i < PlayersCount; i++)
            {
                // Draw the post it background
                ScreenManager.SpriteBatch.Draw(_postIts[i], _PostitsPositions[PlayersCount - 2][i], null, Color.White);

                // Draw the selected car with the selected color
                ScreenManager.SpriteBatch.Draw(_availableCars[_selectedCars[i]], _CarsPositions[PlayersCount - 2][i], null, _availableColors[_selectedColors[i]]);

                // Show "Done" or "Ready" string
                if (_didSelectColor[i])
                {
                    ScreenManager.SpriteBatch.Draw(_readyToPlay, _StringsPositions[PlayersCount - 2][i], null, Color.White);
                }
                else
                {
                    ScreenManager.SpriteBatch.Draw(_doneSelecting, _StringsPositions[PlayersCount - 2][i], null, Color.White);
                }
            }
            ScreenManager.SpriteBatch.Draw(_backMainMenu, _backMainMenuPosition, null, Color.White);
            ScreenManager.SpriteBatch.End();
        }

        #endregion

        private bool CanSelectColor(int playerIndex, int colorIndex)
        {
            // Did the player already select a color?
            if (_didSelectColor[playerIndex])
            {
                return false;
            }
            // Did any other player select the same color?
            for (int i = 0; i < PlayersCount; i++)
            {
                if (i == playerIndex)
                {
                    continue;
                }
                if (_didSelectColor[i] && _selectedColors[i] == colorIndex)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsEveryoneReady()
        {
            for (int i = 0; i < PlayersCount; i++)
            {
                if (!_didSelectColor[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void ResetOptions()
        {
            for (int j = 0; j < _didSelectColor.Length; j++)
            {
                _didSelectColor[j] = false;
                _selectedCars[j] = 0;
                _selectedColors[j] = 0;
            }
        }

    }
}
