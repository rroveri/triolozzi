#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace WindowsGame2.Screens
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        #region Initialization

        private MessageBoxScreen QuitDialog;

        static string[] _resumeTextures = { "Images/PauseMenu/resume_menu" };
        static string[] _resumeTexturesSelected = { "Images/PauseMenu/resume_menu_selected" };

        static string[] _quitTextures = { "Images/PauseMenu/quit_game" };
        static string[] _quitTexturesSelected = { "Images/PauseMenu/quit_game_selected" };

        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen() : base("")
        {
            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry(_resumeTextures, _resumeTexturesSelected);
            MenuEntry quitGameMenuEntry = new MenuEntry(_quitTextures, _quitTexturesSelected);
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnCancel;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);

            // Prepare the dialog for quitting the game.
            QuitDialog = new MessageBoxScreen();
            QuitDialog.Accepted += ConfirmQuitMessageBoxAccepted;
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(QuitDialog, ControllingPlayer);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. Go back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.QuitGame();
            GameServices.GetService<SoundManager>().StopSong();
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.ShowScreen<GameScreen>();
            GameServices.GetService<SoundManager>().ResumeSong();
        }

        #endregion
    }
}
