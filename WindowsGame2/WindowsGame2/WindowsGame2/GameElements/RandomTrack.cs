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
        Texture2D bgTextureEasterEgg;
        Texture2D currentTexture;
        int currentTextureIndex;

        float left = 10000000;
        float right = -10000000;
        float up = -10000000;
        float down = 10000000;

        int textureScale = 3;

        public VertexPositionColorTexture[] myArray;

        public List<Texture2D> texturesArray;
        public List<Texture2D> dreamsArray;
        public List<Vector2> dreamsPositions;
        public List<float> dreamsRotations;
        public List<Texture2D> dreamsArrayFigures;
        public List<Color> dreamsArrayColors;
        public List<bool> dreamsArrayState;
        public List<Texture2D> quotesArray;
        public List<Vector2> quotesPositions;
        public List<int> carsNumber;

        public List<float> internalNormalsCoefficients;
        public List<float> externalNormalsCoefficients;

        private Vector2 postItSize;

        public GameLogic gameLogic;

        List<TexturePhysicsObject> postItBodiesList;
        Vector2 textureScaleVec;

        List<int> dreamsMiddlePoints;
        
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
            bgTextureEasterEgg = Content.Load<Texture2D>("Images/bgNew2");
            currentTexture = bgTexture;
            currentTextureIndex = 0;

            texturesArray = new List<Texture2D>();
            texturesArray.Add(bgTexture);          
            texturesArray.Add(Content.Load<Texture2D>("Images/ColinComm2"));
            texturesArray.Add(Content.Load<Texture2D>("Images/asd"));
            texturesArray.Add(Content.Load<Texture2D>("Images/BlackNight_o_o"));
            texturesArray.Add(Content.Load<Texture2D>("Images/cina"));
            texturesArray.Add(Content.Load<Texture2D>("Images/q"));
            texturesArray.Add(bgTextureEasterEgg);

            dreamsArray = new List<Texture2D>();
            dreamsArrayFigures = new List<Texture2D>();
            dreamsArrayColors = new List<Color>();
            dreamsArrayState=new List<bool>();

            Texture2D postItTex = Content.Load<Texture2D>("Images/Dreams/postitFree");
            Texture2D postItTexWish = Content.Load<Texture2D>("Images/Dreams/postitwish");
            Texture2D postItTexNightmare = Content.Load<Texture2D>("Images/Dreams/postitnightmare");

            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Wishes/Ali_Resized"));
            dreamsArray.Add(postItTexWish);
            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Wishes/Cupcake_Resized"));
            dreamsArray.Add(postItTexWish);
            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Wishes/Pony_Resized"));
            dreamsArray.Add(postItTexWish);
            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Nightmares/Diavolo_Resized"));
            dreamsArray.Add(postItTexNightmare);
            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Nightmares/Fantasma_Resized"));
            dreamsArray.Add(postItTexNightmare);
            dreamsArrayFigures.Add(Content.Load<Texture2D>("Images/Dreams/Nightmares/Frankenstein_Resized"));
            dreamsArray.Add(postItTexNightmare);

            quotesArray=new List<Texture2D>();
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/buyBeers"));
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/buyMilk"));
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/callLuigi"));
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/mamaBDay"));
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/marketingMeeting"));
            quotesArray.Add(Content.Load<Texture2D>("Images/Dreams/Quotes/todolist"));
            quotesPositions = new List<Vector2>();
            
            postItSize = new Vector2(500f,500f);
            textureScaleVec = new Vector2(postItSize.X / (float)postItTex.Width, postItSize.Y / (float)postItTex.Height);
            postItBodiesList = new List<TexturePhysicsObject>();

            

            dreamsPositions = new List<Vector2>();
            dreamsRotations = new List<float>();
            dreamsMiddlePoints = new List<int>();

            internalNormalsCoefficients = new List<float>();
            externalNormalsCoefficients = new List<float>();

            carsNumber = new List<int>();

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
            int timeStep = 1;
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

            bool hasToRemoveLast = false;
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

                float randomizedRadius = (float)MathHelper.Lerp(0f, tangentVector.Length(), (float)random.NextDouble());
                internalNormalsCoefficients.Add(Math.Min( tangentVector.Length() * tangentVector.Length(), pathWidth/2f));


                bool okToAdd=true;
                bool notAdded = false;
                
                for (int j = 0; j < curvePointsInternal.Count - 1; j++)
                {
                    

                    //slow like an old kurva!!! 
                    //0.01 margin della vita!!
                    
                    if (Vector2.Distance(newPoint, curvePointsInternal[j]) < pathWidth  && j!=i)
                    {
                        okToAdd = false;
                       
                    }
 
                }
                if (okToAdd)
                {
                    if (notAdded)
                    {
                        notAdded = false;
                        if (curvePointsExternal.Count > 0)
                        {
                            curvePointsExternal.RemoveAt(curvePointsExternal.Count - 1);
                            normals.RemoveAt(curvePointsExternal.Count - 1);
                            curvePointsMiddle.RemoveAt(curvePointsExternal.Count - 1);
                            internalCorrispondances.RemoveAt(curvePointsExternal.Count - 1);

                        }
                        else {
                            hasToRemoveLast = true;
                        }
                    }
                    else
                    {
                        curvePointsExternal.Add(newPoint);
                        normals.Add(normalVector);
                        curvePointsMiddle.Add(curvePointsInternal[i] + (newPoint - curvePointsInternal[i]) / 2f);
                        internalCorrispondances.Add(i);

                    }
                }
                else
                {
                    notAdded = true;
                }
                
            }

            if (hasToRemoveLast)
            {
                curvePointsExternal.RemoveAt(curvePointsExternal.Count - 1);
                normals.RemoveAt(curvePointsExternal.Count - 1);
                curvePointsMiddle.RemoveAt(curvePointsExternal.Count - 1);
                internalCorrispondances.RemoveAt(curvePointsExternal.Count - 1);
            }
            


            externalBody = BodyFactory.CreateLoopShape(world, curvePointsExternal);
            internalBody = BodyFactory.CreateLoopShape(world, curvePointsInternal);

            externalBody.CollisionGroup = -1;
            internalBody.CollisionGroup = -1;


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

            //create external normal coefficients, and interpolate the first ones to get a smooth ink width at the start
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                externalNormalsCoefficients.Add(internalNormalsCoefficients[i]);
            }
            int startingVertices=10;
            for (int i = 0; i < startingVertices; i++)
            {
                float lastCoeff = externalNormalsCoefficients[externalNormalsCoefficients.Count-1];
                float targetCoeff=externalNormalsCoefficients[startingVertices-1];
                externalNormalsCoefficients[i] = MathHelper.Lerp(lastCoeff, targetCoeff, 1f / startingVertices * i);
            }

            //create normals for drawing ink
            List<Vector2> normalExternalInk = new List<Vector2>();
            float inkWidth = 5f;
            for (int j = 0; j < curvePointsExternal.Count; j++)
            {
                normalExternalInk.Add(Vector2.Normalize(curvePointsExternal[j]) * pathWidth * inkWidth);
            }

            List<Vector2> normalInternalInk = new List<Vector2>();
            for (int j = 0; j < curvePointsInternal.Count; j++)
            {
                normalInternalInk.Add(-Vector2.Normalize(curvePointsInternal[j]) * pathWidth * inkWidth);
            }


                //set borders triangles for shaders
                //external borders
                myArray = new VertexPositionColorTexture[(curvePointsExternal.Count) * 6 + (curvePointsInternal.Count) * 6];
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                int preIndex = i - 1;
                if (preIndex < 0)
                {
                    preIndex = curvePointsExternal.Count-1;
                }

                //TO NOTICE: i am NOT taking the corrispondent external point for the externalNormalsCoefficent on purpose (i should use externalNormalsCoefficients[internalCorrispondences[preIndex]] and so on)!!
                //I left it like this in order to have asymmetry between the two borders!!!!!

                myArray[i * 6].Position = new Vector3(curvePointsExternal[preIndex], -0.1f);
                myArray[i * 6 + 1].Position = new Vector3(curvePointsExternal[i], -0.1f);
                myArray[i * 6 + 2].Position = new Vector3(curvePointsExternal[preIndex] + normalExternalInk[preIndex] / pathWidth * externalNormalsCoefficients[preIndex], -0.1f);

                myArray[i * 6 + 3].Position = new Vector3(curvePointsExternal[i], -0.1f);
                myArray[i * 6 + 4].Position = new Vector3(curvePointsExternal[preIndex] + normalExternalInk[preIndex] / pathWidth * externalNormalsCoefficients[preIndex], -0.1f);
                myArray[i * 6 + 5].Position = new Vector3(curvePointsExternal[i] + normalExternalInk[i] / pathWidth * externalNormalsCoefficients[i], -0.1f);

                myArray[i * 6].TextureCoordinate = new Vector2(0, 1);
                myArray[i * 6 + 1].TextureCoordinate = new Vector2(0, 0);
                myArray[i * 6 + 2].TextureCoordinate = new Vector2(1, 1);

                myArray[i * 6 + 3].TextureCoordinate = new Vector2(0, 0);
                myArray[i * 6 + 4].TextureCoordinate = new Vector2(1, 1);
                myArray[i * 6 + 5].TextureCoordinate = new Vector2(1, 0);

             }

            //internal borders
            for (int i = 0; i < curvePointsInternal.Count; i++)
            {
                int preIndex = i - 1;
                if (preIndex < 0)
                {
                    preIndex = curvePointsInternal.Count - 1;
                }

                myArray[(i + curvePointsExternal.Count) * 6].Position = new Vector3(curvePointsInternal[preIndex], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 1].Position = new Vector3(curvePointsInternal[i], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 2].Position = new Vector3(curvePointsInternal[preIndex] + normalInternalInk[preIndex] / pathWidth * internalNormalsCoefficients[preIndex], -0.1f);

                myArray[(i + curvePointsExternal.Count) * 6 + 3].Position = new Vector3(curvePointsInternal[i], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 4].Position = new Vector3(curvePointsInternal[preIndex] + normalInternalInk[preIndex] / pathWidth * internalNormalsCoefficients[preIndex], -0.1f);
                myArray[(i + curvePointsExternal.Count) * 6 + 5].Position = new Vector3(curvePointsInternal[i] + normalInternalInk[i] / pathWidth * internalNormalsCoefficients[i], -0.1f);

                myArray[(i + curvePointsExternal.Count) * 6].TextureCoordinate = new Vector2(0, 1);
                myArray[(i + curvePointsExternal.Count) * 6 + 1].TextureCoordinate = new Vector2(0, 0);
                myArray[(i + curvePointsExternal.Count) * 6 + 2].TextureCoordinate = new Vector2(1, 1);

                myArray[(i + curvePointsExternal.Count) * 6 + 3].TextureCoordinate = new Vector2(0, 0);
                myArray[(i + curvePointsExternal.Count) * 6 + 4].TextureCoordinate = new Vector2(1, 1);
                myArray[(i + curvePointsExternal.Count) * 6 + 5].TextureCoordinate = new Vector2(1, 0);

            }

            
            //put random dreams
            for (int i = 0; i < dreamsArray.Count; i++)
            {
                int randomMiddlePoint= computeRandomDreamMiddlePoint();
                dreamsMiddlePoints.Add(randomMiddlePoint);
                float randomOffset = (float)MathHelper.Lerp(-0.30f, 0.30f, (float)random.NextDouble());
                Vector2 finalPos = curvePointsMiddle[randomMiddlePoint] - normals[randomMiddlePoint] * randomOffset;
                dreamsPositions.Add(finalPos);

                float randomAngle = (float)MathHelper.Lerp(-MathHelper.Pi / 4f, MathHelper.Pi / 4f, (float)random.NextDouble());
                dreamsRotations.Add(randomAngle);

                TexturePhysicsObject newBody = new TexturePhysicsObject(world, GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Dreams/postitContour"), postItSize, Color.White);
                newBody._compound.Position = finalPos;
                newBody._compound.Rotation = randomAngle;
                newBody._compound.UserData = i;

                postItBodiesList.Add(newBody);
                newBody._compound.IsSensor = true;

                dreamsArrayColors.Add(Color.White);
                dreamsArrayState.Add(true);

                carsNumber.Add(0);
            }

            //put random quotes
            for (int i = 0; i < quotesArray.Count; i++)
            {
                int randomMiddlePoint = computeRandomDreamMiddlePoint();
                float randomOffset = (float)MathHelper.Lerp(-0.30f, 0.30f, (float)random.NextDouble());
                Vector2 finalPos = curvePointsMiddle[randomMiddlePoint] - normals[randomMiddlePoint] * randomOffset;
                quotesPositions.Add(finalPos);
            }
        }

        public void changePostItColor(int index, Car car)
        {
            //color the post it and update car score
            dreamsArrayColors[index] = car.mColor;
            if (index < 3)
            {
                gameLogic.UpdateScore(car, 0);
            }
            else
            {
                gameLogic.UpdateScore(car, 1);
            }
        }

        public int computeRandomDreamMiddlePoint()
        {
            int randomMiddlePoint = random.Next(20, curvePointsMiddle.Count - 20);
            for (int i = 0; i < dreamsMiddlePoints.Count; i++)
            {
                if (Math.Abs(randomMiddlePoint - dreamsMiddlePoints[i]) < 40)
                {
                    return computeRandomDreamMiddlePoint();
                }
            }
            return randomMiddlePoint;
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

        public void swapTexture(){

            currentTexture =texturesArray[currentTextureIndex];
            currentTextureIndex++;
            if (currentTextureIndex > texturesArray.Count - 1)
            {
                currentTextureIndex = 0;
            }
            
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
            for (int y = (int)Math.Ceiling(down); y < (int)Math.Ceiling(up); y = y + currentTexture.Height * textureScale)
            {
                for (int x = (int)Math.Floor(left); x < (int)Math.Ceiling(right); x = x + currentTexture.Width * textureScale)
                {
                    spriteBatch.Draw(currentTexture, new Vector2(x, y), null, Color.White, 0.0f, Vector2.Zero, Vector2.One * textureScale, SpriteEffects.None, 1f);
                }
            }

            //draw starting line
            DrawLine(spriteBatch, 100, Color.Yellow, ConvertUnits.ToDisplayUnits(curvePointsInternal[internalCorrispondances[0]]), ConvertUnits.ToDisplayUnits(curvePointsExternal[0]));

            //draw quotes (it uses post its rotations and origins...disgusting like a rotten kurva)
            for (int i = 0; i < quotesArray.Count; i++)
            {
                spriteBatch.Draw(quotesArray[i], ConvertUnits.ToDisplayUnits(quotesPositions[i]), null, Color.Yellow, dreamsRotations[(i + 3) % quotesArray.Count], postItBodiesList[(i + 3) % quotesArray.Count]._origin, textureScaleVec, SpriteEffects.None, 1f);
            }

            //draw dreams
            for (int i = 0; i < dreamsArray.Count; i++)
            {
                spriteBatch.Draw(dreamsArray[i], ConvertUnits.ToDisplayUnits(dreamsPositions[i]), null, Color.Yellow, dreamsRotations[i], postItBodiesList[i]._origin, textureScaleVec, SpriteEffects.None, 1f);
                spriteBatch.Draw(dreamsArrayFigures[i], ConvertUnits.ToDisplayUnits(dreamsPositions[i]), null, dreamsArrayColors[i], dreamsRotations[i], postItBodiesList[i]._origin, textureScaleVec, SpriteEffects.None, 1f);
            }


            

            /*
            //draw triangulation of the track for debugging 
            for (int i = 0; i < curvePointsExternal.Count; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex > curvePointsExternal.Count-1)
                {
                    nextIndex = 0;
                }

                int a=internalCorrispondances[i];
                int b=internalCorrispondances[nextIndex];
                if (b - a == 1 )
                {
                    //for each "rectangle" there are two triangles
                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[a]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsInternal[a]));

                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsExternal[nextIndex]));
                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                    DrawLine(spriteBatch, 5, Color.Red, ConvertUnits.ToDisplayUnits(curvePointsExternal[nextIndex]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                }
                else
                {
                   for (int j = a; j < b; j++)
                   {
                       DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsInternal[j]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                       DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsInternal[j]), ConvertUnits.ToDisplayUnits(curvePointsInternal[j+1]));
                       DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsInternal[j+1]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                   }
                   DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                   DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsInternal[b]), ConvertUnits.ToDisplayUnits(curvePointsExternal[nextIndex]));
                   DrawLine(spriteBatch, 5, Color.Green, ConvertUnits.ToDisplayUnits(curvePointsExternal[nextIndex]), ConvertUnits.ToDisplayUnits(curvePointsExternal[i]));
                }
            }
             *
             */
            
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
