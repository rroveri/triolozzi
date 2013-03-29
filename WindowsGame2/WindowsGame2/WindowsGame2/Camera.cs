using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.SamplesFramework;

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
        public Matrix Transform
        {
            get;
            private set;
        }

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

        //The source object to follow
        public Car Source
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

        /// <summary>
        /// Initialize a new Camera object
        /// </summary>
        /// <param name="view">The viewport we want the camera to use (holds dimensions and so on)</param>
        /// <param name="position">Where to point the center of the camera (0x0 will be the center of the viewport)</param>
        public Camera(Viewport view, Vector2 position)
        {
            View = view;
            Position = position;
            Zoom = 1.0f;
            Rotation = 0;
            random = new Random();
            FocusPoint = new Vector2(view.Width / 2, view.Height / 2);
            _screenCenter = new Vector2(View.Width / 2f, View.Height / 2f);
        }

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
            random = new Random();
            FocusPoint = focus;
            _screenCenter = new Vector2(View.Width / 2f, View.Height / 2f);
        }

        
        public void Update(GameTime gametime)
        {
            
            /* Create a transform matrix through position, scale, rotation, and translation to the focus point
             * We use Math.Pow on the zoom to speed up or slow down the zoom.  Both X and Y will have the same zoom levels
             * so there will be no stretching.
             * */
            Vector2 objectPosition = Source != null ? Source.Position : Position;
            float objectRotation = Source != null ? Source._compound.Rotation : Rotation;
            float deltaRotation = Source != null ? SourceRotationOffset : 0.0f;

            //headache!!!
            objectRotation = 0;
            deltaRotation = 0;

            Transform = Matrix.CreateTranslation(new Vector3(-objectPosition, 0)) *
                Matrix.CreateScale(new Vector3((float)Math.Pow(Zoom, 10), (float)Math.Pow(Zoom, 10), 0)) *
                Matrix.CreateRotationZ(-objectRotation + deltaRotation) *
                Matrix.CreateTranslation(new Vector3(FocusPoint.X, FocusPoint.Y, 0));

            
            //create also projection and viewMatrix for the shaders
            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(View.Width) * (1 / (float)Math.Pow(Zoom, 10)),
                                                              ConvertUnits.ToSimUnits(View.Height) * (1 / (float)Math.Pow(Zoom, 10)), 0f, 0f,1f);
            ViewMatrix = Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(Source.Position) + ConvertUnits.ToSimUnits(_screenCenter) * (1 / (float)Math.Pow(Zoom, 10)), 0f));

       
        }

     

        /// <summary>
        /// Resets the camera to default values
        /// </summary>
        private void Reset()
        {
            Position = Vector2.Zero;
            Rotation = 0;
            Zoom = 1.0f;
            Source = null;
        }


        public void Follow(Car source, float rotationOffset)
        {
            Source = source;
            SourceRotationOffset = rotationOffset;
        }

    }
}
