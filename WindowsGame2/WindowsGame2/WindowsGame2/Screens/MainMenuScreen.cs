#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using Microsoft.Xna.Framework.Input;
#endregion

namespace WindowsGame2.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Fields

        private MenuEntry playersMenuEntry;
        private MenuEntry resolutionMenuEntry;
        private MenuEntry optionsMenuEntry;

        static int[] numberOfPlayers = { 2, 3, 4 };
        static string[] _newGameTextures = { "Images/MainMenu/start_new_game" };
        static string[] _newGameSelectedTextures = { "Images/MainMenu/start_new_game_selected" };

        static string[] _playersTextures = { "Images/MainMenu/2players", "Images/MainMenu/3players", "Images/MainMenu/4players" };
        static string[] _playersSelectedTextures = { "Images/MainMenu/2players_selected", "Images/MainMenu/3players_selected", "Images/MainMenu/4players_selected" };

        private Texture2D _exitButton;
        private Vector2 _exitButtonPosition;

        static int _playersCountIndex = 2;

        InputAction exitAction;

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen() : base("")
        {
            exitAction = new InputAction(
                new Buttons[] { Buttons.X, Buttons.Back },
                new Keys[] { Keys.Escape, Keys.X },
                true);

            optionsMenuEntry = new MenuEntry(_newGameTextures, _newGameSelectedTextures);
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            MenuEntries.Add(optionsMenuEntry);

            playersMenuEntry = new MenuEntry(_playersTextures, _playersSelectedTextures);
            playersMenuEntry.LeftClick += PlayersMenuEntryDecrement;
            playersMenuEntry.RightClick += PlayersMenuEntryIncrement;
            MenuEntries.Add(playersMenuEntry);

            resolutionMenuEntry = new MenuEntry(_playersTextures, _playersSelectedTextures);
            resolutionMenuEntry.LeftClick += PlayersMenuEntryDecrement;
            resolutionMenuEntry.RightClick += PlayersMenuEntryIncrement;
            MenuEntries.Add(resolutionMenuEntry);

            _exitButton = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/MainMenu/exit_menu");
            _exitButtonPosition = new Vector2(100, 850);

            GameServices.GetService<SoundManager>().PlaySong(SoundManager.MenuSong, true);
        }

        #endregion

        #region Handle Input

        void PlayersMenuEntryDecrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex > 0)
            {
                _playersCountIndex--;
            }
        }

        void PlayersMenuEntryIncrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex < numberOfPlayers.Length - 1)
            {
                _playersCountIndex++;
            }
        }

        void ResolutionMenuEntryDecrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex > 0)
            {
                _playersCountIndex--;
            }
        }

        void ResolutionMenuEntryIncrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex < 2)
            {
                _playersCountIndex++;
            }
        }

        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.GetScreen<OptionsMenuScreen>().ShowOptions(numberOfPlayers[_playersCountIndex]);
            ScreenManager.ShowScreen<OptionsMenuScreen>();
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;
            if (exitAction.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                ScreenManager.Game.Exit();
            }
            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, scaleMatrix);
            ScreenManager.SpriteBatch.Draw(_exitButton, _exitButtonPosition, null, Color.White);
            ScreenManager.SpriteBatch.End();
        }

        #endregion
    }
}
