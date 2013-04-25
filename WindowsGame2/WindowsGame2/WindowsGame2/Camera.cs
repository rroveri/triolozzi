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
            private set;
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

        /// <summary>
        /// Initialize a new Camera object
        /// </summary>
        /// <param name="view">The viewport we want the camera to use (holds dimensions and so on)</param>
        /// <param name="position">Where to position our camera relative to the focus point</param>
        /// <param name="focus">Where to point the center of the camera (0x0 will be the center of the viewport)</param>
        /// <param name="zoom">How much we want the camera zoomed by default</param>
        /// <param name="rotation">How much we want the camera to be rotated by default</param>
        public Camera(Viewport view, Vector2 position, Vector2 focus, float zoom, float rotation)
        {
            View = view;
            Position = position;
            Zoom = zoom;
            Rotation = rotation;
            random = new Random(DateTime.Now.Millisecond);
            FocusPoint = focus;
            _screenCenter = new Vector2(View.Width / 2f, View.Height / 2f);

            raceCanStart = true;
            firstTime = true;
        }

        
        public void Update(GameTime gametime, List<Car> Cars)
        {

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
            if (firstTime){

                oldPosition = objectPosition_+new Vector2(3000);
                firstTime = false;
            }

            if (Vector2.Distance(oldPosition, objectPosition_) < 0.6f)
            {
                raceCanStart = true;
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
                
            
            
            //create also projection and viewMatrix for the shaders
            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(View.Width) * (1 / (float)Math.Pow(Zoom, 10)),
                                                              ConvertUnits.ToSimUnits(View.Height) * (1 / (float)Math.Pow(Zoom, 10)), 0f, 0f,1f);
            ViewMatrix = Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(objectPosition) + ConvertUnits.ToSimUnits(_screenCenter) * (1 / (float)Math.Pow(Zoom, 10)), 0f));
        }

        

        public Matrix inverseTransformMatrix(){
            return Matrix.Invert(Transform);
        }
    }
}
