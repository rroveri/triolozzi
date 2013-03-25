using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.DebugViews;

namespace WindowsGame2
{

    public class Car : TexturePhysicsObject
    {
        private static int mMinimumCarDistance = 50;
        private static float mIntersectionDistance = 5.0f;
        private static string vicksMode = "vicks";
        private static string richMode = "rich";
        private static string unityMode = "unity";
        private static string microMode = "micro";
        private static string physicMode = "physic";
        private static string drivingMode = unityMode;

        private static int mMaximumTrailPoints = 500;
        private int mTrailPoints;

        private Vector2 mForceVector;
        private Vector2 mDirection;
        private float mForce;

        private bool mShowTrail;
        private bool mIsTrailLoop;
        private bool mFoundIntersection;

        private Vertices mTrailVertices;
        private Vector2[] mTrailPositions;

        private Texture2D mDummyTexture;
        private Color mColor;

        private float acc = 0.4f;
        private float rotVel = 0.07f;
        private float maxVel = 8;
        private float linearVel = 0;

     

        public Car(World world, Game Game, Color Color)
            : base(world, Game.Content.Load<Texture2D>("Images/penis"), new Vector2(65.0f, 40.0f), Color)
        {
            mForceVector = new Vector2();
            mDirection = new Vector2();
            mForce = 1.0f;

            mTrailPoints = 0;
            mTrailVertices = new Vertices();
            mTrailPositions = new Vector2[mMaximumTrailPoints];

            mDummyTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            mDummyTexture.SetData(new Color[] { Color.White });
            mColor = Color;

            _compound.LinearDamping = 1;
            _compound.AngularDamping = 1;
        }

        public void Update(GamePadState gps, KeyboardState ks)
        {

            mDirection.X = (float)Math.Cos(_compound.Rotation);
            mDirection.Y = (float)Math.Sin(_compound.Rotation);

            mForceVector = mDirection * mForce;

            linearVel = _compound.LinearVelocity.Length();
            if(linearVel > maxVel) linearVel = maxVel;

            if (drivingMode == vicksMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && mColor == Color.Blue || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation += rotVel;
                }
                if (ks.IsKeyDown(Keys.Left) && mColor == Color.Blue || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation -= rotVel;
                }

                if (ks.IsKeyDown(Keys.Up) && mColor == Color.Blue || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.LinearVelocity += mDirection * (linearVel);

                }
                if (ks.IsKeyDown(Keys.Down) && mColor == Color.Blue || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.LinearVelocity += -mDirection * (linearVel);
                }
            }
            else if (drivingMode == richMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && mColor == Color.Blue || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.ApplyTorque(0.1f);
                }
                if (ks.IsKeyDown(Keys.Left) && mColor == Color.Blue || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.ApplyTorque(-0.1f);
                }

                if (ks.IsKeyDown(Keys.Up) && mColor == Color.Blue || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.ApplyForce(mForceVector, _compound.WorldCenter);

                }
                if (ks.IsKeyDown(Keys.Down) && mColor == Color.Blue || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.ApplyForce(-mForceVector, _compound.WorldCenter);
                }
            }
            else if (drivingMode == unityMode)
            {
                if (ks.IsKeyDown(Keys.Right) && mColor == Color.Blue || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation += rotVel;
                }
                if (ks.IsKeyDown(Keys.Left) && mColor == Color.Blue || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation -= rotVel;
                }

                if (ks.IsKeyDown(Keys.Up) && mColor == Color.Blue || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.LinearVelocity = mDirection * (linearVel);
                    _compound.LinearVelocity += mDirection * (acc);
                }
                if (ks.IsKeyDown(Keys.Down) && mColor == Color.Blue || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.LinearVelocity = -mDirection * (linearVel);
                    _compound.LinearVelocity += -mDirection * (acc);
                }
            }
            else if (drivingMode == microMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && mColor == Color.Blue || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.ApplyTorque(0.1f);
                }
                if (ks.IsKeyDown(Keys.Left) && mColor == Color.Blue || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.ApplyTorque(-0.1f);
                }

                if (ks.IsKeyDown(Keys.Up) && mColor == Color.Blue || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.ApplyForce(mForceVector, _compound.WorldCenter);

                }
                if (ks.IsKeyDown(Keys.Down) && mColor == Color.Blue || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.ApplyForce(-mForceVector, _compound.WorldCenter);
                }
            }
            
            // Add a trail point if the player is drawing
            if (ks.IsKeyDown(Keys.F) && mColor == Color.Blue || gps.Triggers.Right > 0)
            {
                mShowTrail = true;

                if (mTrailPoints >= mMaximumTrailPoints)
                {
                    mIsTrailLoop = true;
                    mTrailPoints = 0;
                }
                mTrailPositions[mTrailPoints] = Position;
                mTrailPoints++;
            }
            else
            {
                mShowTrail = false;
                mFoundIntersection = false;
                mIsTrailLoop = false;
                mTrailPoints = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            if (mShowTrail)
            {
                for (int i = 1; i < mTrailPoints; i++)
                {
                    DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[i], mTrailPositions[i - 1]);
                }
                if (mIsTrailLoop)
                {
                    for (int i = mTrailPoints + 1; i < mMaximumTrailPoints; i++)
                    {
                        DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[i], mTrailPositions[i - 1]);
                    }
                    if (mTrailPoints != mMaximumTrailPoints)
                    {
                        DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[0], mTrailPositions[mMaximumTrailPoints - 1]);
                    }
                }
            }

            base.Draw(spriteBatch);


        }

        private void DrawLine(SpriteBatch batch, Texture2D blank, float width, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            batch.Draw(blank, point1, null, mColor, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        public PolygonPhysicsObject TrailObstacle(World World, AssetCreator AssetCreator)
        {
            PolygonPhysicsObject result = null;
            if (mTrailPoints > mMinimumCarDistance || (mIsTrailLoop && mMaximumTrailPoints > mMinimumCarDistance))
            {
                float difference = mTrailPoints - mMinimumCarDistance;
                if (difference > 0)
                {
                    difference = 0;
                }
                for (int i = 0; i < mTrailPoints - 1 - mMinimumCarDistance; i++)
                {
                    if (mFoundIntersection)
                    {
                        //already one shape with this trail, do not draw other shapes
                        break;
                    }
                    result = TrailIntersection(World, i, AssetCreator);
                }
                if (mIsTrailLoop)
                {
                    for (int i = mTrailPoints; i < mMaximumTrailPoints - 1 + difference; i++)
                    {
                        if (mFoundIntersection)
                        {
                            //already one shape with this trail, do not draw other shapes
                            break;
                        }
                        result = TrailIntersection(World, i, AssetCreator);
                    }
                }

            }
            if (result != null)
            {
                result.Color = mColor;
                result.compound.IgnoreCollisionWith(_compound);
            }
            return result;
        }

        // Helper Method
        // Return a new polygon in the given world if the trail self-intersects, null otherwise.
        private PolygonPhysicsObject TrailIntersection(World World, int Index, AssetCreator AssetCreator)
        {
            if (Vector2.Distance(Position, mTrailPositions[Index]) < mIntersectionDistance)
            {
                // intersection found
                mFoundIntersection = true;

                // compute polygon vertices
                mTrailVertices.Clear();

                int verticesInterval = 10;

                if (mIsTrailLoop)
                {
                    for (int ii = Index; ii < mMaximumTrailPoints - 1; ii++)
                    {
                        if (ii % verticesInterval == 0)
                        {
                            mTrailVertices.Add(mTrailPositions[ii]);
                        }
                    }
                    for (int ii = 0; ii < mTrailPoints; ii++)
                    {
                        if (ii % verticesInterval == 0)
                        {
                            mTrailVertices.Add(mTrailPositions[ii]);
                        }
                    }
                }
                else
                {
                    for (int ii = Index; ii < mTrailPoints; ii++)
                    {
                        if (ii % verticesInterval == 0)
                        {
                            mTrailVertices.Add(mTrailPositions[ii]);
                        }
                    }
                }

                //if the shape is a polygon, create a new object
                if (mTrailVertices.Count > 2)
                {
                    return new PolygonPhysicsObject(World, mTrailVertices, mDummyTexture, AssetCreator);
                }
            }
            return null;
        }
    }
}
