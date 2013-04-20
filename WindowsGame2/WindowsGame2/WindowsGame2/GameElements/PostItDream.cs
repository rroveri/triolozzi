using System;
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

        public int isNightmare;
        public Vector2 postItSize;
        public Vector2 textureScaleVec;
        public Vector2 position;
        public float rotation;
        public TexturePhysicsObject contourPhysicsObject;
        public Color color;
        
        private Texture2D backgroundTexture;
        private Texture2D foregroundTexture;

        public PostItDream(int middlePoint, Vector2 _postItSize, int _isNightmare, RandomTrack randomTrack, World world, Texture2D _foregroundTexture, int postItIndex, Random random)
        {

            postItSize = _postItSize;
            
            float randomOffset = (float)MathHelper.Lerp(-0.30f, 0.30f, (float)random.NextDouble());
            position = randomTrack.curvePointsMiddle[middlePoint] - randomTrack.normals[middlePoint] * randomOffset;
            rotation = (float)MathHelper.Lerp(-MathHelper.Pi / 4f, MathHelper.Pi / 4f, (float)random.NextDouble());

            contourPhysicsObject = new TexturePhysicsObject(world, GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitContour"), postItSize, Color.White);
            contourPhysicsObject._compound.Position = position;
            contourPhysicsObject._compound.Rotation = rotation;
            contourPhysicsObject._compound.UserData = postItIndex;
            contourPhysicsObject._compound.IsSensor = true;
            color = Color.White;

            isNightmare = _isNightmare;

            if (isNightmare == 0)
            {
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitwish");
            }
            else if (isNightmare == 1)
            {
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitnightmare");
            }
            else
            {
                backgroundTexture = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitFree");
            }

            foregroundTexture = _foregroundTexture;
            textureScaleVec = new Vector2(postItSize.X / (float)backgroundTexture.Width, postItSize.Y / (float)backgroundTexture.Height);

        }

        public void Draw(SpriteBatch spriteBatch){
            spriteBatch.Draw(backgroundTexture, ConvertUnits.ToDisplayUnits(position), null, Color.Yellow, rotation, contourPhysicsObject._origin,  textureScaleVec, SpriteEffects.None, 1f);
            spriteBatch.Draw(foregroundTexture, ConvertUnits.ToDisplayUnits(position), null, color, rotation, contourPhysicsObject._origin, textureScaleVec, SpriteEffects.None, 1f);         
        }
    }
}
