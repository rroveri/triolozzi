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
#endregion

namespace WindowsGame2.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {

        #region Fields

        MenuEntry playersMenuEntry;

        //private MessageBoxScreen ExitDialog;

        static int[] numberOfPlayers = { 2, 3, 4 };
        static string[] _playersText = {"2 Players", "3 Players", "4 Players" };
        static int _playersCountIndex = 2;
        
        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen() : base("The Drunken Dream Maker (With a Cold)")
        {
            //MenuEntry playGameMenuEntry = new MenuEntry("Split Screen Game");
            //playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            //MenuEntries.Add(playGameMenuEntry);

            MenuEntry singleScreenEntry = new MenuEntry("Single Screen Game");
            singleScreenEntry.Selected += PlayGameMenuSingleModeEntrySelected;
            MenuEntries.Add(singleScreenEntry);
            
            //MenuEntry optionsMenuEntry = new MenuEntry("Options");
            //optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            //MenuEntries.Add(optionsMenuEntry);

            playersMenuEntry = new MenuEntry("Players");
            playersMenuEntry.Selected += PlayersMenuEntrySelected;
            MenuEntries.Add(playersMenuEntry);

            MenuEntry exitMenuEntry = new MenuEntry("Exit");
            exitMenuEntry.Selected += OnCancel;
            MenuEntries.Add(exitMenuEntry);

            UpdatePlayersCount();

            // Prepare the exit dialog.
            //const string message = "Are you sure you want to exit the game?";
            //ExitDialog = new MessageBoxScreen(message);
            //ExitDialog.Accepted += ConfirmExitMessageBoxAccepted;
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        //void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    // TODO: start game here
        //    ScreenManager.GetScreen<GameScreen>().PlayersCount = numberOfPlayers[_playersCountIndex];
        //    ScreenManager.ShowScreen<GameScreen>();
        //}


        void PlayGameMenuSingleModeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.GetScreen<GameScreen>().PlayersCount = numberOfPlayers[_playersCountIndex];
            ScreenManager.ShowScreen<GameScreen>();
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        //void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        //}

        void PlayersMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            _playersCountIndex = (_playersCountIndex + 1) % numberOfPlayers.Length;
            UpdatePlayersCount();
        }

        private void UpdatePlayersCount()
        {
            playersMenuEntry.Text = _playersText[_playersCountIndex];
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
            //ScreenManager.AddScreen(ExitDialog, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        //void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        //{
        //    ScreenManager.Game.Exit();
        //}

        #endregion
    }
}
