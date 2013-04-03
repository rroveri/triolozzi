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


namespace WindowsGame2.GameElements
{
    class RandomTrack
    {

        World world;
        Vertices controlPoints;
        Vertices curvePointsInternal;
        Vertices curvePointsExternal;
        Vertices curvePointsMiddle;
        int controlPointsCount = 20;
        Texture2D dummyTexture;
        Random random;
        float pathWidth = 10f;
        float initialRadius = 35f;
        Body externalBody;
        Body internalBody;
        List<Vector2> normals;
        
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
            dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
            random = new Random();
            normals = new List<Vector2>();
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
            for (int i = 0; i <= controlPointsCount - 4; i++)
            {
                for (int t = 10; t <= 100; t=t+10)
                {
                    Vector2 newPoint = Vector2.CatmullRom( controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3], t/100f);
                    curvePointsInternal.Add(newPoint);
                }
            }
            for (int t = 10; t <= 100; t = t + 10)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 3], controlPoints[controlPointsCount - 2], controlPoints[controlPointsCount - 1], controlPoints[0], t / 100f);
                curvePointsInternal.Add(newPoint);
            }
            for (int t = 10; t <= 100; t = t + 10)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 2], controlPoints[controlPointsCount - 1], controlPoints[0], controlPoints[1], t / 100f);
                curvePointsInternal.Add(newPoint);
            }
            for (int t = 10; t <= 100; t = t + 10)
            {
                Vector2 newPoint = Vector2.CatmullRom(controlPoints[controlPointsCount - 1], controlPoints[0], controlPoints[1], controlPoints[2], t / 100f);
                curvePointsInternal.Add(newPoint);
            }

            
            //generate external, middle points and normals
            for (int i = 1; i < curvePointsInternal.Count-1; i++)
            {
                Vector2 tangentVector = curvePointsInternal[i + 1] - curvePointsInternal[i - 1];
                Vector2 normalVector = Vector2.Normalize(new Vector2(-tangentVector.Y, tangentVector.X))*pathWidth;
                Vector2 newPoint=curvePointsInternal[i] + normalVector;
                bool okToAdd=true;
                for (int j = 0; j < curvePointsInternal.Count - 1; j++)
                {
                    //slow like an old kurva!!!
                    if (Vector2.Distance(newPoint, curvePointsInternal[j]) < pathWidth)
                    {
                        okToAdd = false;
                    }
                }
                if (okToAdd)
                {
                    curvePointsExternal.Add(newPoint);
                    normals.Add(normalVector);
                    curvePointsMiddle.Add(curvePointsInternal[i] + (newPoint - curvePointsInternal[i])/2f);
                }
            }

            

            externalBody = BodyFactory.CreateLoopShape(world, curvePointsExternal);
            internalBody = BodyFactory.CreateLoopShape(world, curvePointsInternal);
            

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
            Vector2 point1 = curvePointsInternal[index];
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

            //draw starting line
            DrawLine(spriteBatch, 100, Color.Yellow, ConvertUnits.ToDisplayUnits(curvePointsInternal[0]), ConvertUnits.ToDisplayUnits(curvePointsExternal[0]));
          //spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(curvePointsMiddle[0]), null, Color.Yellow, 0.0f, Vector2.Zero, new Vector2(100, squaredBg.Height * bgScale), SpriteEffects.None, 1f);


            // draw track
            for (int i = 1; i < curvePointsInternal.Count; i++)
            {
                DrawLine(spriteBatch, 50, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[i]), ConvertUnits.ToDisplayUnits(curvePointsInternal[i - 1]));
            }
            DrawLine(spriteBatch, 50, Color.Black, ConvertUnits.ToDisplayUnits(curvePointsInternal[0]), ConvertUnits.ToDisplayUnits(curvePointsInternal[curvePointsInternal.Count - 1]));

            // why multiply by 5????? random like a drunk kurva!!!
            for (int i = 1; i < curvePointsExternal.Count; i++)
            {
                DrawLine(spriteBatch, 50, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsExternal[i]) + normals[i] * 5, ConvertUnits.ToDisplayUnits(curvePointsExternal[i - 1]) + normals[i - 1] * 5);
            }
            DrawLine(spriteBatch, 50, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsExternal[0]) + normals[0] * 5, ConvertUnits.ToDisplayUnits(curvePointsExternal[curvePointsExternal.Count - 1]) + normals[curvePointsExternal.Count - 1] * 5);

            for (int i = 1; i < curvePointsMiddle.Count; i++)
            {
                spriteBatch.Draw(dummyTexture, ConvertUnits.ToDisplayUnits(curvePointsMiddle[i]), null, Color.Green, 0f, Vector2.Zero, new Vector2(20,20), SpriteEffects.None, 0);
            }

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
