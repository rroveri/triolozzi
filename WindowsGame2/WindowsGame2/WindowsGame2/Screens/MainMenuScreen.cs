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
        private MenuEntry optionsMenuEntry;

        static int[] numberOfPlayers = { 2, 3, 4 };
        static string[] _playersText = {"< 2 Players >", "< 3 Players >", "< 4 Players >" };
        static int _playersCountIndex = 2;

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen() : base("The Drunken Dream Maker (With a Cold)")
        {
            optionsMenuEntry = new MenuEntry("Start New Game");
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            MenuEntries.Add(optionsMenuEntry);

            playersMenuEntry = new MenuEntry("Players");
            playersMenuEntry.LeftClick += PlayersMenuEntryDecrement;
            playersMenuEntry.RightClick += PlayersMenuEntryIncrement;
            MenuEntries.Add(playersMenuEntry);

            MenuEntry exitMenuEntry = new MenuEntry("Exit");
            exitMenuEntry.Selected += OnCancel;
            MenuEntries.Add(exitMenuEntry);

            UpdatePlayersCount();

            GameServices.GetService<SoundManager>().PlaySong(SoundManager.MenuSong, true);
        }

        #endregion

        #region Handle Input

        void PlayersMenuEntryDecrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex > 0)
            {
                _playersCountIndex--;
                UpdatePlayersCount();
            }
        }

        void PlayersMenuEntryIncrement(object sender, PlayerIndexEventArgs e)
        {
            if (_playersCountIndex < numberOfPlayers.Length - 1)
            {
                _playersCountIndex++;
                UpdatePlayersCount();
            }
        }

        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.GetScreen<OptionsMenuScreen>().ShowOptions(numberOfPlayers[_playersCountIndex]);
            ScreenManager.ShowScreen<OptionsMenuScreen>();
        }

        private void UpdatePlayersCount()
        {
            playersMenuEntry.Text = _playersText[_playersCountIndex];
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }

        #endregion
    }
}
