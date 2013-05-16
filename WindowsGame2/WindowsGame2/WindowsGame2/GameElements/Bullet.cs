using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace WindowsGame2.GameElements
{
    public class Bullet
    {
        public Body bulletPhysicsObject;
        public Texture2D dummyTexture;
        public bool isGoing;

        private Car car;
        private Vector2 shootingDirection;

        private GraphicsDeviceManager graphics;

        private Texture2D pencilTexture;

        public VertexPositionColorTexture[] laserVertices;

        public Vector2 tangent;

        SoundEffect laserSound;
        SoundEffectInstance laserSoundInstance;

        public Bullet(World world, Car _car, Texture2D _dummyTexture)
        {
            car = _car;
            dummyTexture = _dummyTexture;
            
            bulletPhysicsObject=  BodyFactory.CreateRectangle(world, 0.1f, 0.1f, 1f, Vector2.Zero);
            bulletPhysicsObject.BodyType = BodyType.Dynamic;

            bulletPhysicsObject.IsSensor = true;
            bulletPhysicsObject.IsBullet = true;

            isGoing = false;

            bulletPhysicsObject.IgnoreCollisionWith(car._compound);

            bulletPhysicsObject.CollisionCategories = Category.Cat20; 


            bulletPhysicsObject.OnCollision += bullet_OnCollision;
            graphics = GameServices.GetService<GraphicsDeviceManager>();

            pencilTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/pencilTextire");


            laserVertices = new VertexPositionColorTexture[6];

            laserVertices[0].Color = car.mColor;
            laserVertices[1].Color = car.mColor;
            laserVertices[2].Color = car.mColor;
            laserVertices[3].Color = car.mColor;
            laserVertices[4].Color = car.mColor;
            laserVertices[5].Color = car.mColor;

            laserVertices[0].TextureCoordinate = new Vector2(0,0);
            laserVertices[1].TextureCoordinate = new Vector2(1,0);
            laserVertices[2].TextureCoordinate = new Vector2(0,1);
            laserVertices[3].TextureCoordinate = new Vector2(1, 1);
            laserVertices[4].TextureCoordinate = new Vector2(0, 1);
            laserVertices[5].TextureCoordinate = new Vector2(1, 0);


            laserSound = GameServices.GetService<ContentManager>().Load<SoundEffect>("Sounds/laserorribiledimmerda");

            laserSoundInstance=laserSound.CreateInstance();

            
        }

        private bool checkIfOffScreen(Vector2 vec)
        {
            Vector2 screenPosition = Vector2.Transform(ConvertUnits.ToDisplayUnits(vec), car._camera.Transform);
            if (screenPosition.X < 0 || screenPosition.X > graphics.PreferredBackBufferWidth || screenPosition.Y < 0 || screenPosition.Y > graphics.PreferredBackBufferHeight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            
            if (isGoing)
            {
                // check if obstacle
                if (fixtureB.Body.UserData != null)
                {
                    //update color
                    int obstacleNumber = (int)fixtureB.Body.UserData;

                    

                    //is obstacle
                    if (obstacleNumber >= 1000)
                    {
                        if (car.randomTrack.polygonList[obstacleNumber - 1000].Color != car.mColor)
                        {
                            car.randomTrack.polygonList[obstacleNumber - 1000].Color = car.mColor;
                            car.randomTrack.polygonList[obstacleNumber - 1000].compound.IgnoreCollisionWith(car._compound);
                            car.randomTrack.polygonList[obstacleNumber - 1000].compound.RestoreCollisionWith(car.randomTrack.polygonList[obstacleNumber - 1000].currentIgnoredBody);
                            car.randomTrack.polygonList[obstacleNumber - 1000].currentIgnoredBody = car._compound;
                        }

                        reset();
                    }

                    //is postit
                    if (obstacleNumber < 1000)
                    {
                        if (car.randomTrack.postItDreamsList[obstacleNumber].color == Color.White)
                        {
                            car.randomTrack.changePostItColor(obstacleNumber, car);

                            reset();
                        }
                    }
                }
            }
            
            return true;
        }

        public void Shoot()
        {
            isGoing = true;

            bulletPhysicsObject.ResetDynamics();
            bulletPhysicsObject.Position = car._compound.Position;
            bulletPhysicsObject.Rotation = car._compound.Rotation;
            shootingDirection = car.mDirection;

            tangent = new Vector2(-car.mDirection.Y, car.mDirection.X);

            laserSoundInstance.Play();
        }

        public void Update()
        {

            if (isGoing)
            {

                bulletPhysicsObject.LinearVelocity = shootingDirection*30f;

                if (checkIfOffScreen(bulletPhysicsObject.Position))
                {
                    reset();
                }
            }

        }

        public void reset()
        {
            isGoing = false;
            bulletPhysicsObject.ResetDynamics();

            laserSoundInstance.Stop();
         //   car.bulletIsShot = false;
        }

        void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            
            
            
            /*point1 = point1 + tangent * 10f;
            point2 = point2 + tangent * 10f;

            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            Vector2 lengthWidth = new Vector2(length, width);

            batch.Draw(dummyTexture, point1, null, color, angle, Vector2.Zero, lengthWidth, SpriteEffects.None, 0);
             */

            float widthFactor = 3f;

            laserVertices[0].Position = new Vector3(car._compound.Position - tangent / widthFactor, -0.1f);
            laserVertices[1].Position = new Vector3(car._compound.Position + tangent / widthFactor, -0.1f);
            laserVertices[2].Position = new Vector3(bulletPhysicsObject.Position - tangent / widthFactor, -0.1f);
            laserVertices[3].Position = new Vector3(bulletPhysicsObject.Position + tangent / widthFactor, -0.1f);
            laserVertices[4].Position = new Vector3(bulletPhysicsObject.Position - tangent / widthFactor, -0.1f);
            laserVertices[5].Position = new Vector3(car._compound.Position + tangent / widthFactor, -0.1f);




        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isGoing)
            {
                DrawLine(spriteBatch, 20f, car.mColor, ConvertUnits.ToDisplayUnits(bulletPhysicsObject.Position), car.Position);
               // spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(bulletPhysicsObject.Position),
                //                               null, car.mColor, 0, Vector2.Zero, Vector2.One * 10, SpriteEffects.None,
                //                               0.9f);

            }
        }
    }
}
