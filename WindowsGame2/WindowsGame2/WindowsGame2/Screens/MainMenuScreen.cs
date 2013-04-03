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

        private MessageBoxScreen ExitDialog;

        static int[] numberOfPlayers = { 2, 3, 4 };
        static int currentNumberOfPlayers = 0;
        
        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("The Drunken Dream Maker (With a Cold)")
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Split Screen Game");
            MenuEntry singleScreenEntry = new MenuEntry("Single Screen Game");
            //MenuEntry optionsMenuEntry = new MenuEntry("Options");
            playersMenuEntry = new MenuEntry("Players");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            singleScreenEntry.Selected += PlayGameMenuSingleModeEntrySelected;
            //optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            playersMenuEntry.Selected += PlayersMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(singleScreenEntry);
            //MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(playersMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            UpdatePlayersCount();

            // Prepare the exit dialog.
            const string message = "Are you sure you want to exit the game?";
            ExitDialog = new MessageBoxScreen(message);
            ExitDialog.Accepted += ConfirmExitMessageBoxAccepted;
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // TODO: start game here
            ScreenManager.GetScreen<GameScreen>().SetGameMode(0);
            ScreenManager.ShowScreen<GameScreen>();
        }


        void PlayGameMenuSingleModeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // TODO: start game here
            ScreenManager.GetScreen<GameScreen>().SetGameMode(1);
            ScreenManager.ShowScreen<GameScreen>();
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        void PlayersMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentNumberOfPlayers = (currentNumberOfPlayers + 1) % numberOfPlayers.Length;
            UpdatePlayersCount();
        }

        private void UpdatePlayersCount()
        {
            playersMenuEntry.Text = numberOfPlayers[currentNumberOfPlayers] + " players";
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.AddScreen(ExitDialog, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        #endregion
    }
}
