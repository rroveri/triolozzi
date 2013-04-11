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
using Microsoft.Xna.Framework.Content;

namespace WindowsGame2.GameElements
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

        public Vector2 mForceVector;
        public Vector2 mDirection;
        private float mForce;

        public bool mIsTrailLoop;
        public bool justStarted;

        public int score;

        private Vertices mTrailVertices;
        private Vector2[] mTrailPositions;


        private Texture2D mDummyTexture;
        private Color mColor;

        private VertexPositionColorTexture[] trailVertices = new VertexPositionColorTexture[mMaximumTrailPoints * 6];
        private VertexPositionColorTexture[] burnoutsVertices = new VertexPositionColorTexture[100000];
        private Vector3 tdPos = new Vector3(0, 0, -0.1f);
        private Vector3 oldWVert, newWVert, oldEVert, newEVert;
        private Vector3 oldWVertBurnoutRight, newWVertBurnoutRight, oldEVertBurnoutRight, newEVertBurnoutRight;
        private Vector3 oldWVertBurnoutLeft, newWVertBurnoutLeft, oldEVertBurnoutLeft, newEVertBurnoutLeft;
        private Vector2 texNW, texNE, texOW, texOE;
        private float offset = 0.3f, tailOffset = 0.2f, burnoutOffset = 0.1f;
        private Random seed = new Random( DateTime.Now.Millisecond);

        private float acc = 0.4f;
        private float rotVel = 0.12f; //0.12
        private float maxVel = 11.5f; //5
        private float linearVel = 0;
        private float boostAcc = 10f;
        private float boostMaxVel = 21f;
        private float currentAcc;
        private float currentMaxVel;

        private int mIndex;

        private int boostFrames;
        private bool hasBoost;

        private RandomTrack randomTrack;
        public int currentMiddlePoint;
        public Vector2 projectedPosition;

        public bool isActive;

        private float driftValue;

        private int maxBoostFrames;

        public int burnoutCounter;

        private float wheelsDistance=0.13f;

        private bool freeToSwap;

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

            for (int i = 0; i < burnoutsVertices.Count(); i++) burnoutsVertices[i].Color = Color.Black;

            newWVert = new Vector3(-0.1f);
            newEVert = new Vector3(-0.1f);
            oldWVert = new Vector3(-0.1f);
            oldEVert = new Vector3(-0.1f);

            texNW = new Vector2(0, 0);
            texNE = new Vector2(1, 0);
            texOW = new Vector2(0, 1);
            texOE = new Vector2(1, 1);

            newWVertBurnoutRight = new Vector3(-0.1f);
            newEVertBurnoutRight = new Vector3(-0.1f);
            oldWVertBurnoutRight = new Vector3(-0.1f);
            oldEVertBurnoutRight = new Vector3(-0.1f);

            newWVertBurnoutLeft = new Vector3(-0.1f);
            newEVertBurnoutLeft = new Vector3(-0.1f);
            oldWVertBurnoutLeft = new Vector3(-0.1f);
            oldEVertBurnoutLeft = new Vector3(-0.1f);

            score = 27;

            driftValue = 0;
            maxBoostFrames = -1;

            currentAcc = acc;
            currentMaxVel = maxVel;

            burnoutCounter = 0;

            freeToSwap = true;
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

        public void resetBoost()
        {
            boostFrames = 0;
            hasBoost = false;
            currentMaxVel = maxVel;
            currentAcc = acc;

            driftValue = 0;
        }

        public void Update(GamePadState gps, KeyboardState ks)
        {
            if (isActive == false)
            {
                return;
            }

            if (boostFrames == maxBoostFrames)
            {
                resetBoost();
            }
            if (hasBoost)
            {
                driftValue = 1;
                boostFrames++;
            }

            
            mDirection.X = (float)Math.Cos(_compound.Rotation);
            mDirection.Y = (float)Math.Sin(_compound.Rotation);

            mForceVector = mDirection * mForce;

           // linearVel = _compound.LinearVelocity.Length();
          //  if(linearVel > maxVel) linearVel = maxVel;

            bool blueOnly = mColor == Color.Blue || true;

            if (drivingMode == vicksMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && blueOnly || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation += rotVel;
                }
                if (ks.IsKeyDown(Keys.Left) && blueOnly || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation -= rotVel;
                }

                if (ks.IsKeyDown(Keys.Up) && blueOnly || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.LinearVelocity += mDirection * (linearVel);

                }
                if (ks.IsKeyDown(Keys.Down) && blueOnly || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.LinearVelocity += -mDirection * (linearVel);
                }
            }
            else if (drivingMode == richMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && blueOnly || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.ApplyTorque(0.05f);
                }
                if (ks.IsKeyDown(Keys.Left) && blueOnly || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.ApplyTorque(-0.05f);
                   
                }

                if (ks.IsKeyDown(Keys.Up) && blueOnly || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.ApplyForce(mForceVector, _compound.WorldCenter);

                }
                if (ks.IsKeyDown(Keys.Down) && blueOnly || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.ApplyForce(-mForceVector, _compound.WorldCenter);
                }

                KillOrthogonalVelocity(this, 0.1f);
            }
            else if (drivingMode == unityMode)
            {
                float newAcc = 0.0f;
                if (ks.IsKeyDown(Keys.Right) && blueOnly || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation += rotVel;
                }
                if (ks.IsKeyDown(Keys.Left) && blueOnly || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.AngularVelocity = 0;
                    _compound.Rotation -= rotVel;
                }

                if (ks.IsKeyDown(Keys.Up) && blueOnly || gps.ThumbSticks.Left.Y > 0)
                {
                    newAcc = currentAcc;
                }
                if (ks.IsKeyDown(Keys.Down) && blueOnly || gps.ThumbSticks.Left.Y < 0)
                {
                    newAcc = -currentAcc;
                }

                _compound.LinearVelocity += mDirection * (newAcc);
                KillOrthogonalVelocity(this, driftValue);
                if (_compound.LinearVelocity.Length() > currentMaxVel)
                {
                    Vector2 tempVel=_compound.LinearVelocity;
                    _compound.LinearVelocity = Vector2.Normalize(tempVel) * currentMaxVel;
                }
                

            }
            else if (drivingMode == microMode)
            {
                // Move the car
                if (ks.IsKeyDown(Keys.Right) && blueOnly || gps.ThumbSticks.Right.X > 0)
                {
                    _compound.ApplyTorque(0.1f);
                }
                if (ks.IsKeyDown(Keys.Left) && blueOnly || gps.ThumbSticks.Right.X < 0)
                {
                    _compound.ApplyTorque(-0.1f);
                }

                if (ks.IsKeyDown(Keys.Up) && blueOnly || gps.ThumbSticks.Left.Y > 0)
                {
                    _compound.ApplyForce(mForceVector, _compound.WorldCenter);

                }
                if (ks.IsKeyDown(Keys.Down) && blueOnly || gps.ThumbSticks.Left.Y < 0)
                {
                    _compound.ApplyForce(-mForceVector, _compound.WorldCenter);
                }
            }

            tdPos.X = _compound.Position.X;
            tdPos.Y = _compound.Position.Y;

            // Add a trail point if the player is drawing
            if (((ks.IsKeyDown(Keys.F) && blueOnly || gps.Triggers.Right > 0) || true) && !justStarted)
            {
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
        
            //easter egg
            if (gps.Triggers.Left > 0 && gps.Buttons.Y== ButtonState.Pressed && freeToSwap)
            {
                randomTrack.swapTexture();
                freeToSwap = false;
            }
            else if (gps.Buttons.Y == ButtonState.Released)
            {
                freeToSwap = true;
            }
        
        }

        public void KillOrthogonalVelocity(Car car, float drift)
        {
            Vector2 forwardVelocity = mDirection * Vector2.Dot(car._compound.LinearVelocity, mDirection);
            Vector2 rightVector = new Vector2(-mDirection.Y, mDirection.X);
            Vector2 rightVelocity = rightVector * Vector2.Dot(car._compound.LinearVelocity,rightVector);
            car._compound.LinearVelocity = forwardVelocity + rightVelocity * drift;

            if (drift != 0 && rightVelocity.Length() > 5f)
            {
                newWVertBurnoutRight.X = tdPos.X - mDirection.Y * burnoutOffset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.X * wheelsDistance;
                newWVertBurnoutRight.Y = tdPos.Y + mDirection.X * burnoutOffset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.Y * wheelsDistance;

                newEVertBurnoutRight.X = tdPos.X + mDirection.Y * burnoutOffset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.X * wheelsDistance;
                newEVertBurnoutRight.Y = tdPos.Y - mDirection.X * burnoutOffset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.Y * wheelsDistance;

                burnoutsVertices[burnoutCounter * 6 + 0].Position = newWVertBurnoutRight;
                burnoutsVertices[burnoutCounter * 6 + 1].Position = newEVertBurnoutRight;
                burnoutsVertices[burnoutCounter * 6 + 2].Position = oldWVertBurnoutRight;
                burnoutsVertices[burnoutCounter * 6 + 3].Position = oldWVertBurnoutRight;
                burnoutsVertices[burnoutCounter * 6 + 4].Position = oldEVertBurnoutRight;
                burnoutsVertices[burnoutCounter * 6 + 5].Position = newEVertBurnoutRight;

                burnoutsVertices[burnoutCounter * 6 + 0].TextureCoordinate = texNW;
                burnoutsVertices[burnoutCounter * 6 + 1].TextureCoordinate = texNE;
                burnoutsVertices[burnoutCounter * 6 + 2].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 3].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 4].TextureCoordinate = texOE;
                burnoutsVertices[burnoutCounter * 6 + 5].TextureCoordinate = texNE;

                oldWVertBurnoutRight = newWVertBurnoutRight;
                oldEVertBurnoutRight = newEVertBurnoutRight;

                burnoutCounter++;


                newWVertBurnoutLeft.X = tdPos.X - mDirection.Y * burnoutOffset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.X * wheelsDistance;
                newWVertBurnoutLeft.Y = tdPos.Y + mDirection.X * burnoutOffset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.Y * wheelsDistance;

                newEVertBurnoutLeft.X = tdPos.X + mDirection.Y * burnoutOffset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.X * wheelsDistance;
                newEVertBurnoutLeft.Y = tdPos.Y - mDirection.X * burnoutOffset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.Y * wheelsDistance;

                burnoutsVertices[burnoutCounter * 6 + 0].Position = newWVertBurnoutLeft;
                burnoutsVertices[burnoutCounter * 6 + 1].Position = newEVertBurnoutLeft;
                burnoutsVertices[burnoutCounter * 6 + 2].Position = oldWVertBurnoutLeft;
                burnoutsVertices[burnoutCounter * 6 + 3].Position = oldWVertBurnoutLeft;
                burnoutsVertices[burnoutCounter * 6 + 4].Position = oldEVertBurnoutLeft;
                burnoutsVertices[burnoutCounter * 6 + 5].Position = newEVertBurnoutLeft;

                burnoutsVertices[burnoutCounter * 6 + 0].TextureCoordinate = texNW;
                burnoutsVertices[burnoutCounter * 6 + 1].TextureCoordinate = texNE;
                burnoutsVertices[burnoutCounter * 6 + 2].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 3].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 4].TextureCoordinate = texOE;
                burnoutsVertices[burnoutCounter * 6 + 5].TextureCoordinate = texNE;

                oldWVertBurnoutLeft = newWVertBurnoutLeft;
                oldEVertBurnoutLeft = newEVertBurnoutLeft;

                burnoutCounter++;

            }
            else
            {
                oldWVertBurnoutRight.X = tdPos.X - mDirection.Y * burnoutOffset + rightVector.X * wheelsDistance;
                oldWVertBurnoutRight.Y = tdPos.Y + mDirection.X * burnoutOffset + rightVector.Y * wheelsDistance;

                oldEVertBurnoutRight.X = tdPos.X + mDirection.Y * burnoutOffset + rightVector.X * wheelsDistance;
                oldEVertBurnoutRight.Y = tdPos.Y - mDirection.X * burnoutOffset + rightVector.Y * wheelsDistance;

                oldWVertBurnoutLeft.X = tdPos.X - mDirection.Y * burnoutOffset - rightVector.X * wheelsDistance;
                oldWVertBurnoutLeft.Y = tdPos.Y + mDirection.X * burnoutOffset - rightVector.Y * wheelsDistance;

                oldEVertBurnoutLeft.X = tdPos.X + mDirection.Y * burnoutOffset - rightVector.X * wheelsDistance;
                oldEVertBurnoutLeft.Y = tdPos.Y - mDirection.X * burnoutOffset - rightVector.Y * wheelsDistance;
            }
            
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
            float margin = 15f; //0.2 for step=3

            float totalDist = (distBack - randomTrack.pathWidth) + (distFront - randomTrack.pathWidth);
            float ratio = (distBack - randomTrack.pathWidth) / totalDist;
            Vector2 _projectedPosition = randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count] + (randomTrack.curvePointsMiddle[nextMiddlePoint % randomTrack.curvePointsMiddle.Count] - randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count]) * ratio;

            if (distFront < randomTrack.pathWidth + margin)
            {
                currentMiddlePoint = nextMiddlePoint;
            }

            return _projectedPosition;
        }

        public void Draw(SpriteBatch spriteBatch, out VertexPositionColorTexture[] vertices, out VertexPositionColorTexture[] _burnoutsVertices)
        {
            vertices = trailVertices;
            _burnoutsVertices = burnoutsVertices;

            spriteBatch.Draw(mDummyTexture,ConvertUnits.ToDisplayUnits( projectedPosition),
                                           null, mColor, 0, Vector2.Zero, Vector2.One*10, SpriteEffects.None,
                                           0.9f);

            base.Draw(spriteBatch);
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

            //if the shape is a polygon, create a new object
            if (mTrailVertices.Count > 2)
            {
                PolygonPhysicsObject result = new PolygonPhysicsObject(World, mTrailVertices);
                if (result.IsValid)
                {
                    result.Color = mColor;
                    result.compound.IgnoreCollisionWith(_compound);

                    maxBoostFrames =(int)Math.Floor(result.compound.Mass * 4);
                  //  maxVel = 10 + result.compound.Mass;
                    currentMaxVel = boostMaxVel;
                 //   acc = acc + result.compound.Mass / 10;
                    currentAcc = boostAcc;
                    hasBoost = true;
                    return result;
                }
                else
                {
                    return null;
                }
                    
            }
            else
            {
                return null;
            }
        }
    }
}



