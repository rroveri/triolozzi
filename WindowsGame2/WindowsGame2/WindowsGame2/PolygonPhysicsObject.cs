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
            set;
        }

        public Body currentIgnoredBody;

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

            //ROBA NUOVA PER EVITARE L'ASSERT s>0 e area big enough IN FARSEER...MAGARI NON VA PIU UNA SEGA!!! -- inizio
            for (int i = 0; i < list.Count; i++)
            {
                if (!checkFarseerAssertSBiggerThanZero(list[i]) || !checkFarseerAssertAreaNotTooSmall(list[i]))
                {
                    //list.RemoveAt(i);
                    //don't create polygon
                    canCreate = false;
                }
            }
            //ROBA NUOVA PER EVITARE L'ASSERT s>0 IN FARSEER e area big enough...MAGARI NON VA PIU UNA SEGA!!! -- fine

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

        bool checkFarseerAssertSBiggerThanZero(Vertices vertices)
        {
            // Ensure the polygon is convex and the interior
            // is to the left of each edge.
            bool sOk = true;

            for (int i = 0; i < vertices.Count; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < vertices.Count ? i + 1 : 0;
                Vector2 edge = vertices[i2] - vertices[i1];

                for (int j = 0; j < vertices.Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                    {
                        continue;
                    }

                    Vector2 r = vertices[j] - vertices[i1];

                    // Your polygon is non-convex (it has an indentation) or
                    // has colinear edges.
                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0)
                    {
                        sOk = false;
                        break;
                    }
                    
                }
            }

            return sOk;
        }

        bool checkFarseerAssertAreaNotTooSmall(Vertices vertices)
        {
            float area = 0.0f;
            Vector2 pRef = Vector2.Zero;

            for (int i = 0; i < vertices.Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = vertices[i];
                Vector2 p3 = i + 1 < vertices.Count ? vertices[i + 1] : vertices[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float d;
                MathUtils.Cross(ref e1, ref e2, out d);

                float triangleArea = 0.5f * d;
                area += triangleArea;
            }

            if (area > Settings.Epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            lengthWidth.X = length;
            lengthWidth.Y = width;
        }


        public void Draw( ref Matrix projection, ref Matrix view, Matrix transform, ref VertexPositionColorTexture[] basicVert,ref int counter, int maxTriangles, ref int loop)
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

                    if (counter == Math.Round(maxTriangles/3f))
                    {
                        counter = 1;
                        loop =(int) Math.Round((maxTriangles-1) / 3f);
                    }

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




