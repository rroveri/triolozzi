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
using FarseerPhysics.DebugViews;



namespace WindowsGame2
{
    public class PolygonPhysicsObject 
    {

        Vector2 origin;
        public Body compound;
        Texture2D texture;
        Rectangle dummyRectangle;
        Transform xf;

        Vector2 vertex1;
        Vector2 vertex2;
        Vector2 vertex3;

        Vector2 lengthWidth;
        Sprite sprite;
        Texture2D _polygonTexture;
        Vector2 _origin;
        PolygonShape shape;
        Random random;
        Vertices _vertices;

        private VertexPositionColor[] verts;
        BasicEffect _colorShader;

        GraphicsDevice _graphicsDevice;
        



        public bool IsValid
        {
            get;
            private set;
        }


        public Color Color
        {
            get;
            set;
        }

        MaterialType chooseMaterialType(Random random)
        {
            int caseSwitch = random.Next(1, 6);
            caseSwitch = 1;
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

        public PolygonPhysicsObject(World world, Vertices vertices, Texture2D texture, AssetCreator assetCreator, GraphicsDevice graphicsDevice, BasicEffect polygonsColorShader)
        {
            this._graphicsDevice = graphicsDevice;
   

            // set up a new basic effect, and enable vertex colors.
            _colorShader = polygonsColorShader;


            this._vertices = vertices;
            this.texture = texture;
            Vector2 centroid = -vertices.GetCentroid();
            //vertices.Translate(ref origin);
            origin = -centroid;

            // reduce vertices
            vertices = SimplifyTools.ReduceByDistance(vertices, 4f);

            if (vertices.Count < 2)
            {
                IsValid = false;
                return;
            }

            IsValid = true;
            // compute comvex shape

            List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
            //List<Vertices> list = BayazitDecomposer.ConvexPartition(vertices);

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

            //generate texture (SLOW!)
            //shape = new PolygonShape(vertices, 1);
            //random = new Random();
            //MaterialType myMaterial = chooseMaterialType(random);
            //sprite = new Sprite(assetCreator.TextureFromShape(shape, MaterialType.Blank, Color.White, 1f), AssetCreator.CalculateOrigin(compound));

            //shader
            verts = new VertexPositionColor[3];

            
        }

        void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            lengthWidth.X = length;
            lengthWidth.Y = width;

           // batch.Draw(blank, point1, null, color, angle, Vector2.Zero, lengthWidth, SpriteEffects.None, 0);
           
        }

        public void Draw(SpriteBatch spriteBatch, ref Matrix projection, ref Matrix view, Matrix transform)
        {

            //draw texture
            //spriteBatch.Draw(sprite.Texture, ConvertUnits.ToDisplayUnits(compound.Position),
            //                             null, Color, compound.Rotation, sprite.Origin, 1f, SpriteEffects.None,
            //                             0.95f);


            // iterate fixtures
            for (int j = 0; j < compound.FixtureList.Count; j++)
            {

                compound.GetTransform(out xf);

                // iterate vertices
                if (compound.FixtureList[j].ShapeType != ShapeType.Polygon)
                {
                    continue;
                }
                for (int i = 1; i < ((PolygonShape)compound.FixtureList[j].Shape).Vertices.Count; ++i)
                {
                    // transform them from local to world coordinates
                    vertex1 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[i]);
                    vertex2 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[i-1]);
                    vertex3 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[0]);

                    // draw vertices
                    //dummyRectangle.X = (int)newVertex.X;
                    // dummyRectangle.Y = (int)newVertex.Y;
                    // dummyRectangle.Width = 15;
                    //dummyRectangle.Height = 15;
                    //spriteBatch.Draw(texture, dummyRectangle, Color);

                    //draw triangles borders, cannot used in the same time with shaders! If not used, spriteBatch can be removed from the arguments
                    DrawLine(spriteBatch, texture, 5, Color, ConvertUnits.ToDisplayUnits(vertex1), ConvertUnits.ToDisplayUnits(vertex2));
                    DrawLine(spriteBatch, texture, 5, Color, ConvertUnits.ToDisplayUnits(vertex1), ConvertUnits.ToDisplayUnits(vertex3));

                    //set shader values
                    verts[0].Position = new Vector3(vertex1, -0.1f);
                    verts[0].Color = Color;
                    verts[1].Position = new Vector3(vertex2, -0.1f);
                    verts[1].Color = Color;
                    verts[2].Position = new Vector3(vertex3, -0.1f);
                    verts[2].Color = Color;

                    _colorShader.Projection = projection;
                    _colorShader.View = view;
                    _colorShader.CurrentTechnique.Passes[0].Apply();
          
                    //draw shader
                    _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 1);
                    
                    

                    /*
                    //draw random lines
                    if (i > 0 && i < ((PolygonShape)compound.FixtureList[j].Shape).Vertices.Count)
                    {
                        Vector2 position1 = _tempVertices[i];
                        Vector2 position2 = _tempVertices[i - 1];
                        DrawLine(spriteBatch, texture, 5, Color.Red, position1, position2);
                    }
                     */


                }
            }


        }



    }
}




