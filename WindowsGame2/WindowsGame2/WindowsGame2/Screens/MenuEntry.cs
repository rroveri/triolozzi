#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame2;
using Microsoft.Xna.Framework.Content;
#endregion

namespace WindowsGame2.Screens
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class MenuEntry
    {
        #region Fields

        /// <summary>
        /// The position at which the entry is drawn. This is set by the MenuScreen
        /// each frame in Update.
        /// </summary>
        Vector2 position;
        private Vector2 origin;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the position at which to draw this menu entry.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public PlayerIndexEventArgs PlayerIndexEvent { get; private set; }

        #endregion

        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<PlayerIndexEventArgs> Selected;

        public event EventHandler<PlayerIndexEventArgs> LeftClick;
        public event EventHandler<PlayerIndexEventArgs> RightClick;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        {
            PlayerIndexEvent.PlayerIndex = playerIndex;
            if (Selected != null)
                Selected(this, PlayerIndexEvent);
        }

        protected internal virtual void OnSelectLeft(PlayerIndex playerIndex)
        {
            if (_currentTextureIndex > 0)
            {
                _currentTextureIndex--;
            }
            PlayerIndexEvent.PlayerIndex = playerIndex;
            if (LeftClick != null)
                LeftClick(this, PlayerIndexEvent);
        }

        protected internal virtual void OnSelectRight(PlayerIndex playerIndex)
        {
            if (_currentTextureIndex < _textures.Length - 1)
            {
                _currentTextureIndex++;
            }
            PlayerIndexEvent.PlayerIndex = playerIndex;
            if (RightClick != null)
                RightClick(this, PlayerIndexEvent);
        }


        #endregion

        private Texture2D[] _textures;
        private Texture2D[] _selectedTextures;

        private int _currentTextureIndex;

        #region Initialization

        public MenuEntry(string[] textures, string[] selectedTextures)
        {
            origin = new Vector2(0f, 0f);
            PlayerIndexEvent = new PlayerIndexEventArgs(0);

            ContentManager content = GameServices.GetService<ContentManager>();

            _currentTextureIndex = 0;
            _textures = new Texture2D[textures.Length];
            _selectedTextures = new Texture2D[selectedTextures.Length];

            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = content.Load<Texture2D>(textures[i]);
                _selectedTextures[i] = content.Load<Texture2D>(selectedTextures[i]);
            }
            
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {

        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            
            if (isSelected)
            {
                spriteBatch.Draw(_selectedTextures[_currentTextureIndex], position, null, Color.White * screen.TransitionAlpha);
            }
            else
            {
                spriteBatch.Draw(_textures[_currentTextureIndex], position, null, Color.White * screen.TransitionAlpha);
            }
        }


        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(MenuScreen screen)
        {
            return _textures[_currentTextureIndex].Bounds.Height*2;
        }


        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public virtual int GetWidth(MenuScreen screen)
        {
            return _textures[_currentTextureIndex].Bounds.Width;
        }


        #endregion
    }
}
