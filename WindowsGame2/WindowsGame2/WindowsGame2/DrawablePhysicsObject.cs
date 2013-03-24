using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace WindowsGame2
{
    public class DrawablePhysicsObject
    {
        // Because Farseer uses 1 unit = 1 meter we need to convert
        // between pixel coordinates and physics coordinates.
        // I've chosen to use the rule that 100 pixels is one meter.
        // We have to take care to convert between these two
        // coordinate-sets wherever we mix them!

        Vector2 textureOrigin = new Vector2();
 
        public const float unitToPixel = 100.0f;
        public const float pixelToUnit = 1 / unitToPixel;
 
        public Body body;
        public Vector2 Position
        {
            get { return body.Position * unitToPixel; }
            set { body.Position = value * pixelToUnit; }
        }
 
        public Texture2D texture;
        public Color color;
 
        private Vector2 size;
        public Vector2 Size
        {
            get { return size * unitToPixel; }
            set { size = value * pixelToUnit; }
        }

        private Vector2 scale;
 
        ///The farseer simulation this object should be part of
        ///The image that will be drawn at the place of the body
        ///The size in pixels
        ///The mass in kilograms
        public DrawablePhysicsObject(World world, Texture2D texture, Vector2 size, float mass, Color color)
        {
            body = BodyFactory.CreateRectangle(world, size.X * pixelToUnit, size.Y * pixelToUnit, 1);
            body.BodyType = BodyType.Dynamic;
            
 
            this.Size = size;
            this.texture = texture;
            this.color = color;

            scale = new Vector2(Size.X / (float)texture.Width, Size.Y / (float)texture.Height);
        }
 
        public void Draw(SpriteBatch spriteBatch)
        {
            textureOrigin.X = texture.Width / 2.0f;
            textureOrigin.Y = texture.Height / 2.0f;
            spriteBatch.Draw(texture, Position, null, color, body.Rotation, textureOrigin, scale, SpriteEffects.None, 0.9f);

        }
    }
}

