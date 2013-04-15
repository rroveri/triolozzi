﻿using System;
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

        string message;
        Texture2D gradientTexture;

        InputAction menuSelect;

        private string[] _carColors = { "Red", "Blue", "Green", "Brown" };
        private string[] _ranks = { "1st: ", "2nd: ", "3rd: ", "4th: " };

        private StringBuilder _rankingText;

        #endregion

        #region Events

        public event EventHandler<PlayerIndexEventArgs> Accepted;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public RankingScreen(string message)
        {
            this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            menuSelect = new InputAction(
                new Buttons[] { Buttons.A, Buttons.Start },
                new Keys[] { Keys.Space, Keys.Enter },
                true);

            _rankingText = new StringBuilder();
        }

        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            gradientTexture = content.Load<Texture2D>("Images/gradient");
        }

        public void UpdateRankings(List<Car> Cars)
        {
            List<Car> sortedCars = Cars.OrderByDescending(c => c.score).ToList();
            _rankingText.Remove(0, _rankingText.Length);
            _rankingText.Append(message);
            _rankingText.Append('\n');
            for (int i = 0; i < sortedCars.Count; i++)
            {
                _rankingText.Append(_ranks[i]);
                _rankingText.Append(_carColors[Cars.IndexOf(sortedCars[i])]);
                _rankingText.Append('\n');
            }
            _rankingText.Append("Press Start to return to the main menu");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input when cancelling the ranking screen.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if (menuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, new PlayerIndexEventArgs(playerIndex));

                ExitScreen();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(_rankingText.ToString());
            Vector2 textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

            // Draw the message box text.
            spriteBatch.DrawString(font, _rankingText.ToString(), textPosition, color);

            spriteBatch.End();
        }


        #endregion
    }
}
