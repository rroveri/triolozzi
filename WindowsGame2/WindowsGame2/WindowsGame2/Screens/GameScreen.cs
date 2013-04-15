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
using FarseerPhysics.Collision;

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

        List<Car> Cars, AllCars;
        List<PlayerIndex> playerIndexes;
        Color[] carColors = { Color.Red, Color.Blue, Color.Green, Color.Brown };
        string[] paperEffects = { "redCarPos", "blueCarPos", "greenCarPos", "pinkCarPos" };

        List<PolygonPhysicsObject> polygonsList;
        VertexPositionColorTexture[][] trails = new VertexPositionColorTexture[4][];
        VertexPositionColorTexture[][] burnouts = new VertexPositionColorTexture[4][];


        KeyboardState prevKeyboardState;
        Random Random;
        float[] randomArray;
        //int lastFrame = 0, randomIndex = 0;

        KeyboardState ks;

        AssetCreator assetCreator;

        Camera cameraFollowing;

        Viewport defaultViewport;

        RandomTrack randomRaceTrack;
        private GameLogic Logic;

        PauseMenuScreen PauseScreen;
        RankingScreen RankScreen;
        float pauseAlpha;

        Effect paperEffect, screenEffect;
        ScreenRenderer screenRenderer;
        Matrix projection;
        Matrix view;

        VertexPositionColorTexture[] basicVert;
        short[] triangleListIndices;
        int maxNumberOfTriangles = 10000;

        Vertices startingPos;

        public bool readyToStart;

        AABB aabb;
        Vector2[] aabbVerts;
        int activeBodiesCount;

        //AABB startingPosAabb;
        Vector2[] startingPosAabbVerts;

        private int _playersCount;
        public int PlayersCount
        {
            get
            {
                return _playersCount;
            }
            set
            {
                // AllCars contains the 4 cars
                // When changing the players count we just remove all cars from the Cars list
                // and add the chosen number of cars taking them from the AllCars list (this avoids creating new Car objects during the game)
                // When we create the GameScreen the PlayersCount is set to 4 (the maximum number of players) and this setter method is called.
                // Since the ScreenRenderer and AllCars are not initialized yet, we skip this first call with an if-statement.
                Cars.Clear();
                _playersCount = value;
                if (screenRenderer != null)
                {
                    screenRenderer.PlayersCount = value;
                    for (int i = 0; i < value; i++)
                    {
                        Cars.Add(AllCars[i]);
                    }
                }
            }
        }

        //private int _pauseTime;
        //private int kDefaultPauseTime = 3000;

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
            RankScreen = new RankingScreen("RANKINGS");
            RankScreen.Accepted += RankScreenAccepted;

            polygonsList = new List<PolygonPhysicsObject>();
            AllCars = new List<Car>();
            Cars = new List<Car>();
            playerIndexes = new List<PlayerIndex>();
            playerIndexes.Add(PlayerIndex.One); playerIndexes.Add(PlayerIndex.Two);
            playerIndexes.Add(PlayerIndex.Three); playerIndexes.Add(PlayerIndex.Four);
            Random = new Random(DateTime.Now.Millisecond);

            PlayersCount = 4;
            startingPos = new Vertices();

            readyToStart = false;
            aabbVerts = new Vector2[4];
            activeBodiesCount = 0;
            startingPosAabbVerts = new Vector2[4];
        }

        public override void LoadContent()
        {
            // We add to the GameServices objects that we want to be able to use accross different classes
            // without having to pass them explicitly every time.
            GraphicsDevice = GameServices.GetService<GraphicsDevice>();
            Content = GameServices.GetService<ContentManager>();
            graphics = GameServices.GetService<GraphicsDeviceManager>();

            ScreenManager.AddScreen(PauseScreen, null);
            ScreenManager.AddScreen(RankScreen, null);

            world = new World(new Vector2(0, 0));
            GameServices.AddService<World>(world);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new track
            randomRaceTrack = RandomTrack.createTrack();

            // TODO: redo with crucial points generated by racetrack
            int[] crucialPoints = new int[2];
            crucialPoints[0] = 200; crucialPoints[1] = 400;
            Logic = new GameLogic(crucialPoints, randomRaceTrack.curvePointsMiddle.Count);

            randomRaceTrack.gameLogic = Logic;
            Logic.DidFinishLap += randomRaceTrack.ResetStickyNotes;

            prevKeyboardState = Keyboard.GetState();


            paperEffect = Content.Load<Effect>("Shaders/PaperEffect");
            paperEffect.CurrentTechnique = paperEffect.Techniques["DoodleTechinque"];
            Texture2D trailSketch = Content.Load<Texture2D>("Materials/trailSketch");
            Texture2D objectSketch = Content.Load<Texture2D>("Materials/objectSketch");
            Texture2D ink = Content.Load<Texture2D>("Materials/ink_texture");
            paperEffect.Parameters["trailSketch"].SetValue(trailSketch);
            paperEffect.Parameters["objectSketch"].SetValue(objectSketch);
            paperEffect.Parameters["ink"].SetValue(ink);
            randomArray = new float[16 * 16];
            Color[] randomCol = new Color[16 * 16];
            randomArray[0] = 0.5f;
            for (int i = 1; i < randomArray.Count(); i++) randomArray[i] = (float)Random.NextDouble();
            for (int i = 0; i < randomArray.Count(); i++) randomCol[i] = Color.White * randomArray[i];
            Texture2D randomTex = new Texture2D(graphics.GraphicsDevice, 16, 16);
            randomTex.SetData(randomCol);
            paperEffect.Parameters["random"].SetValue(randomTex);

            screenRenderer = new ScreenRenderer(4);
            screenEffect = Content.Load<Effect>("Shaders/ScreenEffect");
            screenEffect.CurrentTechnique = screenEffect.Techniques["ScreenTechinque"];
            Texture2D postitHappy = Content.Load<Texture2D>("Images/postitHappy");
            screenEffect.Parameters["postitHappy"].SetValue(postitHappy);
            Texture2D postitSad = Content.Load<Texture2D>("Images/postitSad");
            screenEffect.Parameters["postitSad"].SetValue(postitSad);
            Texture2D numbers = Content.Load<Texture2D>("Images/numbers");
            screenEffect.Parameters["numbers"].SetValue(numbers);

            for (int i = 0; i < 4; i++)
            {
                Car aCar = new Car(world, carColors[i], randomRaceTrack);
                AllCars.Add(aCar);
                Cars.Add(aCar);
            }
            
            assetCreator = new AssetCreator(graphics.GraphicsDevice);
            assetCreator.LoadContent(this.Content);

            defaultViewport = GraphicsDevice.Viewport;

            // Single screen mode only
            cameraFollowing = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.95f, 0.0f);

            //generate starting positions and angles
            int startingPoint = 0;
            positionCars(startingPoint);

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

        public void positionCars(int startingPointToCheck)
        {
            GC.Collect();

            int startingPoint = startingPointToCheck;
            bool tooClose=true;
            while (tooClose)
            {
                tooClose = false;
                for (int i=0; i<randomRaceTrack.dreamsMiddlePoints.Count; i++){
                    if (Math.Abs(startingPoint - randomRaceTrack.dreamsMiddlePoints[i]) < 10)
                    {
                        tooClose = true;
                        startingPoint++;
                    }
                }      
            }

            //compute cars positions
            startingPos = randomRaceTrack.computeStartingPositions(startingPoint);

            //clear part of track where cars are positioned
            float distMargin=2f;
            for (int i = 0; i < polygonsList.Count; i++)
            {
                bool foundTooClose = false;
                for (int j = 0; j < polygonsList[i].compound.FixtureList.Count; j++)
                {
                    if (foundTooClose)
                    {
                        break;
                    }

                    //compute bounding AABB bounding box of fixture !!! USE VERTICES INSTEAD IF NOT ENOUGH PRECISE !!!
                    polygonsList[i].compound.FixtureList[j].GetAABB(out aabb, 0);

                    aabbVerts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
                    aabbVerts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
                    aabbVerts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
                    aabbVerts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

                    for (int u = 0; u < Cars.Count; u++)
                    {
                        if (Vector2.Distance(polygonsList[i].compound.LocalCenter, startingPos[u]) < distMargin)
                        {
                            foundTooClose = true;
                            break;
                        }
                        for (int k = 0; k < 4; k++)
                        {
                            float dist = Vector2.Distance(aabbVerts[k], startingPos[u]);
                            if (dist < distMargin)
                            {
                                foundTooClose = true;
                                break;
                            }
                        }
                    }
                }
                if (foundTooClose)
                {
                    //debug
                    //polygonsList[i].Color = Color.Black;

                    //don't want to screw up vertices, so just throw it far away like an old kurva
                    polygonsList[i].compound.Position = new Vector2(100000,100000);
                    polygonsList[i].compound.Enabled = false;
                    polygonsList[i].IsValid = false;

                }
            }

            //compute angle
            float angle = randomRaceTrack.computeStartingAngle(startingPoint) - 90;
            
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].isActive = true;
                
                //erase velocities
                Cars[i]._compound.ResetDynamics();

                Cars[i]._compound.Position = startingPos[i];
                //set current middle point
                Cars[i].currentMiddlePoint = startingPoint;
                //set rotation
                Cars[i]._compound.Rotation = angle;
                

                //clear trail
                Cars[i].mIsTrailLoop = false;
                Cars[i].mTrailPoints = 0;
                Cars[i].justStarted = true;

                Cars[i].resetBoost();

                Cars[i].isActive = false;
                Cars[i]._compound.Enabled = false;
            }
            
            readyToStart = true;
            cameraFollowing.raceCanStart = false;
        }

        public override void Unload()
        {
            base.Unload();
            GameServices.DeleteService<World>();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (!IsActive)
                return;

            //if (lastFrame == 5)
            //{
            //    randomIndex = (randomIndex + 1) % randomArray.Count();
            //    lastFrame = 0;
            //}
            //lastFrame++;
            //paperEffect.Parameters["randomSeed"].SetValue(randomArray[randomIndex]);

            if (ShouldPauseGame())
            {
                ScreenManager.ShowScreen<PauseMenuScreen>();
                return;
            }

            if (Logic.isGameOver())
            {
                RankScreen.UpdateRankings(Cars);
                ScreenManager.ShowScreen<RankingScreen>();
                return;
            }

            if (cameraFollowing.raceCanStart && readyToStart)
            {
                readyToStart = false;
                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].isActive = true;
                    Cars[i]._compound.Enabled = true;
                }
            }

            UpdateCars();

            UpdateObstacles();

            UpdateGameLogic();

            UpdateCamera(gameTime);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
        }

        private void UpdateCars()
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                // TODO: declare obstacle as instance variable and set it to null here?
                PolygonPhysicsObject obstacle;
                // Update the position of the car
                Cars[i].Update(GamePad.GetState(playerIndexes[i]), ks);
                // Find an obstacle (if any) drawn by the car and add it to the scene
                if (Cars[i].TrailObstacle(world))
                {
                    obstacle = Cars[i].TrailIntersection(world);
                    if (obstacle != null)
                    {
                        polygonsList.Add(obstacle);
                    }
                }

            }
        }

        private void UpdateObstacles()
        {
            //enable or disable polygons if off screen
            activeBodiesCount = 0;

            for (int i = 0; i < polygonsList.Count; i++)
            {
                bool foundInside = false;
                for (int j = 0; j < polygonsList[i].compound.FixtureList.Count; j++)
                {
                    if (foundInside)
                    {
                        break;
                    }

                    //compute bounding AABB bounding box of fixture !!! USE VERTICES INSTEAD IF NOT ENOUGH PRECISE !!!
                    polygonsList[i].compound.FixtureList[j].GetAABB(out aabb, 0);

                    aabbVerts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
                    aabbVerts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
                    aabbVerts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
                    aabbVerts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);


                    for (int k = 0; k < 4; k++)
                    {
                        //check if vertices of the bounding box are inside or outside
                        bool outside = checkIfOffScreen(aabbVerts[k]);

                        if (!outside)
                        {
                            if (polygonsList[i].IsValid)
                            {
                                polygonsList[i].compound.Enabled = true;
                                foundInside = true;

                                //for debugging
                                activeBodiesCount++;
                                break;
                            }
                        }
                    }


                }
                if (!foundInside)
                {
                    polygonsList[i].compound.Enabled = false;

                }
            }
        }

        private bool checkIfOffScreen(Vector2 vec){
            Vector2 screenPosition = Vector2.Transform(ConvertUnits.ToDisplayUnits(vec), cameraFollowing.Transform);
            if (screenPosition.X < 0 || screenPosition.X > graphics.PreferredBackBufferWidth || screenPosition.Y < 0 || screenPosition.Y > graphics.PreferredBackBufferHeight)
            {
                return true;
            }
            else{
                return false;
            }
        }

        private void UpdateGameLogic()
        {
            Logic.Update(Cars, cameraFollowing.Transform, graphics);

            if (Logic.isMiniRaceOver)
            {
                int newMiddlePoint = findACloserMiddlePoint();
                positionCars(newMiddlePoint % randomRaceTrack.curvePointsMiddle.Count);
            }
        }

        private void UpdateCamera(GameTime gameTime)
        {
            cameraFollowing.Update(gameTime, Cars);

            //set camera parameters
            cameraFollowing.firstCarIndex = Logic.Ranking[0];
            if (PlayersCount > 1)
            {
                cameraFollowing.lastCarIndex = Logic.LastCarIndex;
            }
            else
            {
                cameraFollowing.lastCarIndex = Logic.Ranking[0];
            }
        }

        public int findACloserMiddlePoint()
        {
            int farAwayMiddlePoint=Cars[Logic.Ranking[0]].currentMiddlePoint;
            int pointsToCheck=100;
            float minDist = Vector2.Distance(Cars[Logic.Ranking[0]]._compound.Position, randomRaceTrack.curvePointsMiddle[farAwayMiddlePoint % randomRaceTrack.curvePointsMiddle.Count]);
            int closestMiddlePoint = farAwayMiddlePoint;
            for (int i = farAwayMiddlePoint; i > farAwayMiddlePoint - pointsToCheck; i--)
            {
                if (i < 0)
                {
                    break;
                }
                float newDist = Vector2.Distance(Cars[Logic.Ranking[0]]._compound.Position, randomRaceTrack.curvePointsMiddle[i % randomRaceTrack.curvePointsMiddle.Count]);
                if (newDist < minDist)
                {
                    minDist = newDist;
                    closestMiddlePoint = i;
                }
            }
            return closestMiddlePoint;
        }

        private bool ShouldPauseGame()
        {
            // Check for keyboard 'Esc'
            ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape))
            {
                return true;
            }
            // Check if any players has pressed the back button
            for (int i = 0; i < playerIndexes.Count; i++)
            {
                if (GamePad.GetState(playerIndexes[i]).Buttons.Back == ButtonState.Pressed)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            GraphicsDevice.Viewport = defaultViewport;
            DrawSprites(cameraFollowing);
            
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
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].Draw(spriteBatch, out trails[i], out burnouts[i]);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            paperEffect.Parameters["Projection"].SetValue(projection);
            paperEffect.Parameters["View"].SetValue(view);

            paperEffect.CurrentTechnique.Passes["TrailPass"].Apply();

            for (int i = 0; i < Cars.Count; i++)
            {
                //cars[i].Draw(spriteBatch, out trails[i]);
                paperEffect.Parameters[paperEffects[i]].SetValue(Cars[i]._compound.Position);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, trails[i], 0, 130 * 2);

                
            }

            for (int i = 0; i < Cars.Count; i++)
            {
                if (Cars[i].burnoutCounter > 0)
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, burnouts[i], 0, Cars[i].burnoutCounter * 2);
            }


            //now draw 3D (shaders)

            //reset GraphicsDevice states (might be slow)


            int counter = 0;

            paperEffect.CurrentTechnique.Passes["BorderPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.myArray, 0, randomRaceTrack.myArray.Count() / 3);

            paperEffect.CurrentTechnique.Passes["ObjectPass"].Apply();

            

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

            screenEffect.CurrentTechnique.Passes["PostitPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.postitVertices, 0, PlayersCount * 2);

            screenEffect.CurrentTechnique.Passes["BarPass"].Apply();
            for (int i = 0; i < PlayersCount; i++)
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.barVertices[i], 0, Cars[i].score * 2);
        }

        public void DrawSpritesDebug(Camera camera)
        {

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            //draw debug
            _debugView.RenderDebugData(ref projection, ref view);
        }

        void RankScreenAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.RemoveScreen(this);
            ScreenManager.RemoveScreen(PauseScreen);
            ScreenManager.RemoveScreen(RankScreen);
            ScreenManager.AddScreen(new GameScreen(), null);
            ScreenManager.ShowScreen<MainMenuScreen>();
        }
    }
}