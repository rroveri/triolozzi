using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;

using FarseerPhysics.Dynamics;
//using FarseerPhysics.Factories;
//using FarseerPhysics.Common;
//using FarseerPhysics.Controllers;
//using FarseerPhysics.Collision.Shapes;
//using FarseerPhysics.Common.Decomposition;
//using FarseerPhysics.Common.PolygonManipulation;
//using FarseerPhysics.SamplesFramework;

namespace WindowsGame2
{

    class Car : TexturePhysicsObject
    {
        private Vector2 mForceVector;
        private Vector2 mDirection;
        private float mForce;

        private bool mShowTrail;
        private bool mIsTrailLoop;
        private bool mFoundIntersection;

        public Car(World world, Game Game, Color Color)
            : base(world, Game.Content.Load<Texture2D>("Images/penis"), new Vector2(65.0f, 40.0f), Color)
        {
            mForceVector = new Vector2();
            mDirection = new Vector2();
            mForce = 1.0f;
        }

        public void Update(GamePadState gps, KeyboardState ks)
        {
            if (ks.IsKeyDown(Keys.Right) || gps.ThumbSticks.Right.X > 0)
            {
                _compound.ApplyTorque(0.1f);
            }
            if (ks.IsKeyDown(Keys.Left) || gps.ThumbSticks.Right.X < 0)
            {
                _compound.ApplyTorque(-0.1f);
            }


            mDirection.X = (float)Math.Cos(_compound.Rotation);
            mDirection.Y = (float)Math.Sin(_compound.Rotation);

            mForceVector = mDirection * mForce;

            if (ks.IsKeyDown(Keys.Up) || gps.ThumbSticks.Left.Y > 0)
            {
                _compound.ApplyForce(mForceVector, _compound.WorldCenter);

            }
            if (ks.IsKeyDown(Keys.Down) || gps.ThumbSticks.Left.Y < 0)
            {
                _compound.ApplyForce(-mForceVector, _compound.WorldCenter);
            }
        }
    }
}
