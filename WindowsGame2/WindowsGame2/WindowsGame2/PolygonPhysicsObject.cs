using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics;



namespace WindowsGame2
{
    public class PolygonPhysicsObject
    {
       
        Vector2 origin;
        public Body compound;
        Texture2D texture;
        Rectangle dummyRectangle;
        Transform xf;
        Vector2 newVertex;
        Vector2 lengthWidth;
        Sprite sprite;
        Texture2D _polygonTexture;
        Vector2 _origin;
        PolygonShape shape;
        Random random;

        public Color Color
        {
            get;
            set;
        }

       MaterialType chooseMaterialType(Random random)
        {
            int caseSwitch = random.Next(1, 6);
            MaterialType resultMaterial = MaterialType.Blank;
            switch (caseSwitch)
            {
                case 1:
                    resultMaterial = MaterialType.Blank;
                    break;
                case 2:
                    resultMaterial = MaterialType.Dots;
                    break;
                case 3:
                    resultMaterial = MaterialType.Pavement;
                    break;
                case 4:
                    resultMaterial = MaterialType.Squares;
                    break;
                case 5:
                    resultMaterial = MaterialType.Waves;
                    break;
                default:
                    resultMaterial = MaterialType.Blank;
                    break;
            }

            return resultMaterial;
        }

        public PolygonPhysicsObject(World world, Vertices vertices, Texture2D texture , AssetCreator assetCreator)
        {

            this.texture = texture;
            Vector2 centroid = -vertices.GetCentroid();
            //vertices.Translate(ref origin);
            origin = -centroid;

            // reduce vertices
            vertices = SimplifyTools.ReduceByDistance(vertices, 4f);
            
            // compute comvex shape
            List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
            
            // scale vertices
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * 1f;
            foreach (Vertices verti in list)
            {
                verti.Scale(ref vertScale);
            }

            // create compound
            compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            compound.BodyType = BodyType.Dynamic;

            lengthWidth = new Vector2();
            dummyRectangle = new Rectangle();
            Color = Color.Black;
            
            shape = new PolygonShape(vertices, 1);
            random = new Random();
            MaterialType myMaterial = chooseMaterialType(random);
            sprite = new Sprite(assetCreator.TextureFromShape(shape, myMaterial, Color.White, 1f), AssetCreator.CalculateOrigin(compound));

        }

        void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            lengthWidth.X = length;
            lengthWidth.Y = width;

            batch.Draw(blank, point1, null, color, angle, Vector2.Zero, lengthWidth, SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(sprite.Texture, ConvertUnits.ToDisplayUnits(compound.Position),
                                         null, Color, compound.Rotation, sprite.Origin, 1f, SpriteEffects.None,
                                         0.95f);



            /*
            // iterate fixtures
            for (int j = 0; j < compound.FixtureList.Count; j++)
            {
                
                compound.GetTransform(out xf);

                // iterate vertices
                for (int i = 0; i < ((PolygonShape)compound.FixtureList[j].Shape).Vertices.Count; ++i)
                {
                    // transform them from local to world coordinates
                    newVertex = ConvertUnits.ToDisplayUnits(MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[i]));
                 
                   // draw vertices
                    dummyRectangle.X = (int)newVertex.X;
                    dummyRectangle.Y = (int)newVertex.Y;
                    dummyRectangle.Width = 5;
                    dummyRectangle.Height = 5;
                    spriteBatch.Draw(texture, dummyRectangle, Color);

                    
                    //draw lines
                    //if (i > 0 && i < ((PolygonShape)compound.FixtureList[j].Shape).Vertices.Count)
                    //{
                    //    Vector2 position1 = _tempVertices[i];
                    //    Vector2 position2 = _tempVertices[i - 1];
                    //    DrawLine(spriteBatch, texture, 5, Color.Red, position1, position2);
                    //}
                     

                }
            }

            */

           
            
        }

        

    }
}




