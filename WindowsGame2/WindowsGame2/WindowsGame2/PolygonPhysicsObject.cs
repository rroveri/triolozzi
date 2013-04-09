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
using System.Diagnostics;


namespace WindowsGame2
{
    public class PolygonPhysicsObject
    {
        public Body compound;
        Transform xf;

        Vector2 vertex1;
        Vector2 vertex2;
        Vector2 vertex3;

        Vector2 lengthWidth;

        public bool IsValid
        {
            get;
            private set;
        }

        public Color Color { get; set; }

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

        public PolygonPhysicsObject(World world, Vertices vertices)
        {
            // reduce vertices
            vertices = SimplifyTools.ReduceByDistance(vertices, 4f);
            vertices = SimplifyTools.CollinearSimplify(vertices);

            if (vertices.Count < 2)
            {
                IsValid = false;
                return;
            }
           // Console.WriteLine(vertices.Count);

            IsValid = true;
            // compute comvex shape
            List<Vertices> list;
            try
            {
                list = EarclipDecomposer.ConvexPartition(vertices);
            }
            catch (Exception)
            {
                // Non fare un cip
                IsValid = false;
                return;
            }

            // scale vertices
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * 1f;
            foreach (Vertices verti in list)
            {
                verti.Scale(ref vertScale);
            }

            bool canCreate = true;

            // create compound
            if (canCreate)
            {
                compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);

                if (compound.Mass < 1f)
                {
                    compound.Enabled = false;
                   
                    IsValid = false;
                }
                else
                {
                    compound.BodyType = BodyType.Dynamic;
                    compound.CollisionGroup = -1;
                }
            }
            else
            {
                IsValid = false;
            }
        }

        void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            lengthWidth.X = length;
            lengthWidth.Y = width;
        }


        public void Draw( ref Matrix projection, ref Matrix view, Matrix transform, ref VertexPositionColorTexture[] basicVert,ref int counter)
        {
            compound.GetTransform(out xf);
            // iterate fixtures
            for (int j = 0; j < compound.FixtureList.Count; j++)
            {
                // iterate vertices
                if (compound.FixtureList[j].ShapeType != ShapeType.Polygon)
                {
                    continue;
                }
                for (int i = 1; i < ((PolygonShape)compound.FixtureList[j].Shape).Vertices.Count; ++i)
                {
                    counter++;
                    // transform them from local to world coordinates
                    vertex1 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[i]);
                    vertex2 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[i - 1]);
                    vertex3 = MathUtils.Multiply(ref xf, ((PolygonShape)compound.FixtureList[j].Shape).Vertices[0]);
                        
                    Vector3 v1 = new Vector3(vertex1, -0.1f), v2 = new Vector3(vertex2, -0.1f), v3 = new Vector3(vertex3, -0.1f);

                    //set shader values
                    basicVert[(counter - 1) * 3].Position = v3;
                    basicVert[(counter - 1) * 3].Color = Color;
                    if (basicVert[(counter - 1) * 3].TextureCoordinate.X == -1)
                    {
                        basicVert[(counter - 1) * 3].TextureCoordinate.X = v3.X;
                        basicVert[(counter - 1) * 3].TextureCoordinate.Y = v3.Y;
                    }

                    basicVert[(counter - 1) * 3 + 1].Position = v1;
                    basicVert[(counter - 1) * 3 + 1].Color = Color;
                    if (basicVert[(counter - 1) * 3 + 1].TextureCoordinate.X == -1)
                    {
                        basicVert[(counter - 1) * 3 + 1].TextureCoordinate.X = v1.X;
                        basicVert[(counter - 1) * 3 + 1].TextureCoordinate.Y = v1.Y;
                    }

                    basicVert[(counter - 1) * 3 + 2].Position = v2;
                    basicVert[(counter - 1) * 3 + 2].Color = Color;
                    if (basicVert[(counter - 1) * 3 + 2].TextureCoordinate.X == -1)
                    {
                        basicVert[(counter - 1) * 3 + 2].TextureCoordinate.X = v2.X;
                        basicVert[(counter - 1) * 3 + 2].TextureCoordinate.Y = v2.Y;
                    }
                }
            }
        }
    }
}




