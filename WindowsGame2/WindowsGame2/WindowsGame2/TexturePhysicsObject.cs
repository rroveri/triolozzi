using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics;

namespace WindowsGame2
{
    class TexturePhysicsObject
    {
        public Body _compound;
        private Vector2 _origin;
        private Texture2D _polygonTexture;
        private Vector2 textureScale;
        private Color color;

        public Vector2 Position
        {
            get { return ConvertUnits.ToDisplayUnits(_compound.Position); }
            set { _compound.Position = value * ConvertUnits.ToSimUnits(1);}
        }


        public TexturePhysicsObject(World world, Texture2D texture, Vector2 size, Color color)
        {
            this.textureScale = new Vector2(size.X / (float)texture.Width, size.Y / (float)texture.Height);
            this.color = color;

            //load texture that will represent the physics body
            _polygonTexture = texture;

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 60f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);


            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * textureScale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            _compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _compound.BodyType = BodyType.Dynamic;

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(_polygonTexture, ConvertUnits.ToDisplayUnits(_compound.Position),
                                           null, color, _compound.Rotation, _origin, textureScale, SpriteEffects.None,
                                           0.9f);

           // Console.WriteLine(ConvertUnits.ToDisplayUnits(_compound.Position));

        }
    }
}
