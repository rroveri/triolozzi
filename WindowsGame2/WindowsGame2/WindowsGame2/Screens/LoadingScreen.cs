using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame2
{
    class LoadingScreen : AbstractScreen
    {
        #region Fields

        private Texture2D _backgroundTexture;

        #endregion

        #region Initialization

        public override void LoadContent()
        {
            base.LoadContent();
            _backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/bgNew");
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the loading screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            
        }


        /// <summary>
        /// Draws the loading screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            const string message = "Loading...";

            // Center the text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            spriteBatch.End();

            // Draw the text.
            spriteBatch.Begin();
            spriteBatch.DrawString(font, message, textPosition, Color.Black);
            spriteBatch.End();
        }

        #endregion
    }
}
