using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace WindowsGame2.Screens
{
    class CreditsScreen : AbstractScreen
    {
        private Texture2D _creditsTexture;

        private InputAction backAction;

        public CreditsScreen()
        {
            backAction = new InputAction(
                    new Buttons[] { Buttons.X },
                    new Keys[] { Keys.X },
                    true);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            ContentManager content = GameServices.GetService<ContentManager>();
            _creditsTexture = content.Load<Texture2D>("Images/Credits");
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            spriteBatch.Draw(_creditsTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            spriteBatch.End();
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            base.HandleInput(gameTime, input);
            PlayerIndex playerIndex;
            if (backAction.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                ScreenManager.ShowScreen<MainMenuScreen>();
            }
        }
    }
}
