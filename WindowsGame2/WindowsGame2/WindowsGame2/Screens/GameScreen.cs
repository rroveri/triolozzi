﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

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
using FarseerPhysics.Dynamics.Contacts;
using X2DPE;
using X2DPE.Helpers;
using System.Threading;

namespace WindowsGame2.Screens

{
    class GameScreen : AbstractScreen
    {

        #region Fields

        string[] collisionsQuotes;
        string[] collisionsQuotesNormal = {"ouch!", "bam", "boom", "crash!", "toc", "bang", "splat!", "ka pow!", "pow!", "thud!", "bong", "bonk!", "ka rack!", "rat tat tat" };
        string[] collisionsQuotesSerbian = { "kurvo!", "jebem ti mater bre!!", "boli me kurac!", "najebo si!", "picko!", "pusi kurac bre!!", "odjebi bre!!" };
        string[] collisionsQuotesGreek = { "kavliaris", "gourouna!", "poutsos", "eimai eggios", "parakalo?", "ta mu klasis ta arhidia", "effretikon" };
        string[] collisionsQuotesItalian = { "zio borghiano", "scrofa!", "porcano", "oca!", "sbocco anale", "asilo nido" };
        
 
        DebugViewXNA _debugView;
        World world;
        GraphicsDevice GraphicsDevice;
        ContentManager Content;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SoundManager soundManager;

        List<Car> Cars;
        List<PlayerIndex> playerIndexes;

        string[] paperEffects = { "redCarPos", "blueCarPos", "greenCarPos", "pinkCarPos" };

        List<PolygonPhysicsObject> polygonsList;
        VertexPositionColorTexture[][] trails = new VertexPositionColorTexture[4][];
        VertexPositionColorTexture[][] burnouts = new VertexPositionColorTexture[4][];

        Random Random;
        float[] randomArray;

        KeyboardState ks;

        AssetCreator assetCreator;

        Camera cameraFollowing;

        Viewport defaultViewport;

        RandomTrack randomRaceTrack;
        private GameLogic Logic;

        private RankingScreen RankScreen;
        float pauseAlpha;

        private PauseMenuScreen PauseScreen;

        Effect paperEffect;
        ScreenRenderer screenRenderer;
        StringWriter stringWriter = new StringWriter();
        Matrix projection;
        Matrix view;

        VertexPositionColorTexture[] basicVert;
        int maxNumberOfTriangles = 3000;

        VertexPositionColorTexture[] verticesBorders;

        Vertices startingPos;

        public bool readyToStart;

        AABB aabb;
        Vector2[] aabbVerts;
        int activeBodiesCount;

        //AABB startingPosAabb;
        Vector2[] startingPosAabbVerts;

        ParticleComponent particleComponent;
        Fluid fluid;
        Vector2 fluidPos = new Vector2(-1);
        Vector2[] currentPoses;
        Vector2[] prevPoses;
        Vector2 currentMousePos;
        Vector2 prevMousePos;

        public int PlayersCount { get; set; }

        private Texture2D dummyTexture;

        public SneezesManager mySneezesManager;
        private QuadRenderComponent quad;
        private bool gameIsExiting;
        private Thread fluidUpdateThread;

        Texture2D texInk;
        RenderTarget2D buffer;

        Effect fluidEffect;

        private bool isFullHd;

        private int obstaclesLoop;
        private Texture2D randomTex;

        private bool firstTime;


        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public GameScreen()
        {
            firstTime = true;

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            RankScreen = new RankingScreen();
            RankScreen.Accepted += RankScreenAccepted;
            PauseScreen = new PauseMenuScreen();

            polygonsList = new List<PolygonPhysicsObject>();
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

            particleComponent = GameServices.GetService<ParticleComponent>();

            collisionsQuotes = collisionsQuotesNormal;

            dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            byte whiteAlpha = 50;
            Color dummyTextureColor = new Color(whiteAlpha, whiteAlpha, whiteAlpha);
            dummyTexture.SetData(new Color[] { dummyTextureColor });

            mySneezesManager = new SneezesManager();

            

            obstaclesLoop = 0;
        }
 
           
        public override void LoadContent()
        {
            // We add to the GameServices objects that we want to be able to use accross different classes
            // without having to pass them explicitly every time.
            GraphicsDevice = GameServices.GetService<GraphicsDevice>();
            Content = GameServices.GetService<ContentManager>();
            graphics = GameServices.GetService<GraphicsDeviceManager>();
            soundManager = GameServices.GetService<SoundManager>();

            ScreenManager.AddScreen(RankScreen, null, false);
            ScreenManager.AddScreen(PauseScreen, null, false);

            world = new World(new Vector2(0, 0));
            GameServices.AddService<World>(world);

            this.world.ContactManager.PostSolve += new PostSolveDelegate(PostSolve);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new track
            randomRaceTrack = RandomTrack.createTrack();

            randomRaceTrack.polygonList = polygonsList;

            int[] crucialPoints = { 300, 600, 1000 };
            Logic = new GameLogic(crucialPoints, randomRaceTrack.curvePointsMiddle.Count);

            randomRaceTrack.gameLogic = Logic;
            Logic.DidFinishLap += randomRaceTrack.ResetStickyNotes;

            mySneezesManager.randomTrack = randomRaceTrack;

            LoadPaperEffect();

            screenRenderer = new ScreenRenderer();
            GameServices.AddService<ScreenRenderer>(screenRenderer);
            Logic.DidEliminateCar += screenRenderer.setSadToPlayer;
            Logic.DidFinishLap += screenRenderer.setLap;

            for (int i = 0; i < 4; i++)
            {
                Car aCar = new Car(world, Content.Load<Texture2D>("Images/small_car"), Color.White, randomRaceTrack, i, playerIndexes[i]);
                Cars.Add(aCar);
            }
            
            assetCreator = new AssetCreator(graphics.GraphicsDevice);
            assetCreator.LoadContent(this.Content);

            defaultViewport = GraphicsDevice.Viewport;


            isFullHd=(ScreenManager.preferredHeight!=720);

            // Single screen mode only
            cameraFollowing = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.0f, Cars.Count, isFullHd); 
            //ZOOM:

            //low res:
            //0.95 for 2 players
            //0.93 for 3 players
            //0.91 for 4 players

            //high res:
            //0.95 for 4 players

            GameServices.AddService<Camera>(cameraFollowing);

            mySneezesManager.camera = cameraFollowing;


            //generate starting positions and angles
            int startingPoint = 0;
           // positionCars(startingPoint);

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
          

            verticesBorders = new VertexPositionColorTexture[randomRaceTrack.curvePointsInternal.Count];


            fluid = new Fluid();
            fluid.Init();

            currentPoses = new Vector2[4];
            prevPoses = new Vector2[4];

            currentMousePos = new Vector2(100);
            prevMousePos = new Vector2(0);

            quad = new QuadRenderComponent(ScreenManager.Game, cameraFollowing, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);
            ScreenManager.Game.Components.Add(quad);

            fluidUpdateThread = new Thread(this.UpdateFluid);
            fluidUpdateThread.Start();

            mySneezesManager.fluid = fluid;

            fluidEffect = Content.Load<Effect>("Shaders/FluidEffect");
            fluidEffect.Parameters["random"].SetValue(randomTex);

            texInk = new Texture2D(graphics.GraphicsDevice,
                fluid.m_w, fluid.m_h, true,
                SurfaceFormat.Color);
            buffer = new RenderTarget2D(GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height, true,
                SurfaceFormat.Color, GraphicsDevice.PresentationParameters.DepthStencilFormat);

            //gaussian = new GaussianBlur(ScreenManager.Game);


    
        }

        private void LoadPaperEffect()
        {
            paperEffect = Content.Load<Effect>("Shaders/PaperEffect");
            paperEffect.CurrentTechnique = paperEffect.Techniques["DoodleTechinque"];
            Texture2D trailSketch = Content.Load<Texture2D>("Materials/trailSketch");
            Texture2D objectSketch = Content.Load<Texture2D>("Materials/pencil");
            Texture2D ink = Content.Load<Texture2D>("Materials/ink_texture");
            Texture2D startLine = Content.Load<Texture2D>("Materials/squares");
            Texture2D alphabet = Content.Load<Texture2D>("Images/alphabet");
            Texture2D messageBg = Content.Load<Texture2D>("Images/onomatopeeBg");
            Texture2D externalTex = Content.Load<Texture2D>("Images/external_bg3");
            paperEffect.Parameters["trailSketchBrush"].SetValue(dummyTexture);
            paperEffect.Parameters["trailSketch"].SetValue(trailSketch);
            paperEffect.Parameters["objectSketch"].SetValue(objectSketch);
            paperEffect.Parameters["ink"].SetValue(ink);
            paperEffect.Parameters["startLine"].SetValue(startLine);
            paperEffect.Parameters["alphabet"].SetValue(alphabet);
            paperEffect.Parameters["popupMessage"].SetValue(messageBg);
            paperEffect.Parameters["externalSketch"].SetValue(externalTex);
            randomArray = new float[16 * 16];
            Color[] randomCol = new Color[16 * 16];
            randomArray[0] = 0.5f;
            for (int i = 1; i < randomArray.Count(); i++) randomArray[i] = (float)Random.NextDouble();
            for (int i = 0; i < randomArray.Count(); i++) randomCol[i] = Color.White * randomArray[i];
            randomTex = new Texture2D(graphics.GraphicsDevice, 16, 16);
            randomTex.SetData(randomCol);
            paperEffect.Parameters["random"].SetValue(randomTex);
        }

        public void positionCars(int startingPointToCheck)
        {
            if (firstTime)
            {
                cameraFollowing.timerCanStart = true;
                firstTime = false;
                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].isVisible = true;
                }
            }
            else
            {
                cameraFollowing.firstTimeNonGongolare = false;
               // soundManager.PlaySound("Wish");
                cameraFollowing.timerShowOffCanStart = true;
            }
            

            GC.Collect();

            screenRenderer.setBulletNotShotToAllPlayers();

            int startingPoint = startingPointToCheck;
            bool tooClose=true;
            while (tooClose)
            {
                tooClose = false;
                for (int i=0; i<randomRaceTrack.dreamsMiddlePoints.Count; i++){
                    if (Math.Abs(startingPoint - randomRaceTrack.dreamsMiddlePoints[i]) < 20)
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

                    //NUOVA COSA PER FIXARE L'ASSERT IN DEBUG PROXYCOUNT!=0...MAGARI NON VA PIU UNA SEGA -- inizio
                    if (polygonsList[i].compound.FixtureList[j].ProxyCount == 0)
                    {
                       // foundTooClose = true;
                        continue;
                    }
                    //NUOVA COSA PER FIXARE L'ASSERT IN DEBUG PROXYCOUNT!=0...MAGARI NON VA PIU UNA SEGA -- fine

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

            bool didFinishLap = false;

            for (int i = 0; i < Cars.Count; i++)
            {
                

                Cars[i].isActive = true;
                
                //erase velocities
                Cars[i]._compound.ResetDynamics();

                Cars[i]._compound.Position = startingPos[i];

                // Check if moving the car to the new
                // position caused a lap to be completed
                if (startingPoint / randomRaceTrack.curvePointsMiddle.Count > Cars[i].currentMiddlePoint / randomRaceTrack.curvePointsMiddle.Count)
                {
                    didFinishLap = true;
                }
                
                //set current middle point
                Cars[i].currentMiddlePoint = startingPoint;

                //set rotation
                Cars[i]._compound.Rotation = angle;
                
                //clear trail
                Cars[i].mIsTrailLoop = false;
                Cars[i].mTrailPoints = 0;
                Cars[i].justStarted = true;

                Cars[i].resetBoost();
                Cars[i].resetTrail();

                Cars[i].isActive = false;
                Cars[i]._compound.IsStatic = true;

                Cars[i].message.disactivate();

                Cars[i].bulletIsShot = false;
            }

            if (didFinishLap)
            {
                Logic.IncreaseLaps();
            }
            
            readyToStart = true;
            cameraFollowing.raceCanStart = false;
        }

        public override void Unload()
        {
            base.Unload();
            GameServices.DeleteService<World>();
            GameServices.DeleteService<Camera>();
            GameServices.DeleteService<ScreenRenderer>();
            gameIsExiting = true;

            if (fluidUpdateThread != null)
            {
                fluidUpdateThread.Join();
                fluidUpdateThread = null;
            }
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

            if (ShouldPauseGame())
            {
                ScreenManager.ShowScreen<PauseMenuScreen>();
                soundManager.PauseSong();
                soundManager.StopAllSounds();

                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].stopSteeringSound();
                }

                    return;
            }

            if (Logic.isGameOver() && RankScreen != null)
            {
                soundManager.StopAllSounds();
                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].stopSteeringSound();
                }

                RankScreen.UpdateRankings(Cars);
                ScreenManager.ShowScreen<RankingScreen>();
                RankScreen = null;
                return;
            }

            if (cameraFollowing.raceCanStart && readyToStart)
            {
                readyToStart = false;
                for (int i = 0; i < Cars.Count; i++)
                {
                    Cars[i].isActive = true;
                    Cars[i]._compound.IsStatic = false;
                }
            }

            UpdateCars(gameTime);

            UpdateObstacles();

            UpdateGameLogic();

            UpdateCamera(gameTime);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));

            updateFluidParameters(gameTime);

            mySneezesManager.Update(gameTime,Cars);

            if (Keyboard.GetState().IsKeyDown(Keys.F)) fluid.saveDensity();

            if (Keyboard.GetState().IsKeyDown(Keys.D0)) Cars[0].setPowerup(0);
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) Cars[0].setPowerup(1);
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) Cars[0].setPowerup(2);
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) Cars[0].setPowerup(3);
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) Cars[0].setPowerup(4);
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) Cars[0].setPowerup(5);
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) Cars[0].setPowerup(6);
        }

        private void UpdateCars(GameTime gameTime)
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                particleComponent.particleEmitterList[i].Active = false;

                // TODO: declare obstacle as instance variable and set it to null here?
                PolygonPhysicsObject obstacle;
                // Update the position of the car
                Cars[i].Update(GamePad.GetState(playerIndexes[i]), ks, gameTime);
                // Find an obstacle (if any) drawn by the car and add it to the scene
                if (Cars[i].TrailObstacle(world))
                {
                    obstacle = Cars[i].TrailIntersection(world);
                    if (obstacle != null)
                    {
                        polygonsList.Add(obstacle);
                        obstacle.compound.UserData = 1000 + polygonsList.Count - 1;
                        soundManager.PlaySound("Boost");
                    }
                }
                
                Vector2 relativePos = Vector2.Transform(Cars[i].Position,cameraFollowing.Transform) - Vector2.Transform(fluid.renderPosition, cameraFollowing.Transform);

                float relativePosX = relativePos.X * fluid.m_w / fluid.renderWidth;
                float relativePosY = fluid.m_h + relativePos.Y * fluid.m_h / fluid.renderHeight;

                float densValue = fluid.fluidLevelAtPosition(relativePosX, relativePosY);
                if (densValue > 0.09f)
                {
                    Cars[i]._compound.LinearVelocity *= 0.8f;
                    Cars[i].resetBoost();

                    if (!Cars[i].isInsideMucus)
                    {
                        //play splat sound
                        soundManager.PlaySound(SoundManager.Splat);
                        Cars[i].isInsideMucus = true;
                    }
                }
                else
                {
                    Cars[i].isInsideMucus = false;
                }
            }

            for (int i = Cars.Count; i < Cars.Count * 2; i++ )
            {
                particleComponent.particleEmitterList[i].Active = false;
            }
            for (int i = Cars.Count*2; i < Cars.Count * 3; i++)
            {
                particleComponent.particleEmitterList[i].Active = false;
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

                    //NUOVA COSA PER FIXARE L'ASSERT IN DEBUG PROXYCOUNT!=0...MAGARI NON VA PIU UNA SEGA -- inizio ... NO!! LENTO!!! MA CMQ PENSO CHE IL LOOP INFINITO NON VIENE DA QUA!!
                    /*
                    if (polygonsList[i].compound.FixtureList[j].ProxyCount == 0)
                    {
                        polygonsList[i].compound.Enabled = true;
                        foundInside = true;
                        continue;
                    }
                     */
                    //NUOVA COSA PER FIXARE L'ASSERT IN DEBUG PROXYCOUNT!=0...MAGARI NON VA PIU UNA SEGA -- fine ... NO!! LENTO!!! MA CMQ PENSO CHE IL LOOP INFINITO NON VIENE DA QUA!!

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
                                if (polygonsList[i].compound.Enabled == false)
                                {
                                    polygonsList[i].compound.Enabled = true;
                                }
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
                    if (polygonsList[i].compound.Enabled == true)
                    {
                        polygonsList[i].compound.Enabled = false;
                    }
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
              //  screenRenderer.setHappyToAllPlayers();
                for (int i = 0; i < Cars.Count(); i++) Cars[i].stopPowerup();
            }
        }

        private void UpdateCamera(GameTime gameTime)
        {
            cameraFollowing.Update(gameTime, Cars, Logic._eliminatedCars);

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


        public void PostSolve(Contact contact, ContactConstraint impulse)
        {
            for (int i=0; i<Cars.Count; i++){
                if (Cars[i].currentPowerup == Car.powerupBig) Cars[i]._compound.AngularVelocity *= 0.1f;
                if (Cars[i]._compound.FixtureList.Contains(contact.FixtureA) || Cars[i]._compound.FixtureList.Contains(contact.FixtureB))
                {
                    float maxImpulse = 0.0f;
                    int count = contact.Manifold.PointCount;

                    

                    for (int j = 0; j < count; ++j)
                    {
                        maxImpulse = Math.Max(maxImpulse, impulse.Points[j].NormalImpulse);
                        if (Cars[i].currentPowerup == Car.powerupBig) maxImpulse /= 5;
                    }
                    
                    float multiplicator = 1f;
#if XBOX360
            multiplicator=2f;
#endif

                    Cars[i].startVibrationTimer(Math.Min(maxImpulse*2f*multiplicator,1.0f));


                    if (maxImpulse > 1)
                    {

                        

                        //randomly choose whether front or back

                        if (Cars[i]._compound.LinearVelocity.Length() > 2.0f)
                        {

                            int sign = 3;
                            if (Random.Next(2) == 1)
                            {
                                sign = -1;
                            }

                            Vector2 carTail = Cars[i]._compound.Position + sign * Cars[i].mDirection * Cars[i].tailOffset * 4;
                            Vector2 carDir = Cars[i].mDirection;
                            Vector2 carDirNormal = Vector2.Normalize(new Vector2(-carDir.Y, carDir.X));
                            int newIndex = Random.Next(collisionsQuotes.Count());
                            stringWriter.addString(collisionsQuotes[newIndex], Cars[i].mColor, maxImpulse / 5f, carTail - carDirNormal, carDirNormal);
                            Cars[i].resetTrail();

                            particleComponent.particleEmitterList[i].Position = Cars[i].Position;
                            particleComponent.particleEmitterList[i].Active = true;

                            // Play sound for car that crashed
                            soundManager.PlaySound(SoundManager.CarCrash);

                        }
                    }
                }
            }
        }

        private void updateFluidParameters(GameTime gameTime)
        {
            for (int i = 0; i < PlayersCount; i++)
            {
                currentPoses[i] = Vector2.Transform(Cars[i].Position, cameraFollowing.Transform);

                Vector2 relativePos = currentPoses[i] - Vector2.Transform(fluid.renderPosition, cameraFollowing.Transform);

                float relativePosX = relativePos.X * fluid.m_w / fluid.renderWidth;
                float relativePosY = fluid.m_h + relativePos.Y * fluid.m_h / fluid.renderHeight;

                float dX = currentPoses[i].X - prevPoses[i].X;
                float dY = currentPoses[i].Y - prevPoses[i].Y;

                if (fluid.fluidLevelAtPosition(relativePosX, relativePosY) <= 0.09f) continue;
                fluid.makeImpulse(relativePosX, relativePosY, dX, dY, false);

                prevPoses[i] = currentPoses[i]; 
            }

            /*
             * Server per disegnare col mouse dei nouvi muchi 
             * 
            */

            //if (keyState.IsKeyDown(Keys.Enter) && !previousState.IsKeyDown(Keys.Enter))
            //    this.texInk.Save("captura.bmp", ImageFileFormat.Bmp);

            //MouseState mouseState = Mouse.GetState();
            //KeyboardState keyState = Keyboard.GetState();
            
            //Vector2 currentPos = new Vector2((float)mouseState.X, (float)mouseState.Y);

            //ButtonState leftState = mouseState.LeftButton;

            //float dX = currentMousePos.X - prevMousePos.X;
            //float dY = currentMousePos.Y - prevMousePos.Y;

            //if (leftState == ButtonState.Pressed)
            //{
            //    fluid.makeImpulse(relativePosX, relativePosY, dX, dY, true);
            //}

            //previousState = keyState;

            //prevMousePos = currentMousePos;
            
        }

        void UpdateFluid()
        {
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 3 });
#endif

            while (!gameIsExiting)
            {
                fluid.Update(0.04f);
            }
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

        private void drawFluid()
        {
            
            //graphics.GraphicsDevice.Clear(Color.White);

            if (fluid.shouldResetDensity) return;

            graphics.GraphicsDevice.Textures[0] = null;

            texInk.SetData<Color>(fluid.texData);
            fluidEffect.Parameters["Texture"].SetValue(texInk);

#if !XBOX360
            fluidEffect.Parameters["textureSize"].SetValue(100.0f);
            fluidEffect.Parameters["texelSize"].SetValue(0.01f);
#endif
            //graphics.GraphicsDevice.SetRenderTarget(buffer);
            //graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);
            //coloredEffect.Techniques[0].Passes[0].Apply();
            //quad.renderMainQuad();
            //graphics.GraphicsDevice.SetRenderTarget(null);
            //Texture2D rend = gaussian.Render(buffer);
            fluidEffect.Parameters["Texture"].SetValue(texInk);
            fluidEffect.Techniques[0].Passes[0].Apply();
            quad.renderFromDisplayUnits(fluid.renderPosition,fluid.renderWidth,fluid.renderHeight);

            Texture2D trailSketch = Content.Load<Texture2D>("Materials/trailSketch");
            fluidEffect.Parameters["Texture"].SetValue(trailSketch);
            fluidEffect.Techniques[0].Passes[0].Apply();
            
            //quad.renderFromScreenUnits(currentMousePos, 10, 10);
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


            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            paperEffect.Parameters["Projection"].SetValue(projection);
            paperEffect.Parameters["View"].SetValue(view);

            

            


            //now draw 3D (shaders)

            //reset GraphicsDevice states (might be slow)


            int counter = 0;

            paperEffect.CurrentTechnique.Passes["StartLinePass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.startLineVertices, 0, 2);

            


            paperEffect.CurrentTechnique.Passes["ExternalSketchPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.verticesBordersInternal, 0, randomRaceTrack.verticesBordersInternal.Count() / 3);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.verticesBordersExternal, 0, randomRaceTrack.verticesBordersExternal.Count() / 3);


            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Transform);

            // Draw the race track and the starting line
            // raceTrack.DrawSprites(camera, spriteBatch);
            randomRaceTrack.DrawSpritesPostItQuotes(camera, spriteBatch);

            spriteBatch.End();

            paperEffect.CurrentTechnique.Passes["BorderPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.myArray, 0, randomRaceTrack.myArray.Count() / 3);


            if (Car.isBrush)
                paperEffect.CurrentTechnique.Passes["TrailPassBrush"].Apply();
            else
                paperEffect.CurrentTechnique.Passes["TrailPass"].Apply();

            for (int i = 0; i < Cars.Count; i++)
            {
                //cars[i].Draw(spriteBatch, out trails[i]);
                paperEffect.Parameters[paperEffects[i]].SetValue(Cars[i]._compound.Position);
                if (Cars[i].mIsTrailLoop)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, trails[i], 0, Car.mMaximumTrailPoints/2 * Car.paintersCount * 2);
                }
                else if (Cars[i].mTrailPoints/2 > 0)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, trails[i], 0, Cars[i].mTrailPoints/2 * Car.paintersCount * 2);
                }

            }
            paperEffect.CurrentTechnique.Passes["TrailPass"].Apply();
            for (int i = 0; i < Cars.Count; i++)
            {
                if (Cars[i].burnoutCounter > 0)
                {
                    int totalCount = Cars[i].burnoutCounter * 2;
                    if (Cars[i].loopBurnout)
                    {
                        totalCount = (Car.burnoutCounterMaxValue - 4)*2;
                    }
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, burnouts[i], 0, totalCount);
                }
            }

            paperEffect.CurrentTechnique.Passes["AlphabetPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, stringWriter.stringVertices, 0, stringWriter.stringVertices.Count() / 3);
            for (int i = 0; i < Cars.Count; i++)
            {
                if (Cars[i].message.isActive)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].message.stringWriter.stringVertices, 0, Cars[i].message.stringWriter.stringVertices.Count() / 3);
                }
            }
            
            paperEffect.CurrentTechnique.Passes["ObjectPass"].Apply();

            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw(ref projection, ref view, camera.Transform, ref basicVert, ref counter, maxNumberOfTriangles, ref obstaclesLoop);
            }

            if (counter > 0)
            {
                int totalNumber = counter;
                if (obstaclesLoop > 0)
                {
                    totalNumber = obstaclesLoop;
                }
                //draw shader
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, totalNumber);
            }

            //draw laser
            paperEffect.CurrentTechnique.Passes["TrailPass"].Apply();
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].DrawBullet(spriteBatch);
                if (Cars[i].bullet.isGoing && Cars[i].isActive)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].bullet.laserVertices, 0, 2);
                }
            }


            paperEffect.CurrentTechnique.Passes["PopupMessagePass"].Apply();
            for (int i = 0; i < Cars.Count; i++)
            {
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].message.bgTextureVertices, 0, 2);
            }

            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Transform);

            // draw cars and their trails
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].DrawGlow(spriteBatch);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].DrawGloatingStars(spriteBatch);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            // draw cars and their trails
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].Draw(spriteBatch, out trails[i], out burnouts[i]);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].DrawPowerup(spriteBatch);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            spriteBatch.End();

            drawFluid();

            

            screenRenderer.drawHUD(ref Cars);

            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.TransformNoZoom);

            // draw cars and their trails
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].DrawMessage(spriteBatch);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            spriteBatch.End();


            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            cameraFollowing.counterIndicator.Draw(spriteBatch);
            spriteBatch.End();


        }

        public void DrawSpritesDebug(Camera camera)
        {

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            //draw debug
            _debugView.RenderDebugData(ref projection, ref view);
        }

        public void RankScreenAccepted(object sender, PlayerIndexEventArgs e)
        {
            soundManager.StopSong();
            
            ScreenManager.QuitGame();
        }

        public void InitializeGame(int playersCount, ref List<Tuple<Texture2D, Color>> cars)
        {
            Cars.Clear();
            particleComponent.particleEmitterList.Clear();

            PlayersCount = playersCount;
            screenRenderer.PlayersCount = playersCount;
            Logic.PlayersCount = playersCount;

            randomRaceTrack.carsCount = playersCount;

            for (int i = 0; i < playersCount; i++)
            {
                Cars.Add(new Car(world, cars[i].Item1, cars[i].Item2, randomRaceTrack, i, playerIndexes[i]));
                screenRenderer.SetColor(cars[i].Item2, i);
            }

            double frequency = 1d;
#if XBOX360
            frequency = 2d;
#endif

            //add particles for collisions with walls and cars
            for (int i = 0; i < Cars.Count; i++)
            {
                particleComponent.particleEmitterList.Add(
                        new Emitter()
                        {
                            Active = false,
                            TextureList = new List<Texture2D>() {
                                Content.Load<Texture2D>("Images\\whiteStar"),
                               // Content.Load<Texture2D>("Sprites\\flower_orange"),
                               // Content.Load<Texture2D>("Sprites\\flower_green"),
                               // Content.Load<Texture2D>("Sprites\\flower_yellow"),
                               // Content.Load<Texture2D>("Sprites\\flower_purple")
                                },
                            RandomEmissionInterval = new RandomMinMax(2d),
                            ParticleLifeTime = 1500,
                            ParticleDirection = new RandomMinMax(0, 359),
                            ParticleSpeed = new RandomMinMax(9.1f, 13.0f),
                            ParticleRotation = new RandomMinMax(0, 100),
                            RotationSpeed = new RandomMinMax(0.015f),
                            ParticleFader = new ParticleFader(false, true, 1350),
                            ParticleScaler = new ParticleScaler(false, 0.3f),
                            TextureColor = Cars[i].mColor
                        }
                );
            }
            //add particles for collisions with nightmares
            for (int i = 0; i < Cars.Count; i++)
            {
                particleComponent.particleEmitterList.Add(
                        new Emitter()
                        {
                            Active = false,
                            TextureList = new List<Texture2D>() {
                                Content.Load<Texture2D>("Sprites\\smoke"),
                               // Content.Load<Texture2D>("Sprites\\flower_orange"),
                               // Content.Load<Texture2D>("Sprites\\flower_green"),
                              //  Content.Load<Texture2D>("Sprites\\flower_yellow"),
                              //  Content.Load<Texture2D>("Sprites\\flower_purple")
                                },
                            RandomEmissionInterval = new RandomMinMax(frequency),
                            ParticleLifeTime = 1000,
                            ParticleDirection = new RandomMinMax(0, 359),
                            ParticleSpeed = new RandomMinMax(5.1f, 7.0f),
                            ParticleRotation = new RandomMinMax(0, 100),
                            RotationSpeed = new RandomMinMax(0.015f),
                            ParticleFader = new ParticleFader(false, true, 1350),
                            ParticleScaler = new ParticleScaler(false, 0.3f),
                            TextureColor = Color.Black
                        }
                );
            }

            //add particles for collisions with wishes
            for (int i = 0; i < Cars.Count; i++)
            {
                particleComponent.particleEmitterList.Add(
                        new Emitter()
                        {
                            Active = false,
                            TextureList = new List<Texture2D>() {
                                Content.Load<Texture2D>("Sprites\\smokeWhite"),
                                //Content.Load<Texture2D>("Sprites\\flower_orange"),
                               // Content.Load<Texture2D>("Sprites\\flower_green"),
                                //Content.Load<Texture2D>("Sprites\\flower_yellow"),
                                //Content.Load<Texture2D>("Sprites\\flower_purple")
                                },
                            RandomEmissionInterval = new RandomMinMax(frequency),
                            ParticleLifeTime = 1000,
                            ParticleDirection = new RandomMinMax(0, 359),
                            ParticleSpeed = new RandomMinMax(5.1f, 7.0f),
                            ParticleRotation = new RandomMinMax(0, 100),
                            RotationSpeed = new RandomMinMax(0.015f),
                            ParticleFader = new ParticleFader(false, true, 1350),
                            ParticleScaler = new ParticleScaler(false, 0.3f),
                            TextureColor = Color.White
                        }
                );
            }


            cameraFollowing.setCameraZoom(Cars.Count);

            // Call the Jean Charles
            GC.Collect();
        }
    }
}