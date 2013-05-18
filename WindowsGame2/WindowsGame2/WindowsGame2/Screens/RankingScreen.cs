using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame2;
using WindowsGame2.GameElements;

namespace WindowsGame2.Screens
{
    /// <summary>
    /// A screen that displays the final ranking of a race.
    /// </summary>
    class RankingScreen : AbstractScreen
    {
        #region Fields

        Texture2D gradientTexture;
        private Rectangle _bgPosition;

        InputAction menuSelect;

        private Rectangle[] _carPositions;
        private List<Car> _cars;

        public event EventHandler<PlayerIndexEventArgs> Accepted;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public RankingScreen()
        {
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            menuSelect = new InputAction(
                new Buttons[] { Buttons.A, Buttons.Start },
                new Keys[] { Keys.Space, Keys.Enter },
                true);
        }

        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            gradientTexture = content.Load<Texture2D>("Images/rankingBG");

            int width = 1920 / 2;
            int height = 1080 / 2;
            int carWidth = 130;
            int carHeight = 80;
            int offset = 180;
            int top = 480;

            _carPositions = new Rectangle[4];
            _carPositions[0] = new Rectangle(width - carHeight/2, top, carWidth, carHeight);

            carWidth = 117; carHeight = 72; top += 70;
            _carPositions[1] = new Rectangle(width - offset - carHeight, top, carWidth, carHeight);

            carWidth = 104; carHeight = 64; top += 40;
            _carPositions[2] = new Rectangle(width + offset, top, carWidth, carHeight);

            carWidth = 78; carHeight = 48; top += 150;
            _carPositions[3] = new Rectangle(width - carHeight/2, top, carWidth, carHeight);

            _bgPosition = new Rectangle(width - gradientTexture.Width / 2, height - gradientTexture.Height / 2, gradientTexture.Width, gradientTexture.Height);
        }

        public void UpdateRankings(List<Car> Cars)
        {
            _cars = Cars.OrderByDescending(c => c.score).ToList();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input when cancelling the ranking screen.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;

            if (menuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, new PlayerIndexEventArgs(playerIndex));
            }
        }


        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 1 / 3);

            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, ScreenManager.scaleMatrix);

            spriteBatch.Draw(gradientTexture, _bgPosition, Color.White * TransitionAlpha);

            for (int i = 0; i < _cars.Count; i++)
            {
                spriteBatch.Draw(_cars[i]._polygonTexture, _carPositions[i], null, _cars[i].mColor, -(float)Math.PI/2, Vector2.Zero, SpriteEffects.None, 1f);
            }

            spriteBatch.End();
        }


        #endregion
    }
}
