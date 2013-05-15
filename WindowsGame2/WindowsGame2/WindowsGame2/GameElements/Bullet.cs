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



        public Bullet(World world, Car _car, Texture2D _dummyTexture)
        {
            car = _car;
            dummyTexture = _dummyTexture;

            bulletPhysicsObject=  BodyFactory.CreateRectangle(world, 64, 64, 1f, Vector2.Zero);

            bulletPhysicsObject.IsSensor = true;

            isGoing = false;

            bulletPhysicsObject.OnCollision += bullet_OnCollision;
        }

        bool bullet_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            /*
            // check if post it
            if (fixtureB.Body.UserData != null)
            {fixtureB.Body.IsBullet
                //update color
                int postItNumber = (int)fixtureB.Body.UserData;
                if (randomTrack.postItDreamsList[postItNumber].color == Color.White)
                {
                    randomTrack.changePostItColor(postItNumber, this);
                }
            }
             */
            return true;
        }

        public void Shoot()
        {
            isGoing = true;
            bulletPhysicsObject.Position = car._compound.Position;
            bulletPhysicsObject.Rotation = car._compound.Rotation;
            shootingDirection = car.mDirection;
        }

        public void Update()
        {
            if (isGoing)
            {
                bulletPhysicsObject.Position = bulletPhysicsObject.Position + shootingDirection;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(bulletPhysicsObject.Position),
                                           null, Color.Red, 0, Vector2.Zero, Vector2.One*10, SpriteEffects.None,
                                           0.9f);
        }
    }
}
