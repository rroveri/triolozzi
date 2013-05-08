using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using WindowsGame2.Screens;

namespace WindowsGame2
{
    class LoadingScreen : AbstractScreen
    {
        #region Fields

        private Texture2D _backgroundTexture;
        private Texture2D _loadingMessage;
        private Texture2D _readyToPlayMessage;

        private Texture2D _currentTexture;
        private Rectangle _currentPosition;

        private Texture2D[] _tutorials;
        private Rectangle[] _tutorialsPosition;
        private int currentTutorial = 0;

        private Vector2 zeroVector = new Vector2();

        private bool _isReady;

        private InputAction StartAction, switchTutorialAction;
        private Texture2D _switchTip;
        private Rectangle _switchTipPosition;

        #endregion

        #region Initialization

        public override void LoadContent()
        {
            base.LoadContent();
            ContentManager content = GameServices.GetService<ContentManager>();
            _backgroundTexture = content.Load<Texture2D>("Images/bgNew");
            _loadingMessage = content.Load<Texture2D>("Images/PimpScreen/loadingMessage");
            _switchTip = content.Load<Texture2D>("Images/PimpScreen/tutorialSwitchMessage");
            _readyToPlayMessage = content.Load<Texture2D>("Images/PimpScreen/startGameMessage");

            _tutorials = new Texture2D[4];
            _tutorials[0] = content.Load<Texture2D>("Images/PimpScreen/game_controls");
            _tutorials[1] = content.Load<Texture2D>("Images/PimpScreen/tutorial1");
            _tutorials[2] = content.Load<Texture2D>("Images/PimpScreen/tutorial2");
            _tutorials[3] = content.Load<Texture2D>("Images/PimpScreen/tutorial3");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            _currentPosition = new Rectangle(1920/2 - 300, 1080/2 - 500, 600, 150);
            _switchTipPosition = new Rectangle(1920 / 2 - 850, 1080 / 2 + 400, 500, 150);

            _tutorialsPosition = new Rectangle[4];
            _tutorialsPosition[0] = new Rectangle(1920 / 2 - 450, 1080 / 2 - 300, 900, 581);
            _tutorialsPosition[1] = new Rectangle(0, 0, 1920, 1080);
            _tutorialsPosition[2] = new Rectangle(0, 0, 1920, 1080);
            _tutorialsPosition[3] = new Rectangle(0, 0, 1920, 1080);

            _currentTexture = _loadingMessage;

            StartAction = new InputAction(
                    new Buttons[] { Buttons.A },
                    new Keys[] { Keys.A },
                    true);

            switchTutorialAction = new InputAction(new Buttons[] { Buttons.X }, new Keys[] { Keys.X }, true);
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Draws the loading screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, ScreenManager.scaleMatrix);
            spriteBatch.Draw(_tutorials[currentTutorial], _tutorialsPosition[currentTutorial], null, Color.White);
            spriteBatch.Draw(_currentTexture, _currentPosition, null, Color.White);
            spriteBatch.Draw(_switchTip, _switchTipPosition, null, Color.White);
            spriteBatch.End();
        }

        #endregion

        #region Handle Input

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;
            base.HandleInput(gameTime, input);
            if (_isReady && StartAction.Evaluate(input, null, out playerIndex))
            {
             //   GameServices.GetService<SoundManager>().PlaySong(SoundManager.GameSong, true);
                GameServices.GetService<SoundManager>().StopSong();
                ScreenManager.ShowScreen<GameScreen>();
            }
            if (switchTutorialAction.Evaluate(input, null, out playerIndex))
            {
                //   GameServices.GetService<SoundManager>().PlaySong(SoundManager.GameSong, true);
                currentTutorial++;
                if (currentTutorial >= _tutorials.Length) currentTutorial = 0;
            }
        }

        public void SetReady()
        {
            _currentTexture = _readyToPlayMessage;
            _isReady = true;
        }

        public void SetBusy()
        {
            _currentTexture = _loadingMessage;
            _isReady = false;
        }

        #endregion
    }
}
