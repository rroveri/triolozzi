using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using FarseerPhysics.DebugViews;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.SamplesFramework;
using WindowsGame2.Screens;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using X2DPE;
using X2DPE.Helpers;
using FarseerPhysics.Dynamics.Contacts;
using WindowsGame2.Events;
using System.Threading;

namespace WindowsGame2.GameElements
{
    class Physics : Thread
    {

        #region Fields

        string[] collisionsQuotes;
        string[] collisionsQuotesNormal = { "ouch!", "bam", "boom", "crash!", "toc", "bang", "splat!", "ka pow!", "pow!", "thud!", "bong", "bonk!", "ka rack!", "rat tat tat" };
        string[] collisionsQuotesSerbian = { "kurvo!", "jebem ti mater bre!!", "boli me kurac!", "najebo si!", "picko!", "pusi kurac bre!!", "odjebi bre!!" };
        string[] collisionsQuotesGreek = { "kavliaris", "gourouna!", "poutsos", "eimai eggios", "parakalo?", "ta mu klasis ta arhidia", "effretikon" };
        string[] collisionsQuotesItalian = { "zio borghiano", "scrofa!", "porcano", "oca!", "sbocco anale", "asilo nido" };

        World world;
        GraphicsDevice GraphicsDevice;
        ContentManager Content;
        GraphicsDeviceManager graphics;
        //SpriteBatch spriteBatch;
        SoundManager soundManager;

        public List<Car> Cars;
        List<PlayerIndex> playerIndexes;

        //string[] paperEffects = { "redCarPos", "blueCarPos", "greenCarPos", "pinkCarPos" };

        public List<PolygonPhysicsObject> polygonsList;
        //VertexPositionColorTexture[][] trails = new VertexPositionColorTexture[4][];
        //VertexPositionColorTexture[][] burnouts = new VertexPositionColorTexture[4][];
        
        Random Random;
        float[] randomArray;

        public GameInput gameInput;

        AssetCreator assetCreator;

        public Camera cameraFollowing;

        public Viewport defaultViewport;

        public RandomTrack randomRaceTrack;
        private GameLogic Logic;

        //private RankingScreen RankScreen;
        float pauseAlpha;

        //private PauseMenuScreen PauseScreen;

        //Effect paperEffect, screenEffect;
        //ScreenRenderer screenRenderer;
        StringWriter stringWriter = new StringWriter();
        Matrix projection;
        Matrix view;

        VertexPositionColorTexture[] verticesBorders;

        Vertices startingPos;

        public bool readyToStart;

        AABB aabb;
        Vector2[] aabbVerts;
        int activeBodiesCount;

        Vector2[] startingPosAabbVerts;

        ParticleComponent particleComponent;

        public int PlayersCount { get; set; }

        public GameTime gameTime { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public Physics(GameLogic gameLogic)
        {

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

            Logic = gameLogic;
            Logic.DidFinishMiniRace += DidFinishMiniRace;

            //dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            //byte whiteAlpha = 50;
            //Color dummyTextureColor = new Color(whiteAlpha, whiteAlpha, whiteAlpha);
            //dummyTexture.SetData(new Color[] { dummyTextureColor });

            //mySneezesManager = new SneezesManager();
        }

        public void InitializeGame(int playersCount, ref int[] selectedCars, ref Texture2D[] availableCars, ref int[] selectedColors, ref Color[] availableColors)
        {
            Cars.Clear();
            particleComponent.particleEmitterList.Clear();

            PlayersCount = playersCount;
            //screenRenderer.PlayersCount = playersCount;
            Logic.PlayersCount = playersCount;

            randomRaceTrack.carsCount = playersCount;

            for (int i = 0; i < playersCount; i++)
            {
                Cars.Add(new Car(world, availableCars[selectedCars[i]], availableColors[selectedColors[i]], randomRaceTrack, i));
                //screenRenderer.SetColor(availableColors[selectedColors[i]], i);
            }


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
                            RandomEmissionInterval = new RandomMinMax(0.5d),
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
                            RandomEmissionInterval = new RandomMinMax(0.5d),
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




            // Call the Jean Charles
            GC.Collect();
        }

        #endregion

        public void LoadContent()
        {
            // We add to the GameServices objects that we want to be able to use accross different classes
            // without having to pass them explicitly every time.
            Content = GameServices.GetService<ContentManager>();
            graphics = GameServices.GetService<GraphicsDeviceManager>();
            GraphicsDevice = graphics.GraphicsDevice;
            soundManager = GameServices.GetService<SoundManager>();

            world = new World(new Vector2(0, 0));
            GameServices.AddService<World>(world);

            this.world.ContactManager.PostSolve += new PostSolveDelegate(PostSolve);

            // Create a new track
            randomRaceTrack = RandomTrack.createTrack();

            Logic.PointsCount = randomRaceTrack.curvePointsMiddle.Count;

            randomRaceTrack.gameLogic = Logic;
            Logic.DidFinishLap += randomRaceTrack.ResetStickyNotes;

            for (int i = 0; i < 4; i++)
            {
                Car aCar = new Car(world, Content.Load<Texture2D>("Images/small_car"), Color.White, randomRaceTrack, i);
                Cars.Add(aCar);
            }
            
            assetCreator = new AssetCreator(GraphicsDevice);
            assetCreator.LoadContent(this.Content);

            defaultViewport = GraphicsDevice.Viewport;

            // Single screen mode only
            cameraFollowing = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.95f, 0.0f);
            GameServices.AddService<Camera>(cameraFollowing);

            //generate starting positions and angles
            int startingPoint = 0;
            positionCars(startingPoint);


            verticesBorders = new VertexPositionColorTexture[randomRaceTrack.curvePointsInternal.Count];
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
                Cars[i].resetTrail();

                Cars[i].isActive = false;
                Cars[i]._compound.Enabled = false;

                Cars[i].message.disactivate();
            }
            
            readyToStart = true;
            cameraFollowing.raceCanStart = false;
        }

        public void Unload()
        {
            GameServices.DeleteService<World>();
            GameServices.DeleteService<Camera>();
        }

        public void Update()
        {
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

            Logic.Update(Cars, cameraFollowing.Transform, graphics);

            UpdateCamera();

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
        }

        private void UpdateCars()
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                particleComponent.particleEmitterList[i].Active = false;

                PolygonPhysicsObject obstacle;
                // Update the position of the car
                Cars[i].Update(i, gameInput, gameTime);
                // Find an obstacle (if any) drawn by the car and add it to the scene
                if (Cars[i].TrailObstacle(world))
                {
                    obstacle = Cars[i].TrailIntersection(world);
                    if (obstacle != null)
                    {
                        polygonsList.Add(obstacle);
                    }
                }
                // TODO
                //Vector2 screen = Vector2.Transform(Cars[i].Position, cameraFollowing.Transform);
                //float densValue = fluid.fluidLevelAtPosition(screen);
                //if (densValue > 0.09f) Cars[i]._compound.LinearVelocity *= 0.8f;
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

        private void DidFinishMiniRace(object sender, MiniRaceOverEventArgs e)
        {
            int newMiddlePoint = findACloserMiddlePoint();
            positionCars(newMiddlePoint % randomRaceTrack.curvePointsMiddle.Count);
        }

        private void UpdateCamera()
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

        public void PostSolve(Contact contact, ContactConstraint impulse)
        {
            for (int i=0; i<Cars.Count; i++){
                if (Cars[i]._compound.FixtureList.Contains(contact.FixtureA) || Cars[i]._compound.FixtureList.Contains(contact.FixtureB))
                {
                    float maxImpulse = 0.0f;
                    int count = contact.Manifold.PointCount;

                    for (int j = 0; j < count; ++j)
                    {
                        maxImpulse = Math.Max(maxImpulse, impulse.Points[j].NormalImpulse);
                    }
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
                            //particleComponent.particleEmitterList[i].Active = true;

                            // Play sound for car that crashed
                            soundManager.PlaySound("crash");

                        }
                    }
                }
            }
        }

        protected override void threadFunction()
        {
            if (!shouldStart) return;
            // Update game
            if (gameTime != null)
            {
                Update();
                shouldStart = false;
            }
        }
    }
}
