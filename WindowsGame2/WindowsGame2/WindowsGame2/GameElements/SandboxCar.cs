using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame2.GameElements
{
    class SandboxCar : TexturePhysicsObject
    {

        #region Fields

        private GamePadState _gamePad;
        private KeyboardState _keyboard;
        private InputAction _readyAction;

        private const float _rotationVelocity = 0.12f;
        private const float _defaultAcceleration = 0.4f;
        private const float _maximumVelocity = 8.5f;
        private const float _force = 1f;

        public Vector2 _forceVector;
        public Vector2 _direction;

        private float _acceleration;

        // Is the player ready to join the game?
        // If true, the car should stop moving.
        private bool _isReady;

        /// <summary>
        /// Event for when a player is ready to join the game.
        /// </summary>
        public event EventHandler<ReadyToPlayEventArgs> OnReadyToPlay;

        #endregion

        public SandboxCar(World world, Texture2D texture, Color Color) : base(world, texture, new Vector2(65.0f, 40.0f), Color)
        {
            _acceleration = _defaultAcceleration;

            _forceVector = new Vector2();
            _direction = new Vector2();

            _compound.LinearDamping = 1;
            _compound.AngularDamping = 1;

            _isReady = false;

            _readyAction = new InputAction(new Buttons[] { Buttons.A }, new Keys[] { Keys.A }, true);
        }

        public void Update(GameTime gameTime, PlayerIndex playerIndex)
        {
            _gamePad = GamePad.GetState(playerIndex);
            _keyboard = Keyboard.GetState();

            // Check if the player is ready to play
            if (_isReady) return;

            _direction.X = (float)Math.Cos(_compound.Rotation);
            _direction.Y = (float)Math.Sin(_compound.Rotation);

            _forceVector = _direction * _force;

            float newAcceleration = 0f;

            if (GoesLeft())
            {
                _compound.AngularVelocity = 0;
                _compound.Rotation -= _rotationVelocity;
            }

            if (GoesRight())
            {
                _compound.AngularVelocity = 0;
                _compound.Rotation += _rotationVelocity;
            }

            if (IsAccelerating())
            {
                newAcceleration = _acceleration;
            }

            if (IsDecelerating())
            {
                newAcceleration = -_acceleration;
            }

            _compound.LinearVelocity += _direction * newAcceleration;

            // Kill orthogonal velocity
            Vector2 forwardVelocity = _direction * Vector2.Dot(_compound.LinearVelocity, _direction);
            _compound.LinearVelocity = forwardVelocity;

            if (_compound.LinearVelocity.Length() > _maximumVelocity)
            {
                _compound.LinearVelocity = Vector2.Normalize(_compound.LinearVelocity) * _maximumVelocity;
            }
        }

        public void HandleInput(GameTime gameTime, PlayerIndex controllingPlayer, InputState input)
        {
            if (_isReady) return;

            PlayerIndex playerIndex;
            if (_readyAction.Evaluate(input, controllingPlayer, out playerIndex))
            {
                _isReady = true;
                _compound.LinearVelocity = Vector2.Zero;
                if (OnReadyToPlay != null)
                {
                    OnReadyToPlay(this, new ReadyToPlayEventArgs((int)playerIndex));
                }
            }
        }

        #region Driving Controls

        private bool GoesLeft()
        {
            return (_keyboard.IsKeyDown(Keys.Left) || _gamePad.ThumbSticks.Right.X < 0);
        }

        private bool GoesRight()
        {
            return (_keyboard.IsKeyDown(Keys.Right) || _gamePad.ThumbSticks.Right.X > 0);
        }

        private bool IsAccelerating()
        {
            return (_keyboard.IsKeyDown(Keys.Up) || _gamePad.ThumbSticks.Left.Y > 0);
        }

        private bool IsDecelerating()
        {
            return (_keyboard.IsKeyDown(Keys.Down) || _gamePad.ThumbSticks.Left.Y < 0);
        }

        private bool DidSetReady()
        {
            return (_keyboard.IsKeyDown(Keys.A) || _gamePad.IsButtonDown(Buttons.A));
        }

        #endregion
    }

    public class ReadyToPlayEventArgs : EventArgs
    {
        public int CarIndex { get; private set; }

        public ReadyToPlayEventArgs(int carIndex) { CarIndex = carIndex; }
    }
}
