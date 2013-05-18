using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Common;
using WindowsGame2.GameElements;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame2
{
    public class Camera
    {
        //The viewport we want the camera to use (holds dimensions and so on)
        public Viewport View
        {
            get;
            private set;
        }

        //Where to point the center of the camera (0x0 will be the center of the viewport)
        public Vector2 Position
        {
            get;
            private set;
        }

        //center of focus for the camera
        public Vector2 FocusPoint
        {
            get;
            private set;
        }

        //The zoom scalar (1.0f = 100% zoom level)
        public float Zoom
        {
            get;
            set;
        }

        //Amount to rotate the camera
        public float Rotation
        {
            get;
            private set;
        }

        //Our camera's transform matrix
        public Matrix Transform;
        //{
        //    get;
        //    private set;
        //}

        public Matrix TransformNoZoom;

        //Our camera's transform matrix
        public Matrix ViewMatrix
        {
            get;
            private set;
        }

        //Our camera's transform matrix
        public Matrix ProjectionMatrix
        {
            get;
            private set;
        }

        //Used to matching the rotation of the object, or any value you wish
        public float SourceRotationOffset
        {
            get;
            private set;
        }

        Random random;

        Vector2 _screenCenter;

        public int firstCarIndex;
        public int lastCarIndex;

        public Vector2 oldPosition;
        private bool firstTime;

        public bool raceCanStart;

        public double timer;
        public bool updateTimer;
        public bool timerCanStart;
        public StringWriter stringWriter321 = new StringWriter();

        public bool timerGoAwayBastard = false;

        public CounterIndicator counterIndicator;
        public double timerGoAway;

        private static float FourPlayersFullHd = 0.95f;
        private static float ThreePlayersFullHd = 0.97f;
        private static float TwoPlayersFullHd = 0.99f;
        private static float FourPlayersHd = 0.91f;
        private static float ThreePlayersHd = 0.93f;
        public static float TwoPlayersHd = 0.95f;

        private bool firstTimeSound = false;

        private bool isFullHd;
        private float initialZoom;

        public double timerShowOff;
        public double timerShowOffMax = 1000;
        public bool timerShowOffCanStart = true;
        public bool updateTimerShowOff=false;

        ScreenRenderer screenRenderer;

        private float savedRotation;

        public bool firstTimeNonGongolare = true;

        /// <summary>
        /// Initialize a new Camera object
        /// </summary>
        /// <param name="view">The viewport we want the camera to use (holds dimensions and so on)</param>
        /// <param name="position">Where to position our camera relative to the focus point</param>
        /// <param name="focus">Where to point the center of the camera (0x0 will be the center of the viewport)</param>
        /// <param name="zoom">How much we want the camera zoomed by default</param>
        /// <param name="rotation">How much we want the camera to be rotated by default</param>
        public Camera(Viewport view, Vector2 position, Vector2 focus, float rotation, int numberOfCars, bool _isFullHd)
        {

            firstTimeNonGongolare = true;
            screenRenderer = GameServices.GetService<ScreenRenderer>();

            isFullHd = _isFullHd;

            View = view;
            Position = position;

            Rotation = rotation;
            random = new Random(DateTime.Now.Millisecond);
            FocusPoint = focus;
            _screenCenter = new Vector2(View.Width / 2f, View.Height / 2f);

            raceCanStart = true;
            firstTime = true;

            timer = 0;
            updateTimer = false;
            timerCanStart = false;

            stringWriter321.nCharacters = 3;

            counterIndicator = new CounterIndicator();

            timerGoAwayBastard = false;
            timerGoAway = 0;

            setCameraZoom(numberOfCars);

            timerShowOff = 0;
            timerShowOffCanStart = true;
            updateTimerShowOff = false;

        }

        public void setCameraZoom(int numberOfCars)
        {
            Zoom = computeCameraZoom(numberOfCars);
            initialZoom = Zoom;
        }

        private float computeCameraZoom(int numberOfCars)
        {
            float resultingZoom = 0;
            if (isFullHd)
            {
                if (numberOfCars == 4)
                {
                    resultingZoom = FourPlayersFullHd;
                }
                else if (numberOfCars == 3)
                {
                    resultingZoom = ThreePlayersFullHd;
                }
                else if (numberOfCars == 2)
                {
                    resultingZoom = TwoPlayersFullHd;
                }
            }
            else
            {
                if (numberOfCars == 4)
                {
                    resultingZoom = FourPlayersHd;
                }
                else if (numberOfCars == 3)
                {
                    resultingZoom = ThreePlayersHd;
                }
                else if (numberOfCars == 2)
                {
                    resultingZoom = TwoPlayersHd;
                }
            }

            return resultingZoom;
        }


        public void Update(GameTime gametime, List<Car> Cars, int eliminatedCarsNumber)
        {
            int activeCars = Cars.Count - eliminatedCarsNumber;
            Zoom = MathHelper.Lerp(Zoom, computeCameraZoom(activeCars), 0.1f);

            if (timerCanStart)
            {
                counterIndicator.enter();
            }

            if (timerGoAwayBastard)
            {
                timerGoAway += gametime.ElapsedGameTime.TotalMilliseconds;
                int timerGoAwayDelay = 1000;

                if (Vector2.Distance(counterIndicator.currentPostion, counterIndicator.outPosition) < 0.1f)
                {
                    timerGoAwayBastard = false;
                }

                if (timerGoAway > timerGoAwayDelay)
                {
                    counterIndicator.exit();
                    counterIndicator.changeTexture(3, false);
                }

            }

            if (updateTimer)
            {

                timer += gametime.ElapsedGameTime.TotalMilliseconds;
                checkTimer(Cars);

                counterIndicator.enter();
            }

            if (updateTimerShowOff)
            {
                int winnerIndex=0;
                for (int i=0; i<Cars.Count; i++){
                    if (Cars[i].isVisible)
                    {
                        winnerIndex = i;
                        break;
                    }
                }

                //gongola
                if (!firstTimeNonGongolare)
                {
                    Cars[winnerIndex].isGloating = true;
                    Cars[winnerIndex]._compound.Rotation = MathHelper.Lerp(Cars[winnerIndex]._compound.Rotation, savedRotation + (float)Math.PI * 2, ((float)timerShowOff / (float)timerShowOffMax) * ((float)timerShowOff / (float)timerShowOffMax));
                }
                timerShowOff += gametime.ElapsedGameTime.TotalMilliseconds;
                checkShowOffTimer(Cars);

                counterIndicator.enter();
            }
            else
            {
                for (int i=0; i<Cars.Count; i++)
                    Cars[i].isGloating = false;
            }

            //choose interpolation weight and cars weights depending on the number of players
            float interpWeight = 0.1f;
            float firstCarWeight = 0.5f;
            if (Cars.Count == 4)
            {
                interpWeight = 0.1f;
                firstCarWeight = 0.6f;
            }
            else if (Cars.Count == 3)
            {
                interpWeight = 0.1f;
                firstCarWeight = 0.55f;
            }
            else if (Cars.Count == 2)
            {
                interpWeight = 0.1f;
                firstCarWeight = 0.55f;
            }

            //set taget position
            Vector2 objectPosition_ = ConvertUnits.ToDisplayUnits(firstCarWeight * Cars[firstCarIndex]._compound.Position + (1 - firstCarWeight) * Cars[lastCarIndex]._compound.Position);

            //initialize old position
            if (firstTime)
            {

                oldPosition = objectPosition_ + new Vector2(3000);
                firstTime = false;

                firstTimeSound = true;
            }

            if (Vector2.Distance(oldPosition, objectPosition_) < 0.6f)
            {
                /*
                if (timerCanStart)
                {
                    activateTimer();
                }
                */

                
                if (timerShowOffCanStart )
                {
                    activateShowOffTimer(Cars);
                }

            }

            //interpolate
            Vector2 objectPosition = new Vector2();
            objectPosition.X = MathHelper.Lerp(oldPosition.X, objectPosition_.X, interpWeight);
            objectPosition.Y = MathHelper.Lerp(oldPosition.Y, objectPosition_.Y, interpWeight);

            //set old position
            oldPosition = objectPosition;


            /* Create a transform matrix through position, scale, rotation, and translation to the focus point
             * We use Math.Pow on the zoom to speed up or slow down the zoom.  Both X and Y will have the same zoom levels
             * so there will be no stretching.
             * */
            float objectRotation = 0;
            float deltaRotation = 0;


            Transform = Matrix.CreateTranslation(new Vector3(-objectPosition, 0)) *
                Matrix.CreateScale(new Vector3((float)Math.Pow(Zoom, 10), (float)Math.Pow(Zoom, 10), 1)) *  // UUUUHHHH!!! BUGGG!!! THE LAST 1 WAS 0 IN CASE...Wrong? --> seems yes! http://xboxforums.create.msdn.com/forums/t/28476.aspx
                Matrix.CreateRotationZ(-objectRotation + deltaRotation) *
                Matrix.CreateTranslation(new Vector3(FocusPoint.X, FocusPoint.Y, 0));


           TransformNoZoom = Matrix.CreateTranslation(new Vector3(-objectPosition, 0)) *
                Matrix.CreateScale(new Vector3((float)Math.Pow(initialZoom, 10), (float)Math.Pow(initialZoom, 10), 1)) *  // UUUUHHHH!!! BUGGG!!! THE LAST 1 WAS 0 IN CASE...Wrong? --> seems yes! http://xboxforums.create.msdn.com/forums/t/28476.aspx
                Matrix.CreateRotationZ(-objectRotation + deltaRotation) *
                Matrix.CreateTranslation(new Vector3(FocusPoint.X, FocusPoint.Y, 0));

            //create also projection and viewMatrix for the shaders
            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(View.Width) * (1 / (float)Math.Pow(Zoom, 10)),
                                                              ConvertUnits.ToSimUnits(View.Height) * (1 / (float)Math.Pow(Zoom, 10)), 0f, 0f, 1f);
            ViewMatrix = Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(objectPosition) + ConvertUnits.ToSimUnits(_screenCenter) * (1 / (float)Math.Pow(Zoom, 10)), 0f));
        }

        public void activateShowOffTimer(List<Car> Cars)
        {
            updateTimerShowOff = true;
            timerShowOffCanStart = false;

            savedRotation = Cars[0]._compound.Rotation;
        }

        public void activateTimer()
        {
            updateTimer = true;
            timerCanStart = false;
        }

        public void checkShowOffTimer(List<Car> Cars)
        {
            if (timerShowOff > timerShowOffMax)
            {

                timerShowOff = 0;
                updateTimerShowOff = false;
                activateTimer();
                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].isVisible = true;
                }
            }
        }

        public void checkTimer(List<Car> Cars)
        {

            float totalMilliseconds = 1500;
            if (timer > totalMilliseconds)
            {
                screenRenderer.setHappyToAllPlayers();

                raceCanStart = true;
                timer = 0;
                updateTimer = false;

                Vector2 position = (Cars[0]._compound.Position + Cars[Cars.Count - 1]._compound.Position) / 2f;


                counterIndicator.changeTexture(0, true);

                timerGoAwayBastard = true;
                timerGoAway = 0;

                if (firstTimeSound)
                {
                    firstTimeSound = false;
                    GameServices.GetService<SoundManager>().PlaySong(SoundManager.GameSong, true);

                    for (int i = 0; i < Cars.Count; i++)
                    {
                        Cars[i].hasNeverStarted = false;
                    }
                }
            }
            else if (timer > totalMilliseconds / 3f * 2f)
            {
                Vector2 position = (Cars[0]._compound.Position + Cars[Cars.Count - 1]._compound.Position) / 2f;

                counterIndicator.changeTexture(1, true);
            }
            else if (timer > totalMilliseconds / 3f * 1f)
            {
                Vector2 position = (Cars[0]._compound.Position + Cars[Cars.Count - 1]._compound.Position) / 2f;

                counterIndicator.changeTexture(2, true);
            }
            else if (timer > 0.001f)
            {
                Vector2 position = (Cars[0]._compound.Position + Cars[Cars.Count - 1]._compound.Position) / 2f;

                counterIndicator.changeTexture(3, true);

            }
        }

        public Matrix inverseTransformMatrix()
        {
            return Matrix.Invert(Transform);
        }


        public Matrix inverseTransformMatrixNoZoom()
        {
            return Matrix.Invert(TransformNoZoom);
        }
    }
}
