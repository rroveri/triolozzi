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
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.DebugViews;
using WindowsGame2.GameElements;

namespace WindowsGame2.Screens
{
    class GameScreen : AbstractScreen
    {

        #region Fields

        DebugViewXNA _debugView;
        World world;
        GraphicsDevice GraphicsDevice;
        ContentManager Content;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Car> cars;
        List<PlayerIndex> playerIndexes;
        List<PolygonPhysicsObject> polygonsList;
        VertexPositionColorTexture[][] trails = new VertexPositionColorTexture[4][];

        KeyboardState prevKeyboardState;
        Random random;

        Car redCar;
        Car blueCar;
        Car greenCar;
        Car yellowCar;

        KeyboardState ks;
        GamePadState gps;

        AssetCreator assetCreator;

        Camera cameraTopLeft;
        Camera cameraTopRight;
        Camera cameraBottomLeft;
        Camera cameraFollowing;

        Viewport topLeftViewport;
        Viewport topRightViewport;
        Viewport bottomLeftViewport;
        Viewport defaultViewport;

        Matrix projectionMatrix;
        Matrix halfprojectionMatrix;

        Track raceTrack;
        RandomTrack randomRaceTrack;

        PauseMenuScreen PauseScreen;
        float pauseAlpha;

        Effect polygonsColorShader;
        Matrix projection;
        Matrix view;

        VertexPositionColorTexture[] basicVert;
        short[] triangleListIndices;
        int maxNumberOfTriangles = 10000;
        private int mGameMode;

        private int playersNumber;
        Vertices startingPos;
        int[] ranking;
        int[] taken;
        int[] orderToExit;
        private int currentExitIndex;

        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public GameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            PauseScreen = new PauseMenuScreen();

            polygonsList = new List<PolygonPhysicsObject>();
            cars = new List<Car>();
            playerIndexes = new List<PlayerIndex>();
            playerIndexes.Add(PlayerIndex.One); playerIndexes.Add(PlayerIndex.Two);
            playerIndexes.Add(PlayerIndex.Three); playerIndexes.Add(PlayerIndex.Four);
            random = new Random();

            playersNumber = 4;
            ranking = new int[playersNumber];
            taken = new int[playersNumber];
            orderToExit = new int[playersNumber-1];
            startingPos = new Vertices();
            currentExitIndex = 0;
        }

        public override void LoadContent()
        {
            // We add to the GameServices objects that we want to be able to use accross different classes
            // without having to pass them explicitly every time.
            GraphicsDevice = GameServices.GetService<GraphicsDevice>();
            Content = GameServices.GetService<ContentManager>();
            graphics = GameServices.GetService<GraphicsDeviceManager>();

            world = new World(new Vector2(0, 0));
            GameServices.AddService<World>(world);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new track

            //raceTrack = Track.CreateTrack(TrackType.PenisTrack);
            randomRaceTrack = RandomTrack.createTrack();

            prevKeyboardState = Keyboard.GetState();


            polygonsColorShader = Content.Load<Effect>("Shaders/DrawnObjectsEffect");
            polygonsColorShader.CurrentTechnique = polygonsColorShader.Techniques["DoodleTechinque"];
            Texture2D trailSketch = Content.Load<Texture2D>("Materials/trailSketch");
            Texture2D objectSketch = Content.Load<Texture2D>("Materials/objectSketch");
            polygonsColorShader.Parameters["trailSketch"].SetValue(trailSketch);
            polygonsColorShader.Parameters["objectSketch"].SetValue(objectSketch);
            float[] random = new float[16 * 16];
            Color[] randomCol = new Color[16 * 16];
            Random seed = new Random();
            random[0] = 0.5f;
            for (int i = 1; i < random.Count(); i++) random[i] = (float)seed.NextDouble();
            for (int i = 0; i < random.Count(); i++) randomCol[i] = Color.White * random[i];
            Texture2D randomTex = new Texture2D(graphics.GraphicsDevice, 16, 16);
            randomTex.SetData(randomCol);
            polygonsColorShader.Parameters["random"].SetValue(randomTex);


            
            //create cars
            redCar = new Car(world, Color.Red, randomRaceTrack);
            cars.Add(redCar);
            if (playersNumber > 1)
            {
                blueCar = new Car(world, Color.Blue, randomRaceTrack);
                cars.Add(blueCar); 
                if (playersNumber > 2)
                {
                    greenCar = new Car(world, Color.Green, randomRaceTrack);
                    cars.Add(greenCar);
                    if (playersNumber > 3)
                    {
                        yellowCar = new Car(world, Color.Brown, randomRaceTrack);
                        cars.Add(yellowCar);
                    }
                }
            }

            //generate starting positions and angles
            int startingPoint = 0;
            positionCars(startingPoint);
            

            assetCreator = new AssetCreator(graphics.GraphicsDevice);
            assetCreator.LoadContent(this.Content);

            defaultViewport = GraphicsDevice.Viewport;
            topLeftViewport = defaultViewport;
            topRightViewport = defaultViewport;
            bottomLeftViewport = defaultViewport;
            topLeftViewport.Width = topLeftViewport.Width / 2 - 1;
            topRightViewport.Width = topRightViewport.Width / 2 - 1;
            bottomLeftViewport.Width = bottomLeftViewport.Width / 2 - 1;
            topLeftViewport.Height = topLeftViewport.Height / 2 - 1;
            topRightViewport.Height = topRightViewport.Height / 2 - 1;
            bottomLeftViewport.Height = bottomLeftViewport.Height / 2 - 1;
            topRightViewport.X = topLeftViewport.Width + 2;
            bottomLeftViewport.Y = bottomLeftViewport.Height + 2;

            //useless like an old kurva
            /*
            if (mGameMode == 0)
            {

                //eliminate split screen mode?

               // cameraTopLeft = new Camera(topLeftViewport, Vector2.Zero, new Vector2(topLeftViewport.Width / 2, topLeftViewport.Height / 2), 0.95f, 0.0f);
               // cameraTopRight = new Camera(topRightViewport, Vector2.Zero, new Vector2(topRightViewport.Width / 2, topRightViewport.Height / 2), 0.95f, 0.0f);
               // cameraBottomLeft = new Camera(bottomLeftViewport, Vector2.Zero, new Vector2(bottomLeftViewport.Width / 2, bottomLeftViewport.Height / 2), 0.95f, 0.0f);
               // cameraTopLeft.Follow(redCar, 0.0f);
               // cameraTopRight.Follow(blueCar, 0.0f);
               // cameraBottomLeft.Follow(greenCar, 0.0f);
            }
            else if (mGameMode==1)
            {
                //cameraFollowing = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.95f, 0.0f, cars);
                
            }
            */

            _debugView = new DebugViewXNA(world);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.Shape);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.DebugPanel);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.PerformanceGraph);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.Joint);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.ContactPoints);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.ContactNormals);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.Controllers);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.CenterOfMass);
            _debugView.AppendFlags(FarseerPhysics.DebugViewFlags.AABB);
            _debugView.DefaultShapeColor = Color.White;
            _debugView.SleepingShapeColor = Color.LightGray;
            _debugView.LoadContent(GraphicsDevice, Content);

            basicVert = new VertexPositionColorTexture[maxNumberOfTriangles];
            for (int i = 0; i < maxNumberOfTriangles; i++) basicVert[i].TextureCoordinate = new Vector2(-1);
                triangleListIndices = new short[maxNumberOfTriangles * 3];
        }

        public void positionCars(int startingPoint)
        {
            //compute cars positions
            startingPos = randomRaceTrack.computeStartingPositions(startingPoint);
            
            switch (playersNumber)
            {
                case 1:
                    redCar._compound.Position = startingPos[1];
                    break;
                case 2:
                    redCar._compound.Position = startingPos[1];
                    blueCar._compound.Position = startingPos[2];
                    break;
                case 3:
                    redCar._compound.Position = startingPos[1];
                    blueCar._compound.Position = startingPos[2];
                    greenCar._compound.Position = startingPos[0];
                    break;
                case 4:
                    redCar._compound.Position = startingPos[0];
                    blueCar._compound.Position = startingPos[1];
                    greenCar._compound.Position = startingPos[2];
                    yellowCar._compound.Position = startingPos[3];
                    break;
                default:
                    redCar._compound.Position = startingPos[1];
                    break;
            }

            
            float angle = randomRaceTrack.computeStartingAngle(startingPoint) - 90;
            for (int i = 0; i < cars.Count; i++)
            {
                //set current middle point
                cars[i].currentMiddlePoint = startingPoint;
                //set rotation
                cars[i]._compound.Rotation = angle;
                //erase velocity
                cars[i]._compound.LinearVelocity = Vector2.Zero;

                //clear trail
                cars[i].mIsTrailLoop = false;
                cars[i].mTrailPoints = 0;
                cars[i].justStarted = true;
            }
            
        }

        public override void Unload()
        {
            base.Unload();
            GameServices.DeleteService<World>();
        }

        public void SetGameMode(int gameMode)
        {
            mGameMode = gameMode;

            
            if (mGameMode==1)
            {
                cameraFollowing = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.95f, 0.0f, cars);
                
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // TODO: should this be executed before?
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (!IsActive)
                return;

            gps = GamePad.GetState(PlayerIndex.One);
            ks = Keyboard.GetState();
            PolygonPhysicsObject obstacle;

            // Allows the game to exit
            if (gps.Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
            {
                ScreenManager.AddScreen(PauseScreen, null);
                return;
                //ExitScreen();
            }

            for (int i = 0; i < cars.Count; i++)
            {
                // Update the position of the car
                cars[i].Update(GamePad.GetState(playerIndexes[i]), ks);
                // Find an obstacle (if any) drawn by the car and add it to the scene
                if (cars[i].TrailObstacle(world))
                {
                    obstacle = cars[i].TrailIntersection(world);
                    if (obstacle != null)
                    {
                        polygonsList.Add(obstacle);
                    }
                }

            }

            if (mGameMode == 0)
            {
                cameraTopLeft.Update(gameTime);
                cameraTopRight.Update(gameTime);
                cameraBottomLeft.Update(gameTime);
            }
            else if (mGameMode == 1)
            {
                cameraFollowing.Update(gameTime);
            }

            gameLogic();

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            // world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));

            
            

        }

        public void gameLogic()
        {
            //update ranking
            for (int i = 0; i < cars.Count; i++)
            {
                taken[i]=0;
            }
            for (int i = 0; i < cars.Count; i++)
            {
                int newMax=-1;
                int newIndex = 0;
                for (int ii = 0; ii < cars.Count; ii++)
                {
                    if (cars[ii].isActive && cars[ii].currentMiddlePoint > newMax && taken[ii] == 0)
                    {
                        newMax = cars[ii].currentMiddlePoint;
                        newIndex = ii;
                    }
                }
                taken[newIndex] = 1;
                ranking[i] = newIndex;
            }

            //debug ranking
            /*
            for (int i = 0; i < cars.Count; i++)
            {
                Console.Write(ranking[i]+" ");
            }
            Console.WriteLine();
            */

            //loop the active cars and check if anybody is off screen
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i].isActive)
                {
                    //compute screen coordinates
                    Vector2 screenPosition = Vector2.Transform(cars[i].Position, cameraFollowing.Transform);

                    //check if off screen 
                    //how to put graphics.PreferredBackBufferHeight and graphics.PreferredBackBufferWidth instead of 1000 and 1800????????????????????
                    if (screenPosition.X < 0 || screenPosition.X > 1800 || screenPosition.Y < 0 || screenPosition.Y > 1000)
                    {
                        //check if car is in the last position
                        if (ranking[playersNumber-1-currentExitIndex]==i){
                            //disactivate car and add index to the array with the order of exiting
                            cars[i].isActive = false;
                            orderToExit[currentExitIndex] = i;
                            currentExitIndex++;
                            break;
                        }
                    }
                }
            }

            //check if only one player remains
            if (currentExitIndex == playersNumber - 1)
            {
                currentExitIndex=0;
                
                //look for the remaining car and activate the others
                int winnerIndex = 0;
                for (int i = 0; i < cars.Count; i++)
                {
                    if (cars[i].isActive)
                    {
                        winnerIndex=i;
                    }
                    else{
                        cars[i].isActive=true;
                    }
                }

                //start a new race and update score
                positionCars(cars[winnerIndex].currentMiddlePoint % randomRaceTrack.curvePointsMiddle.Count);
                updateScore();
 
            }

            //set camera parameters
            cameraFollowing.firstCarIndex = ranking[0];
            cameraFollowing.lastCarIndex = ranking[playersNumber - 1 - currentExitIndex];
        }

        public void updateScore()
        {
            // update the score here, a mini race has just finished
            // order in which the cars have fallen off screen is stored in the array orderToExit

        }


        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            if (mGameMode == 0)
            {

                GraphicsDevice.Viewport = defaultViewport;
                GraphicsDevice.Clear(Color.White);

                GraphicsDevice.Viewport = topLeftViewport;
                //if not debug
                DrawSprites(cameraTopLeft);
                //if debug view
                //DrawSpritesDebug(cameraTopLeft);

                GraphicsDevice.Viewport = topRightViewport;
                //if not debug
                DrawSprites(cameraTopRight);
                //if debug
                //DrawSpritesDebug(cameraTopRight);

                GraphicsDevice.Viewport = bottomLeftViewport;
                //if not debug
                DrawSprites(cameraBottomLeft);
                //if debug
                //DrawSpritesDebug(cameraBottomLeft);
            }
            else if (mGameMode == 1)
            {
                GraphicsDevice.Viewport = defaultViewport;
                DrawSprites(cameraFollowing);
            }

            GraphicsDevice.Viewport = defaultViewport;

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

            base.Draw(gameTime);
        }

        public void DrawSprites(Camera camera)
        {

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            //draw 2D (!keep DepthStencilState to None in order to see shaders!)

            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Transform);

            // Draw the race track and the starting line
            // raceTrack.DrawSprites(camera, spriteBatch);
            randomRaceTrack.DrawSprites(camera, spriteBatch);


            // draw cars and their trails
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].Draw(spriteBatch, out trails[i]);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            polygonsColorShader.Parameters["Projection"].SetValue(projection);
            polygonsColorShader.Parameters["View"].SetValue(view);
            polygonsColorShader.CurrentTechnique.Passes["TrailPass"].Apply();
            if (playersNumber > 0)
            {
                polygonsColorShader.Parameters["redCarPos"].SetValue(cars[0]._compound.Position);
            }
            if (playersNumber > 1)
            {
                polygonsColorShader.Parameters["blueCarPos"].SetValue(cars[1]._compound.Position);
            }
            if (playersNumber > 2)
            {
                polygonsColorShader.Parameters["greenCarPos"].SetValue(cars[2]._compound.Position);
            }
            if (playersNumber > 3)
            {
                polygonsColorShader.Parameters["pinkCarPos"].SetValue(cars[3]._compound.Position);
            }
            for (int i = 0; i < cars.Count; i++)
            {
                //cars[i].Draw(spriteBatch, out trails[i]);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, trails[i], 0, 130 * 2);
            }


            //now draw 3D (shaders)

            //reset GraphicsDevice states (might be slow)


            int counter = 0;

            polygonsColorShader.CurrentTechnique.Passes["ObjectPass"].Apply();
            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw(ref projection, ref view, camera.Transform, ref basicVert, ref counter);
            }

            if (counter > 0)
            {

                //draw shader

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, counter);
            }

        }

        public void DrawSpritesDebug(Camera camera)
        {

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            //draw debug
            _debugView.RenderDebugData(ref projection, ref view);
        }
    }
}