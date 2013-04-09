using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Content;


namespace WindowsGame2.GameElements
{
    public class RandomTrack
    {

        World world;
        Vertices controlPoints;
        public Vertices curvePointsInternal;
        public Vertices curvePointsExternal;
        public Vertices curvePointsMiddle;
        int controlPointsCount = 20;
        Texture2D dummyTexture;
        Random random;
        public float pathWidth = 10f;
        float initialRadius = 35f;
        Body externalBody;
        Body internalBody;
        List<Vector2> normals;
        List<Vector2> normalsInternal;
        int[] normalsLengths;
        public List<int> internalCorrispondances;
        Texture2D bgTexture;

        float left = 10000000;
        float right = -10000000;
        float up = -10000000;
        float down = 10000000;

        int textureScale = 3;

        public VertexPositionColorTexture[] myArray;
        
        public static RandomTrack createTrack()
        {
            RandomTrack result = new RandomTrack();
            result.buildRandomTrack();
            return result;
        }


        public RandomTrack()
        {
           
            // Define the worls in which the track is going to be drawn
            world = GameServices.GetService<World>();
            controlPoints=new Vertices();
            curvePointsInternal = new Vertices();
            curvePointsExternal = new Vertices();
            curvePointsMiddle = new Vertices();
            internalCorrispondances= new List<int>();
            dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
            random = new Random(DateTime.Now.Millisecond);
            normals = new List<Vector2>();
            normalsInternal = new List<Vector2>();
            ContentManager Content = GameServices.GetService<ContentManager>();
            bgTexture = Content.Load<Texture2D>("Images/bgNew");

            
        }
    
        public void buildRandomTrack(){
            
            float alpha = (2 * (float)Math.PI) / controlPointsCount;
            
            //create initial circle
            for (int i = 0; i < controlPointsCount; i++)
            {

                float randomizedRadius = (float)MathHelper.Lerp(initialRadius/2f, 2 * initialRadius, (float)random.NextDouble()); 
                controlPoints.Add(new Vector2(randomizedRadius * (float)Math.Sin(alpha * i), randomizedRadius * (float)Math.Cos(alpha * i)));
            }

            //create bezier curve
            int timeStep = 3;
            for (int i = 0; i <= controlPointsCount - 4; i++)
            {
                for (int t = timeStep; t <= 100; t = t + timeStep)
                {
                    Vector2 newPoint = Vector2.CatmullRom( controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3], t/100f);
                    curvePointsInternal.Add(newPoint);
                }
            }
            for (int t = timeStep; t <= 100; t = t + timeStep)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 3], controlPoints[controlPointsCount - 2], controlPoints[controlPointsCount - 1], controlPoints[0], t / 100f);
                curvePointsInternal.Add(newPoint);
            }
            for (int t = timeStep; t <= 100; t = t + timeStep)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 2], controlPoints[controlPointsCount - 1], controlPoints[0], controlPoints[1], t / 100f);
                curvePointsInternal.Add(newPoint);
            }
            for (int t = timeStep; t <= 100; t = t + timeStep)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 1], controlPoints[0], controlPoints[1], controlPoints[2], t / 100f);
                curvePointsInternal.Add(newPoint);
            }

            
            
            
            
            //generate external, middle points and normals
            for (int i = 0; i < curvePointsInternal.Count; i++)
            {
                int preIndex = i - 1;
                int postIndex = i + 1;
                if (preIndex < 0)
                {
                    preIndex = curvePointsInternal.Count - 1;
                }
                if (postIndex > curvePointsInternal.Count - 1)
                {
                    postIndex = 0;
                }

                Vector2 tangentVector = curvePointsInternal[postIndex] - curvePointsInternal[preIndex];
                Vector2 normalVector = Vector2.Normalize(new Vector2(-tangentVector.Y, tangentVector.X))*pathWidth;
                normalsInternal.Add(-normalVector);
                Vector2 newPoint=curvePointsInternal[i] + normalVector;
                bool okToAdd=true;
                for (int j = 0; j < curvePointsInternal.Count - 1; j++)
                {
                    //slow like an old kurva!!! 
                    //0.01 margin della vita!!
                    
                    if (Vector2.Distance(newPoint, curvePointsInternal[j]) < pathWidth -0.05f && j!=i)
                    {
                        okToAdd = false;
                        if (j > 0)
                        {
                            
                        }

                    }
                    
                    /*
                    if (Vector2.Distance(newPoint, curvePointsInternal[j]) < pathWidth-0.01f)
                    {
                        okToAdd = false;
                    }
                     */ 
                }
                if (okToAdd)
                {
                    curvePointsExternal.Add(newPoint);
                    normals.Add(normalVector);
                    curvePointsMiddle.Add(curvePointsInternal[i] + (newPoint - curvePointsInternal[i])/2f);
                    internalCorrispondances.Add(i);
                }
            }
            


            externalBody = BodyFactory.CreateLoopShape(world, curvePointsExternal);
            internalBody = BodyFactory.CreateLoopShape(world, curvePointsInternal);

            //compute bounding box in screen coordinates
            
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                if (ConvertUnits.ToDisplayUnits(curvePointsExternal[i].X) < left)
                {
                    left = ConvertUnits.ToDisplayUnits(curvePointsExternal[i].X);
                }
                else if (ConvertUnits.ToDisplayUnits(curvePointsExternal[i].X) > right)
                {
                    right = ConvertUnits.ToDisplayUnits(curvePointsExternal[i].X);
                }
                if (ConvertUnits.ToDisplayUnits(curvePointsExternal[i].Y) < down)
                {
                    down = ConvertUnits.ToDisplayUnits(curvePointsExternal[i].Y);
                }
                else if (ConvertUnits.ToDisplayUnits(curvePointsExternal[i].Y) > up)
                {
                    up = ConvertUnits.ToDisplayUnits(curvePointsExternal[i].Y);
                }
            }
            

            /*
            //generate external points with external controlPoints
            Vertices externalControlPoints = new Vertices();
            List<int> externalControlPointsIndices = new List<int>();
            for (int i = 0; i < curvePointsInternal.Count; i=i+100)
            {
                externalControlPointsIndices.Add(i);
            }
            for (int i = 1; i < externalControlPointsIndices.Count - 1; i++)
            {
                Vector2 tangentVector = curvePointsInternal[externalControlPointsIndices[i] + 1] - curvePointsInternal[externalControlPointsIndices[i] - 1];
                Vector2 normalVector = Vector2.Normalize(new Vector2(-tangentVector.Y, tangentVector.X)) * pathWidth;

                externalControlPoints.Add(curvePointsInternal[externalControlPointsIndices[i]] + normalVector);
            }
            //create bezier curve
            for (int i = 0; i <= externalControlPoints.Count - 4; i++)
            {
                for (int t = 0; t <= 100; t++)
                {
                    Vector2 newPoint = Vector2.CatmullRom(externalControlPoints[i], externalControlPoints[i + 1], externalControlPoints[i + 2], externalControlPoints[i + 3], t / 100f);
                    curvePointsExternal.Add(newPoint);
                }
            }
            for (int t = 0; t <= 100; t++)
            {
                Vector2 newPoint = Vector2.CatmullRom(externalControlPoints[externalControlPoints.Count - 3], externalControlPoints[externalControlPoints.Count - 2], externalControlPoints[externalControlPoints.Count - 1], externalControlPoints[0], t / 100f);
                curvePointsExternal.Add(newPoint);
            }
            for (int t = 0; t <= 100; t++)
            {
                Vector2 newPoint = Vector2.CatmullRom(externalControlPoints[externalControlPoints.Count - 2], externalControlPoints[externalControlPoints.Count - 1], externalControlPoints[0], externalControlPoints[1], t / 100f);
                curvePointsExternal.Add(newPoint);
            }
            for (int t = 0; t <= 100; t++)
            {
                Vector2 newPoint = Vector2.CatmullRom(externalControlPoints[externalControlPoints.Count - 1], externalControlPoints[0], externalControlPoints[1], externalControlPoints[2], t / 100f);
                curvePointsExternal.Add(newPoint);
            }
            */

            myArray = new VertexPositionColorTexture[(curvePointsExternal.Count) * 6 + (curvePointsInternal.Count) * 6];
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                int preIndex = i - 1;
                if (preIndex < 0)
                {
                    preIndex = curvePointsExternal.Count-1;
                }
                myArray[i * 6].Position = new Vector3(curvePointsExternal[preIndex], -0.1f);
                myArray[i * 6 + 1].Position = new Vector3(curvePointsExternal[i], -0.1f);
                myArray[i * 6 + 2].Position = new Vector3(curvePointsExternal[preIndex] + normals[preIndex] / pathWidth, -0.1f);

                myArray[i * 6 + 3].Position = new Vector3(curvePointsExternal[i], -0.1f);
                myArray[i * 6 + 4].Position = new Vector3(curvePointsExternal[preIndex] + normals[preIndex] / pathWidth, -0.1f);
                myArray[i * 6 + 5].Position = new Vector3(curvePointsExternal[i] + normals[i] / pathWidth,-0.1f);

                myArray[i * 6].TextureCoordinate = new Vector2(0, 1);
                myArray[i * 6 + 1].TextureCoordinate = new Vector2(0, 0);
                myArray[i * 6 + 2].TextureCoordinate = new Vector2(1, 1);

                myArray[i * 6 + 3].TextureCoordinate = new Vector2(0, 0);
                myArray[i * 6 + 4].TextureCoordinate = new Vector2(1, 1);
                myArray[i * 6 + 5].TextureCoordinate = new Vector2(1, 0);

             }

            for (int i = 0; i < curvePointsInternal.Count; i++)
            {
                int preIndex = i - 1;
                if (preIndex < 0)
                {
                    preIndex = curvePointsInternal.Count - 1;
                }

                myArray[(i + curvePointsExternal.Count) * 6].Position = new Vector3(curvePointsInternal[preIndex], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 1].Position = new Vector3(curvePointsInternal[i], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 2].Position = new Vector3(curvePointsInternal[preIndex] + normalsInternal[preIndex] / pathWidth, -0.1f);

                myArray[(i + curvePointsExternal.Count) * 6 + 3].Position = new Vector3(curvePointsInternal[i], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 4].Position = new Vector3(curvePointsInternal[preIndex] + normalsInternal[preIndex] / pathWidth, -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 5].Position = new Vector3(curvePointsInternal[i] + normalsInternal[i] / pathWidth, -0.1f);

                myArray[(i + curvePointsExternal.Count) * 6].TextureCoordinate = new Vector2(0, 1);
                myArray[(i + curvePointsExternal.Count) * 6 + 1].TextureCoordinate = new Vector2(0, 0);
                myArray[(i + curvePointsExternal.Count) * 6 + 2].TextureCoordinate = new Vector2(1, 1);

                myArray[(i + curvePointsExternal.Count) * 6 + 3].TextureCoordinate = new Vector2(0, 0);
                myArray[(i + curvePointsExternal.Count) * 6 + 4].TextureCoordinate = new Vector2(1, 1);
                myArray[(i + curvePointsExternal.Count) * 6 + 5].TextureCoordinate = new Vector2(1, 0);

            }

            

            
        }

        public Vertices computeStartingPositions(int index)
        {
            Vertices result = new Vertices();
            result.Add(curvePointsMiddle[index] + normals[index]/ 8f);
            result.Add(curvePointsMiddle[index] + normals[index] / 8f + normals[index] / 4f);
            result.Add(curvePointsMiddle[index] - normals[index] / 8f);
            result.Add(curvePointsMiddle[index] - normals[index] / 8f - normals[index] / 4f);

            return result;
        }

        public float computeStartingAngle(int index)
        {
            Vector2 point1 = curvePointsInternal[internalCorrispondances[index]];
            Vector2 point2 = curvePointsExternal[index];
            float result = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            return result;
        }


        private Vector2 Bezier(int t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var x = OrdinateX((float)t / 100, p0, p1, p2, p3);
            var y = OrdinateY((float)t / 100, p0, p1, p2, p3);

            return new Vector2(x, y);
        }

        private float OrdinateX(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (float)((Math.Pow((double)(1 - t), 3) * p0.Y) + (3 * Math.Pow((double)(1 - t), 2) * t * p1.X) + (3 * (1 - t) * (t * t) * p2.X) + ((t * t * t) * p3.X));
        }

        private float OrdinateY(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (float)((Math.Pow((double)(1 - t), 3) * p0.Y) + (3 * Math.Pow((double)(1 - t), 2) * t * p1.Y) + (3 * (1 - t) * (t * t) * p2.Y) + ((t * t * t) * p3.Y));
        }   



        void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            Vector2 lengthWidth = new Vector2(length,width);
    
            batch.Draw(dummyTexture, point1, null, color, angle, Vector2.Zero, lengthWidth, SpriteEffects.None, 0);
        }

        public void DrawSprites(Camera camera, SpriteBatch spriteBatch)
        {

            //draw background
            for (int y = (int)Math.Ceiling(down); y < (int)Math.Ceiling(up); y = y + bgTexture.Height * textureScale)
            {
                for (int x = (int)Math.Floor(left); x < (int)Math.Ceiling(right); x = x + bgTexture.Width * textureScale)
                {
                    spriteBatch.Draw(bgTexture, new Vector2(x, y), null, Color.White, 0.0f, Vector2.Zero, Vector2.One*textureScale, SpriteEffects.None, 1f);
                }
            }

                

            //draw starting line
            DrawLine(spriteBatch, 100, Color.Yellow, ConvertUnits.ToDisplayUnits(curvePointsInternal[internalCorrispondances[0]]), ConvertUnits.ToDisplayUnits(curvePointsExternal[0]));

            /*
            //draw connections between internal and external points for debugging
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[internalCorrispondances[i]]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
            }
             */


            //// draw track
            //for (int i = 1; i < curvePointsInternal.Count; i++)
            //{
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[i]), ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1]));

            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1]), ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1] + normalsInternal[i - 1] / pathWidth));
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[i]), ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1] + normalsInternal[i - 1] / pathWidth));
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[i] + normalsInternal[i] / pathWidth), ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1] + normalsInternal[i - 1] / pathWidth));
            //}
            //DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[0]), ConvertUnits.ToDisplayUnits(curvePointsInternal[curvePointsInternal.Count - 1]));

            //// why multiply by 5????? random like a drunk kurva!!!
            //for (int i = 1; i < curvePointsExternal.Count; i++)
            //{
            //   // DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]) + normals[i - 1], ConvertUnits.ToDisplayUnits(curvePointsExternal[i]) + normals[i]);
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]) , ConvertUnits.ToDisplayUnits(curvePointsExternal[i]) );

            //    //ugo
               
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]+normals[i-1] / pathWidth));
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[i]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i-1]+normals[i-1]/pathWidth));
            //    DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[i] + normals[i] / pathWidth), ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1] + normals[i - 1] / pathWidth));
            //}
            //// DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[curvePointsExternal.Count - 1]) + normals[curvePointsExternal.Count - 1], ConvertUnits.ToDisplayUnits(curvePointsExternal[0]) + normals[0]);
            //DrawLine(spriteBatch, 5, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsExternal[curvePointsExternal.Count - 1]), ConvertUnits.ToDisplayUnits(curvePointsExternal[0]));
            


            /*
            //draw middle points
            for (int i = 1; i < curvePointsMiddle.Count; i++)
            {
                spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(curvePointsMiddle[i]), null, Color.Green, 0f, Vector2.Zero, new Vector2(20,20), SpriteEffects.None, 0);
            }
             */ 
            
              

            /*
            for (int i = 1; i < curvePointsExternal.Count; i++)
            {
                DrawLine(spriteBatch, 50, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsExternal[i]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]));
            }
            DrawLine(spriteBatch, 50, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsExternal[0]), ConvertUnits.ToDisplayUnits(curvePointsExternal[curvePointsExternal.Count - 1]));
        
             */ 


        }

    
    }
}
