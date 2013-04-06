﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.SamplesFramework;
using Microsoft.Xna.Framework.Content;
using WindowsGame2.GameElements;

namespace WindowsGame2
{

    public class Car : TexturePhysicsObject
    {
        private static int mMinimumCarDistance = 50;
        private static float mIntersectionDistance = 30.0f;
        private static string vicksMode = "vicks";
        private static string richMode = "rich";
        private static string unityMode = "unity";
        private static string microMode = "micro";
        private static string physicMode = "physic";
        private static string drivingMode = unityMode;

        private static int mMaximumTrailPoints = 130;
        public int mTrailPoints;

        private Vector2 mForceVector;
        private Vector2 mDirection;
        private float mForce;

        private bool mShowTrail;
        public bool mIsTrailLoop;
        private bool wasDrawing;
        private bool mFoundIntersection;
        public bool justStarted;

        private Vertices mTrailVertices;
        private Vector2[] mTrailPositions;

        private Texture2D mDummyTexture;
        private Color mColor;

        private VertexPositionColorTexture[] trailVertices = new VertexPositionColorTexture[mMaximumTrailPoints * 6];
        private Vector3 tdPos = new Vector3(0, 0, -0.1f);
        private Vector3 oldWVert, newWVert, oldEVert, newEVert;
        private Vector2 texNW, texNE, texOW, texOE;
        private float offset = 0.3f, tailOffset = 0.2f;
        private Random seed = new Random();

        private float acc = 0.4f;
        private float rotVel = 0.12f; //0.12
        private float maxVel = 10; //5
        private float linearVel = 0;

        private int mIndex;

        private int boostFrames;
        private bool hasBoost;

        private RandomTrack randomTrack;
        public int currentMiddlePoint;
        public Vector2 projectedPosition;

        public bool isActive;

        public Car(World world, Color Color, RandomTrack _randomTrack)
            : base(world, GameServices.GetService<ContentManager>().Load<Texture2D>("Images/small_white_penis"), new Vector2(65.0f, 40.0f), Color)
        {

            isActive = true;

            this.randomTrack = _randomTrack;
            projectedPosition = new Vector2();

            mForceVector = new Vector2();
            mDirection = new Vector2();
            mForce = 1.0f;

            mTrailPoints = 0;
            mTrailVertices = new Vertices();
            mTrailPositions = new Vector2[mMaximumTrailPoints];

            mDummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            mDummyTexture.SetData(new Color[] { Color.White });
            mColor = Color;

            _compound.LinearDamping = 1;
            _compound.AngularDamping = 1;

            hasBoost = false;
            boostFrames = 0;

            for (int i = 0; i < trailVertices.Count(); i++) trailVertices[i].Color = mColor;

            newWVert = new Vector3(-0.1f);
            newEVert = new Vector3(-0.1f);
            oldWVert = new Vector3(-0.1f);
            oldEVert = new Vector3(-0.1f);

            texNW = new Vector2(0, 0);
            texNE = new Vector2(1, 0);
            texOW = new Vector2(0, 1);
            texOE = new Vector2(1, 1);
        }

        public Vector2 ProjectedPosition
        {
            get { return ConvertUnits.ToDisplayUnits(projectedPosition); }
            set { projectedPosition = value * ConvertUnits.ToSimUnits(1); }
        }


        private int mod(int index)
        {
            return (index + mMaximumTrailPoints) % mMaximumTrailPoints;
        }

        private void resetTrail()
        {
            for (int i = 0; i < trailVertices.Count(); i++)
            {
                trailVertices[i].Position = tdPos;
            }
        }

        public void Update(GamePadState gps, KeyboardState ks)
        {
            if (boostFrames == 50)
            {
                boostFrames = 0;
                hasBoost = false;
                maxVel = 10;
                acc = 0.4f;
            }
            if (hasBoost)
            {
                boostFrames++;
            }

            
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

            tdPos.X = _compound.Position.X;
            tdPos.Y = _compound.Position.Y;

            // Add a trail point if the player is drawing
            if ((ks.IsKeyDown(Keys.F) && mColor == Color.Blue || gps.Triggers.Right > 0) && !justStarted)
            {
                mShowTrail = true;

                if (mTrailPoints >= mMaximumTrailPoints)
                {
                    mIsTrailLoop = true;
                    mTrailPoints = 0;
                }
                mTrailPositions[mTrailPoints] = Position - mDirection * tailOffset;

                newWVert.X = tdPos.X - mDirection.Y * offset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f;
                newWVert.Y = tdPos.Y + mDirection.X * offset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f;

                newEVert.X = tdPos.X + mDirection.Y * offset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f;
                newEVert.Y = tdPos.Y - mDirection.X * offset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f;

                trailVertices[mTrailPoints * 6 + 0].Position = newWVert;
                trailVertices[mTrailPoints * 6 + 1].Position = newEVert;
                trailVertices[mTrailPoints * 6 + 2].Position = oldWVert;
                trailVertices[mTrailPoints * 6 + 3].Position = oldWVert;
                trailVertices[mTrailPoints * 6 + 4].Position = oldEVert;
                trailVertices[mTrailPoints * 6 + 5].Position = newEVert;

                trailVertices[mTrailPoints * 6 + 0].TextureCoordinate = texNW;
                trailVertices[mTrailPoints * 6 + 1].TextureCoordinate = texNE;
                trailVertices[mTrailPoints * 6 + 2].TextureCoordinate = texOW;
                trailVertices[mTrailPoints * 6 + 3].TextureCoordinate = texOW;
                trailVertices[mTrailPoints * 6 + 4].TextureCoordinate = texOE;
                trailVertices[mTrailPoints * 6 + 5].TextureCoordinate = texNE;

                oldWVert = newWVert;
                oldEVert = newEVert;

                mTrailPoints++;
            }
            else
            {
                mShowTrail = false;
                mFoundIntersection = false;
                mIsTrailLoop = false;
                mTrailPoints = 0;

                oldWVert.X = tdPos.X - mDirection.Y * offset;
                oldWVert.Y = tdPos.Y + mDirection.X * offset;

                oldEVert.X = tdPos.X + mDirection.Y * offset;
                oldEVert.Y = tdPos.Y - mDirection.X * offset;

                justStarted = false;
                resetTrail();
            }

            projectedPosition = computeMiddleTrackProjection();
        }

        Vector2 computeMiddleTrackProjection()
        {
            int nextMiddlePoint = currentMiddlePoint + 1;
            if (nextMiddlePoint >= randomTrack.curvePointsMiddle.Count)
            {
              //  nextMiddlePoint = 0;
            }

            float distBack = Vector2.Distance(this._compound.Position, randomTrack.curvePointsInternal[randomTrack.internalCorrispondances[currentMiddlePoint % randomTrack.curvePointsMiddle.Count]]) + Vector2.Distance(this._compound.Position, randomTrack.curvePointsExternal[currentMiddlePoint % randomTrack.curvePointsMiddle.Count]);
            float distFront = Vector2.Distance(this._compound.Position, randomTrack.curvePointsInternal[randomTrack.internalCorrispondances[nextMiddlePoint % randomTrack.curvePointsMiddle.Count]]) + Vector2.Distance(this._compound.Position, randomTrack.curvePointsExternal[nextMiddlePoint % randomTrack.curvePointsMiddle.Count]);
            float margin = 0.2f;

            float totalDist = (distBack - randomTrack.pathWidth) + (distFront - randomTrack.pathWidth);
            float ratio = (distBack - randomTrack.pathWidth) / totalDist;
            Vector2 _projectedPosition = randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count] + (randomTrack.curvePointsMiddle[nextMiddlePoint % randomTrack.curvePointsMiddle.Count] - randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count]) * ratio;

            if (distFront < randomTrack.pathWidth + margin)
            {
                currentMiddlePoint = nextMiddlePoint;
            }

            return _projectedPosition;
        }

        public void Draw(SpriteBatch spriteBatch, out VertexPositionColorTexture[] vertices)
        {
            base.Draw(spriteBatch);
            //if (mShowTrail)
            //{
            //    for (int i = 1; i < mTrailPoints; i++)
            //    {
            //        DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[i], mTrailPositions[i - 1]);
            //    }
            //    if (mIsTrailLoop)
            //    {
            //        for (int i = mTrailPoints + 1; i < mMaximumTrailPoints; i++)
            //        {
            //            DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[i], mTrailPositions[i - 1]);
            //        }
            //        if (mTrailPoints != mMaximumTrailPoints)
            //        {
            //            DrawLine(spriteBatch, mDummyTexture, 5, mTrailPositions[0], mTrailPositions[mMaximumTrailPoints - 1]);
            //        }
            //    }
            //}

            vertices = trailVertices;

            base.Draw(spriteBatch);


        }

        private void DrawLine(SpriteBatch batch, Texture2D blank, float width, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            batch.Draw(blank, point1, null, mColor, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }



         public bool TrailObstacle(World World)
        {
            
            if (mTrailPoints > mMinimumCarDistance || (mIsTrailLoop && mMaximumTrailPoints > mMinimumCarDistance))
            {
                float difference = mTrailPoints - mMinimumCarDistance;
                if (difference > 0)
                {
                    difference = 0;
                }
                for (int i = 0; i < mTrailPoints - 1 - mMinimumCarDistance; i++)
                {
                    if (Vector2.Distance(Position, mTrailPositions[i]) < mIntersectionDistance)
                    {
                        mIndex=i;
                        //already one shape with this trail, do not draw other shapes
                        return true;
                    }
                   
                }
                if (mIsTrailLoop)
                {
                    for (int i = mTrailPoints; i < mMaximumTrailPoints - 1 + difference; i++)
                    {
                        if (Vector2.Distance(Position, mTrailPositions[i]) < mIntersectionDistance)
                    {
                            mIndex=i;
                            //already one shape with this trail, do not draw other shapes
                            return true;
                        }
                        
                    }
                }

            }
            return false;
        }

        // Helper Method
        // Return a new polygon in the given world if the trail self-intersects, null otherwise.
        public PolygonPhysicsObject TrailIntersection(World World)
        {
            
                // intersection found
                mFoundIntersection = true;
       

                // compute polygon vertices
                mTrailVertices.Clear();

                int verticesInterval =5;

                if (mIsTrailLoop)
                {
                    if (mIndex < mTrailPoints)
                    {
                        for (int ii = mIndex; ii < mTrailPoints - 1; ii++)
                        {
                            if (ii % verticesInterval == 0)
                            {
                                mTrailVertices.Add(mTrailPositions[ii]);
                            }
                        }
                    }
                    else
                    {
                        for (int ii = mIndex; ii < mMaximumTrailPoints - 1; ii++)
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
                }
                else
                {
                    for (int ii = mIndex; ii < mTrailPoints; ii++)
                    {
                        if (ii % verticesInterval == 0)
                        {
                            mTrailVertices.Add(mTrailPositions[ii]);
                        }
                    }
                }

                mTrailPoints = 0;
                mIsTrailLoop = false;
                mShowTrail = false;

                //if the shape is a polygon, create a new object
                if (mTrailVertices.Count > 2)
                {
                    PolygonPhysicsObject result = new PolygonPhysicsObject(World, mTrailVertices);
                    if (result.IsValid)
                    {
                        result.Color = mColor;
                        result.compound.IgnoreCollisionWith(_compound);

                       
                        maxVel = 10 + result.compound.Mass;
                        acc = acc + result.compound.Mass / 10;
                        hasBoost = true;
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else{
                    return null;
                }
            
            
        }
    }
}



