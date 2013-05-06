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
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework.Audio;

namespace WindowsGame2.GameElements
{

    public class Car : TexturePhysicsObject
    {
        private static int mMinimumCarDistance = 50;
        private static float mIntersectionDistance = 30.0f;

        public static int mMaximumTrailPoints = 130;
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
        public Color mColor;

        private VertexPositionColorTexture[] trailVertices = new VertexPositionColorTexture[mMaximumTrailPoints * 6 * paintersCount];
        private VertexPositionColorTexture[] burnoutsVertices = new VertexPositionColorTexture[500*6*4];
        private Vector3 tdPos = new Vector3(0, 0, -0.1f);
        private Vector3 oldWVert, newWVert, oldEVert, newEVert;
        private Vector3 oldWVertBurnoutRight, newWVertBurnoutRight, oldEVertBurnoutRight, newEVertBurnoutRight;
        private Vector3 oldWVertBurnoutLeft, newWVertBurnoutLeft, oldEVertBurnoutLeft, newEVertBurnoutLeft;
        private Vector3 oldWVertBurnoutRightFront, newWVertBurnoutRightFront, oldEVertBurnoutRightFront, newEVertBurnoutRightFront;
        private Vector3 oldWVertBurnoutLeftFront, newWVertBurnoutLeftFront, oldEVertBurnoutLeftFront, newEVertBurnoutLeftFront;
        private Vector2 texNW, texNE, texOW, texOE;
        public float offset = 0.3f, tailOffset = 0.2f, burnoutOffset = 0.1f, brushOffset=0.05f;
        private Random seed = new Random( DateTime.Now.Millisecond);

        private float acc = 0.4f;
        private float rotVel = 0.12f; //0.12
        private float maxVel = 11.5f; //5
        //private float linearVel = 0;
        private float boostAcc = 4f;
        private float boostMaxVel = 21f;
        private float currentAcc;
        private float currentMaxVel;

        private int mIndex;

        private int boostFrames;
        public bool hasBoost;

        private RandomTrack randomTrack;
        public int currentMiddlePoint;
        public Vector2 projectedPosition;

        public bool isActive;

        private float driftValue;

        private int maxBoostFrames;

        public int burnoutCounter;

        private float wheelsDistance=0.13f;

        private bool freeToSwap;

        public Vector2 messageImagePos;

        public PopupMessage message;

        public int index;

      
        public static int paintersCount = 10;

        public List<Painter> painters;

        public static bool isBrush = true;

        private GraphicsDeviceManager _graphicsDevice;
        private Camera _camera;
        private SoundManager _soundManager;

        private SoundEffectInstance steeringSound;

        public Car(World world, Texture2D texture, Color Color, RandomTrack _randomTrack, int _index)
            : base(world, texture, new Vector2(65.0f, 40.0f), Color)
        {

            index = _index;

            message = new PopupMessage(this);

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

            newWVertBurnoutRightFront = new Vector3(-0.1f);
            newEVertBurnoutRightFront = new Vector3(-0.1f);
            oldWVertBurnoutRightFront = new Vector3(-0.1f);
            oldEVertBurnoutRightFront = new Vector3(-0.1f);

            newWVertBurnoutLeftFront = new Vector3(-0.1f);
            newEVertBurnoutLeftFront = new Vector3(-0.1f);
            oldWVertBurnoutLeftFront = new Vector3(-0.1f);
            oldEVertBurnoutLeftFront = new Vector3(-0.1f);

            score = 27;

            driftValue = 0;
            maxBoostFrames = -1;

            currentAcc = acc;
            currentMaxVel = maxVel;

            burnoutCounter = 0;

            freeToSwap = true;

            //register collision
            _compound.OnCollision += body_OnCollision;

            messageImagePos = ConvertUnits.ToDisplayUnits( _compound.Position);// +new Vector2(1, 1));


            painters = new List<Painter>();
            for (int i = 0; i < paintersCount; i++)
            {
                Painter newPainter=new Painter(seed);
                painters.Add(newPainter);
            }

            _graphicsDevice = GameServices.GetService<GraphicsDeviceManager>();
            _camera = GameServices.GetService<Camera>();
            _soundManager = GameServices.GetService<SoundManager>();
        }

        bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (this.hasBoost)
            {
                return true;
            }

            // check if post it
            if (fixtureB.Body.UserData != null)
            {
                //update color
                int postItNumber = (int)fixtureB.Body.UserData;
                if (randomTrack.postItDreamsList[postItNumber].color==Color.White){
                    randomTrack.changePostItColor(postItNumber, this);
                }
            }
            return true;
        }

        public Vector2 ProjectedPosition
        {
            get { return ConvertUnits.ToDisplayUnits(projectedPosition); }
            set { projectedPosition = value * ConvertUnits.ToSimUnits(1); }
        }

        public void resetTrail()
        {
            mIsTrailLoop = false;
            mTrailPoints = 0;

            /*
            for (int i = 0; i < trailVertices.Count(); i++)
            {
                trailVertices[i].Position = tdPos;
            }
             */

            justStarted = true;
        }

        public void resetBoost()
        {
            boostFrames = 0;
            hasBoost = false;
            currentMaxVel = maxVel;
            currentAcc = acc;

            driftValue = 0;
        }

        public void stopSteeringSound()
        {
            if (steeringSound != null)
            {
                _soundManager.PoolSound(steeringSound, SoundManager.CarSteering);
                steeringSound = null;
            }
        }

        public void Update(GamePadState gps, KeyboardState ks, GameTime gameTime)
        {
            
            if (isActive == false)
            {

                stopSteeringSound();

                //do nothing except for moving the message position on the screen
                //ATTENTION: at the beginning of the match the inverse of the camera matrix will return NAN, therefore check for NAN when you position the message!!!
                moveMessageImage(gameTime);
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

            bool blueOnly = mColor == Color.Green || true;
            bool brownOnly = mColor == Color.Brown;

            float newAcc = 0.0f;
            if (ks.IsKeyDown(Keys.Right) && blueOnly || gps.ThumbSticks.Right.X > 0 || ks.IsKeyDown(Keys.D) && brownOnly)
            {
                _compound.AngularVelocity = 0;
                _compound.Rotation += rotVel;
            }
            if (ks.IsKeyDown(Keys.Left) && blueOnly || gps.ThumbSticks.Right.X < 0 || ks.IsKeyDown(Keys.A) && brownOnly)
            {
                _compound.AngularVelocity = 0;
                _compound.Rotation -= rotVel;
            }

            bool isSteering = ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.Right) || gps.ThumbSticks.Right.X != 0;

            if (isSteering && steeringSound == null)
            {
                steeringSound = _soundManager.GetSound(SoundManager.CarSteering);
                steeringSound.Play();
            }
            else if (!isSteering){
                stopSteeringSound();
            }

            if (ks.IsKeyDown(Keys.Up) && blueOnly || gps.ThumbSticks.Left.Y > 0 || ks.IsKeyDown(Keys.W) && brownOnly) 
            {
                newAcc = currentAcc;
            }
            if (ks.IsKeyDown(Keys.Down) && blueOnly || gps.ThumbSticks.Left.Y < 0 || ks.IsKeyDown(Keys.S) && brownOnly)
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

            if ( _compound.LinearVelocity.Length() < 0.1f)
            {
                resetTrail();
            }

            tdPos.X = _compound.Position.X;
            tdPos.Y = _compound.Position.Y;

            // Add a trail point if the player is drawing
            if ((((ks.IsKeyDown(Keys.F) && blueOnly || gps.Triggers.Right > 0) || true) && !hasBoost) && !justStarted)
            {
               if (mTrailPoints >= mMaximumTrailPoints)
                {
                    mIsTrailLoop = true;
                    mTrailPoints = 0;
                }
                mTrailPositions[mTrailPoints] = Position - mDirection * tailOffset;

                if (isBrush)
                {
                    //procedural brush 
                    for (int i = 0; i < painters.Count; i++)
                    {

                        painters[i].dx -= painters[i].ax;
                        painters[i].ax = (painters[i].ax + (painters[i].dx - tdPos.X) * painters[i].div) * painters[i].ease;
                        painters[i].dy -= painters[i].ay;
                        painters[i].ay = (painters[i].ay + (painters[i].dy - tdPos.Y) * painters[i].div) * painters[i].ease;

                        newWVert.X = painters[i].dx - mDirection.Y * brushOffset - 0 * mDirection.X * tailOffset;
                        newWVert.Y = painters[i].dy + mDirection.X * brushOffset - 0 * mDirection.Y * tailOffset;
                        newEVert.X = painters[i].dx + mDirection.Y * brushOffset - 0 * mDirection.X * tailOffset;
                        newEVert.Y = painters[i].dy - mDirection.X * brushOffset - 0 * mDirection.Y * tailOffset;

                        drawQuad(newWVert, newEVert, painters[i].oldWVert, painters[i].oldEVert, i);
                        painters[i].oldWVert = newWVert;
                        painters[i].oldEVert = newEVert;
                    }
                }
                else
                {
                    //pencil
                     newWVert.X = tdPos.X - mDirection.Y * offset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f;
                     newWVert.Y = tdPos.Y + mDirection.X * offset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f;

                     newEVert.X = tdPos.X + mDirection.Y * offset - mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f;
                     newEVert.Y = tdPos.Y - mDirection.X * offset - mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f;

                     drawQuad(newWVert, newEVert, oldWVert, oldEVert,0);

                     oldWVert = newWVert;
                     oldEVert = newEVert;
                }

                mTrailPoints++;
            }
            else
            {
                if (isBrush)
                {
                    //procedural brush 
                    for (int i = 0; i < painters.Count; i++)
                    {
                        painters[i].dx = tdPos.X;
                        painters[i].dy = tdPos.Y;
                        painters[i].oldWVert.X = painters[i].dx - mDirection.Y * brushOffset - 0 * mDirection.X * tailOffset;
                        painters[i].oldWVert.Y = painters[i].dy + mDirection.X * brushOffset - 0 * mDirection.Y * tailOffset;
                        painters[i].oldEVert.X = painters[i].dx + mDirection.Y * brushOffset - 0 * mDirection.X * tailOffset;
                        painters[i].oldEVert.Y = painters[i].dy - mDirection.X * brushOffset - 0 * mDirection.Y * tailOffset;
                    }
                }
                else
                {
                    //pencil
                    oldWVert.X = tdPos.X - mDirection.Y * offset;
                    oldWVert.Y = tdPos.Y + mDirection.X * offset;

                    oldEVert.X = tdPos.X + mDirection.Y * offset;
                    oldEVert.Y = tdPos.Y - mDirection.X * offset;
                }


                resetTrail();
                justStarted = false;
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

            //move the message position
            moveMessageImage(gameTime);
            
        }

        public void drawQuad(Vector3 newWVert,Vector3 newEVert,Vector3 oldWVert,Vector3 oldEVert, int painterIndex)
        {
            trailVertices[mTrailPoints * 6 *paintersCount +6*painterIndex + 0].Position = newWVert;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 1].Position = newEVert;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 2].Position = oldWVert;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 3].Position = oldWVert;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 4].Position = oldEVert;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 5].Position = newEVert;

            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 0].TextureCoordinate = texNW;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 1].TextureCoordinate = texNE;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 2].TextureCoordinate = texOW;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 3].TextureCoordinate = texOW;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 4].TextureCoordinate = texOE;
            trailVertices[mTrailPoints * 6 * paintersCount + 6 * painterIndex + 5].TextureCoordinate = texNE;
        }

        public void moveMessageImage(GameTime gameTime)
        {
           
            //move the message and udpate its string
            message.Update(gameTime);

            //compute direction of the message
            float distance = 250f;
            Vector2 dirVec = Vector2.Normalize(_camera.oldPosition - Position) * distance;
            //interpolate position
            messageImagePos = Vector2.Lerp( messageImagePos, Position+dirVec, 0.5f);
            //transform it to screen position
            Vector2 screenPosition = Vector2.Transform(messageImagePos, _camera.Transform);

            //check if still on screen, if not bring it back to screen!
            //set a margin
            int offsetRight=120;
            int offsetUp = 70;
            int offsetDown = 120;
            int offsetLeft = 70;
            if (screenPosition.X < offsetLeft) 
            {
                screenPosition.X = offsetLeft;
            }
            else if (screenPosition.X > _graphicsDevice.PreferredBackBufferWidth - offsetRight)
            {
                screenPosition.X = _graphicsDevice.PreferredBackBufferWidth - offsetRight;
            }
            if (screenPosition.Y < offsetUp)
            {
                screenPosition.Y = offsetUp;
            }
            else if (screenPosition.Y > _graphicsDevice.PreferredBackBufferHeight - offsetDown)
            {
                screenPosition.Y = _graphicsDevice.PreferredBackBufferHeight - offsetDown;
            }

            //avoid corner postits 
            //TODO: interpolation!

            int postitLength = 250;
            int postitHeight = 150;
            if (screenPosition.Y > _graphicsDevice.PreferredBackBufferHeight - postitHeight - offsetDown && screenPosition.X < postitLength)
            {
                screenPosition.X = MathHelper.Lerp(screenPosition.X, postitLength, 1f);
            }
            else if (screenPosition.Y > _graphicsDevice.PreferredBackBufferHeight - postitHeight - offsetDown && screenPosition.X > _graphicsDevice.PreferredBackBufferWidth - postitLength - offsetRight)
            {
                screenPosition.X = MathHelper.Lerp(screenPosition.X, _graphicsDevice.PreferredBackBufferWidth - postitLength - offsetRight, 1f);
            }
            else if (screenPosition.Y < postitHeight && screenPosition.X > _graphicsDevice.PreferredBackBufferWidth - postitLength - offsetRight)
            {
                screenPosition.X = MathHelper.Lerp(screenPosition.X, _graphicsDevice.PreferredBackBufferWidth - postitLength - offsetRight, 1f);
            }
            else if (screenPosition.Y < postitHeight && screenPosition.X < postitLength)
            {
                screenPosition.X = MathHelper.Lerp(screenPosition.X, postitLength, 1f);
            }
            

            //compute inverse matrix
            Matrix inverse = _camera.inverseTransformMatrix();
            //re-transform the vector in world coordinated
            Vector2 croppedPos = Vector2.Transform(screenPosition, inverse);
            messageImagePos = croppedPos;

            // check if Nan, since a the beginning the inverse of the matrix will be NAN, and interpolation won't work afterwards
            if (messageImagePos.X != messageImagePos.X || messageImagePos.Y != messageImagePos.Y)
            {
                messageImagePos = Vector2.Zero;
            }
           
        }

        public void KillOrthogonalVelocity(Car car, float drift)
        {
            Vector2 forwardVelocity = mDirection * Vector2.Dot(car._compound.LinearVelocity, mDirection);
            Vector2 rightVector = new Vector2(-mDirection.Y, mDirection.X);
            Vector2 rightVelocity = rightVector * Vector2.Dot(car._compound.LinearVelocity,rightVector);
            car._compound.LinearVelocity = forwardVelocity + rightVelocity * drift;

            if (burnoutCounter > 500 - 4) burnoutCounter = 0;

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

                newWVertBurnoutRightFront.X = tdPos.X - mDirection.Y * burnoutOffset + mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.X * wheelsDistance;
                newWVertBurnoutRightFront.Y = tdPos.Y + mDirection.X * burnoutOffset + mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.Y * wheelsDistance;

                newEVertBurnoutRightFront.X = tdPos.X + mDirection.Y * burnoutOffset + mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.X * wheelsDistance;
                newEVertBurnoutRightFront.Y = tdPos.Y - mDirection.X * burnoutOffset + mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f + rightVector.Y * wheelsDistance;

                burnoutsVertices[burnoutCounter * 6 + 0].Position = newWVertBurnoutRightFront;
                burnoutsVertices[burnoutCounter * 6 + 1].Position = newEVertBurnoutRightFront;
                burnoutsVertices[burnoutCounter * 6 + 2].Position = oldWVertBurnoutRightFront;
                burnoutsVertices[burnoutCounter * 6 + 3].Position = oldWVertBurnoutRightFront;
                burnoutsVertices[burnoutCounter * 6 + 4].Position = oldEVertBurnoutRightFront;
                burnoutsVertices[burnoutCounter * 6 + 5].Position = newEVertBurnoutRightFront;

                burnoutsVertices[burnoutCounter * 6 + 0].TextureCoordinate = texNW;
                burnoutsVertices[burnoutCounter * 6 + 1].TextureCoordinate = texNE;
                burnoutsVertices[burnoutCounter * 6 + 2].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 3].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 4].TextureCoordinate = texOE;
                burnoutsVertices[burnoutCounter * 6 + 5].TextureCoordinate = texNE;

                oldWVertBurnoutRightFront = newWVertBurnoutRightFront;
                oldEVertBurnoutRightFront = newEVertBurnoutRightFront;

                burnoutCounter++;


                newWVertBurnoutLeftFront.X = tdPos.X - mDirection.Y * burnoutOffset + mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.X * wheelsDistance;
                newWVertBurnoutLeftFront.Y = tdPos.Y + mDirection.X * burnoutOffset + mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.Y * wheelsDistance;

                newEVertBurnoutLeftFront.X = tdPos.X + mDirection.Y * burnoutOffset + mDirection.X * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.X * wheelsDistance;
                newEVertBurnoutLeftFront.Y = tdPos.Y - mDirection.X * burnoutOffset + mDirection.Y * tailOffset + (float)seed.NextDouble() * 0.05f - rightVector.Y * wheelsDistance;

                burnoutsVertices[burnoutCounter * 6 + 0].Position = newWVertBurnoutLeftFront;
                burnoutsVertices[burnoutCounter * 6 + 1].Position = newEVertBurnoutLeftFront;
                burnoutsVertices[burnoutCounter * 6 + 2].Position = oldWVertBurnoutLeftFront;
                burnoutsVertices[burnoutCounter * 6 + 3].Position = oldWVertBurnoutLeftFront;
                burnoutsVertices[burnoutCounter * 6 + 4].Position = oldEVertBurnoutLeftFront;
                burnoutsVertices[burnoutCounter * 6 + 5].Position = newEVertBurnoutLeftFront;

                burnoutsVertices[burnoutCounter * 6 + 0].TextureCoordinate = texNW;
                burnoutsVertices[burnoutCounter * 6 + 1].TextureCoordinate = texNE;
                burnoutsVertices[burnoutCounter * 6 + 2].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 3].TextureCoordinate = texOW;
                burnoutsVertices[burnoutCounter * 6 + 4].TextureCoordinate = texOE;
                burnoutsVertices[burnoutCounter * 6 + 5].TextureCoordinate = texNE;

                oldWVertBurnoutLeftFront = newWVertBurnoutLeftFront;
                oldEVertBurnoutLeftFront = newEVertBurnoutLeftFront;

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

                oldWVertBurnoutRightFront.X = tdPos.X - mDirection.Y * burnoutOffset + rightVector.X * wheelsDistance;
                oldWVertBurnoutRightFront.Y = tdPos.Y + mDirection.X * burnoutOffset + rightVector.Y * wheelsDistance;

                oldEVertBurnoutRightFront.X = tdPos.X + mDirection.Y * burnoutOffset + rightVector.X * wheelsDistance;
                oldEVertBurnoutRightFront.Y = tdPos.Y - mDirection.X * burnoutOffset + rightVector.Y * wheelsDistance;

                oldWVertBurnoutLeftFront.X = tdPos.X - mDirection.Y * burnoutOffset - rightVector.X * wheelsDistance;
                oldWVertBurnoutLeftFront.Y = tdPos.Y + mDirection.X * burnoutOffset - rightVector.Y * wheelsDistance;

                oldEVertBurnoutLeftFront.X = tdPos.X + mDirection.Y * burnoutOffset - rightVector.X * wheelsDistance;
                oldEVertBurnoutLeftFront.Y = tdPos.Y - mDirection.X * burnoutOffset - rightVector.Y * wheelsDistance;
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
            float margin = 8f; //0.2 for step=3

            float totalDist = (distBack - randomTrack.pathWidth) + (distFront - randomTrack.pathWidth);
            float ratio = (distBack - randomTrack.pathWidth) / totalDist;
            Vector2 _projectedPosition = randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count] + (randomTrack.curvePointsMiddle[nextMiddlePoint % randomTrack.curvePointsMiddle.Count] - randomTrack.curvePointsMiddle[currentMiddlePoint % randomTrack.curvePointsMiddle.Count]) * ratio;

            if (distFront < randomTrack.pathWidth + margin)
            {
                currentMiddlePoint = nextMiddlePoint;
            }
    
            return _projectedPosition;
        }

        public void DrawMessage(SpriteBatch spriteBatch)
        {
            message.Draw(spriteBatch);
        }

        public void Draw(SpriteBatch spriteBatch, out VertexPositionColorTexture[] vertices, out VertexPositionColorTexture[] _burnoutsVertices)
        {
            vertices = trailVertices;
            _burnoutsVertices = burnoutsVertices;

            //draw projected position
           //   spriteBatch.Draw(mDummyTexture,ConvertUnits.ToDisplayUnits( projectedPosition),
           //                                null, mColor, 0, Vector2.Zero, Vector2.One*10, SpriteEffects.None,
           //                                0.9f);

            // draw message position
           // spriteBatch.Draw(mDummyTexture, messageImagePos,
           //                                null, mColor, 0, Vector2.Zero, Vector2.One*10, SpriteEffects.None,
           //                                0.9f);


           // message.Draw(spriteBatch);
            

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



