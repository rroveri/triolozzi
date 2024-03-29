﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.SamplesFramework;

namespace WindowsGame2.GameElements
{
    public class PostItDream
    {

        public int isNightmare {
            get {
                if (indexNumber > 3) return 1;
                else return 0;
            }
        }
        public int indexNumber;
        public Vector2 postItSize;
        public Vector2 textureScaleVec;
        public Vector2 position;
        public float rotation;
        public TexturePhysicsObject contourPhysicsObject;
        public Color color;
        
        private Texture2D backgroundTexture;
        private Texture2D foregroundTexture;

        public PostItDream(int middlePoint, Vector2 _postItSize, int indexNumber, RandomTrack randomTrack, World world, Texture2D _foregroundTexture, int postItIndex, Random random)
        {

            postItSize = _postItSize;
            
            float randomOffset = (float)MathHelper.Lerp(-0.30f, 0.30f, (float)random.NextDouble());
            position = randomTrack.curvePointsMiddle[middlePoint] - randomTrack.normals[middlePoint] * randomOffset;
            rotation = (float)MathHelper.Lerp(-MathHelper.Pi / 4f, MathHelper.Pi / 4f, (float)random.NextDouble());
            Texture2D textureContour = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitContourNightmare");
            
            color = Color.White;

            this.indexNumber = indexNumber;

            if (isNightmare == 0)
            {
                textureContour = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitContourWish");
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitwish");
            }
            else if (isNightmare == 1)
            {
                textureContour = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitContourNightmare");
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitnightmare");
            }
            else
            {
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitFree");
            }

            textureContour = _foregroundTexture;

            contourPhysicsObject = new TexturePhysicsObject(world, textureContour, postItSize, Color.White);
            contourPhysicsObject._compound.Position = position;
            contourPhysicsObject._compound.Rotation = rotation;
            contourPhysicsObject._compound.UserData = postItIndex;
            contourPhysicsObject._compound.IsSensor = true;

            foregroundTexture = _foregroundTexture;
            textureScaleVec = new Vector2(postItSize.X / (float)backgroundTexture.Width, postItSize.Y / (float)backgroundTexture.Height);

        }

        public void Draw(SpriteBatch spriteBatch){
            spriteBatch.Draw(backgroundTexture, ConvertUnits.ToDisplayUnits(position), null, Color.White, rotation, contourPhysicsObject._origin,  textureScaleVec, SpriteEffects.None, 1f);
            spriteBatch.Draw(foregroundTexture, ConvertUnits.ToDisplayUnits(position), null, color, rotation, contourPhysicsObject._origin, textureScaleVec/1.4f, SpriteEffects.None, 1f);         
        }
    }
}
