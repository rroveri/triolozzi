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

        private Texture2D _gameControls;
        private Rectangle _gameControlsPosition;

        private bool _isReady;

        private InputAction StartAction;

        #endregion

        #region Initialization

        public override void LoadContent()
        {
            base.LoadContent();
            ContentManager content = GameServices.GetService<ContentManager>();
            _backgroundTexture = content.Load<Texture2D>("Images/bgNew");
            _loadingMessage = content.Load<Texture2D>("Images/PimpScreen/loadingMessage");
            _readyToPlayMessage = content.Load<Texture2D>("Images/PimpScreen/startGameMessage");
            _gameControls = content.Load<Texture2D>("Images/PimpScreen/game_controls");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            _currentPosition = new Rectangle(viewport.Width/2 - 300, viewport.Height/2 + 200, 600, 150);
            _gameControlsPosition = new Rectangle(viewport.Width/2 - 450, viewport.Height/2 - 400, 900, 581);

            _currentTexture = _loadingMessage;

            StartAction = new InputAction(
                    new Buttons[] { Buttons.A },
                    new Keys[] { Keys.Enter },
                    true);
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

            spriteBatch.Begin();
            spriteBatch.Draw(_gameControls, _gameControlsPosition, null, Color.White);
            spriteBatch.Draw(_currentTexture, _currentPosition, null, Color.White);
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
                ScreenManager.ShowScreen<GameScreen>();
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
