using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.SamplesFramework;

namespace WindowsGame2.GameElements
{
    public class PostItQuote
    {
        public Vector2 postItSize;
        public Vector2 textureScaleVec;
        public Vector2 position;
        public float rotation;
        public Texture2D quoteTexture;

        public PostItQuote(int middlePoint, Vector2 _postItSize, RandomTrack randomTrack, Texture2D _quoteTexture, Random random)
        {

            postItSize = _postItSize;

            float randomOffset = (float)MathHelper.Lerp(-0.90f, 0.90f, (float)random.NextDouble());
            position = randomTrack.curvePointsMiddle[middlePoint] - randomTrack.normals[middlePoint] * randomOffset;
            rotation = (float)MathHelper.Lerp(-MathHelper.Pi / 4f, MathHelper.Pi / 4f, (float)random.NextDouble());

            quoteTexture = _quoteTexture;
            textureScaleVec = new Vector2(postItSize.X / (float)quoteTexture.Width, postItSize.Y / (float)quoteTexture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(quoteTexture, ConvertUnits.ToDisplayUnits(position), null, Color.Yellow, rotation, Vector2.Zero, textureScaleVec, SpriteEffects.None, 1f);
        }
    }
}
