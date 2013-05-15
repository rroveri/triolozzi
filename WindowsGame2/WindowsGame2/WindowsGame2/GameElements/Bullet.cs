using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Factories;

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
                        car.randomTrack.polygonList[obstacleNumber - 1000].Color = car.mColor;
                        car.randomTrack.polygonList[obstacleNumber - 1000].compound.IgnoreCollisionWith(car._compound);
                        car.randomTrack.polygonList[obstacleNumber - 1000].compound.RestoreCollisionWith(car.randomTrack.polygonList[obstacleNumber - 1000].currentIgnoredBody);
                        car.randomTrack.polygonList[obstacleNumber - 1000].currentIgnoredBody = car._compound;

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
        }

        public void Update()
        {
          //  bulletPhysicsObject.ResetDynamics();
            if (isGoing)
            {
               // bulletPhysicsObject.ApplyForce(shootingDirection*2000f);
                bulletPhysicsObject.LinearVelocity = shootingDirection*30f;
               // bulletPhysicsObject.Position = bulletPhysicsObject.Position + shootingDirection/2.5f;

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
        }

        void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            Vector2 lengthWidth = new Vector2(length, width);

            batch.Draw(dummyTexture, point1, null, color, angle, Vector2.Zero, lengthWidth, SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isGoing)
            {
                DrawLine(spriteBatch, 10f, car.mColor, ConvertUnits.ToDisplayUnits(bulletPhysicsObject.Position), car.Position);
                spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(bulletPhysicsObject.Position),
                                               null, car.mColor, 0, Vector2.Zero, Vector2.One * 10, SpriteEffects.None,
                                               0.9f);

            }
        }
    }
}
