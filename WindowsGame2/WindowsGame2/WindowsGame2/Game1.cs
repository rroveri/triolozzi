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


namespace WindowsGame2
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        DebugViewXNA _debugView;
        World world;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Car> cars;
        List<PlayerIndex> playerIndexes;
        List<PolygonPhysicsObject> polygonsList;
        
        KeyboardState prevKeyboardState;
        Random random;

        Car redCar;
        Car blueCar;
        Car greenCar;

        KeyboardState ks;
        GamePadState gps;

        AssetCreator assetCreator;

        Camera cameraTopLeft;
        Camera cameraTopRight;
        Camera cameraBottomLeft;

        Viewport topLeftViewport;
        Viewport topRightViewport;
        Viewport bottomLeftViewport;
        Viewport defaultViewport;

        Matrix projectionMatrix;
        Matrix halfprojectionMatrix;

        Track raceTrack;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           
            graphics.PreferredBackBufferHeight = 1000;
            graphics.PreferredBackBufferWidth = 1800;

            graphics.IsFullScreen = false;

            Window.Title = "The Drunken Dream Maker (With a Cold)";

          //  FrameRateCounter myFrameCounter = new FrameRateCounter(this, new Vector2(25, 25), Color.White, Color.Black);
           // Components.Add(myFrameCounter);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            polygonsList = new List<PolygonPhysicsObject>();
            cars = new List<Car>();
            playerIndexes = new List<PlayerIndex>();
            playerIndexes.Add(PlayerIndex.One); playerIndexes.Add(PlayerIndex.Two);
            playerIndexes.Add(PlayerIndex.Three); playerIndexes.Add(PlayerIndex.Four);
            random = new Random();
            
            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        protected override void LoadContent()
        {
            // We add to the GameServices objects that we want to be able to use accross different classes
            // without having to pass them explicitly every time.
            GameServices.AddService<GraphicsDevice>(GraphicsDevice);
            GameServices.AddService<ContentManager>(Content);

            world = new World(new Vector2(0, 0));
            GameServices.AddService<World>(world);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create a new track
            raceTrack = Track.CreateTrack(TrackType.PenisTrack);

            prevKeyboardState = Keyboard.GetState();

            redCar = new Car(world, this, Color.Red);
            //redCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            redCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            blueCar = new Car(world, this, Color.Blue);
            //blueCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            blueCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2+250);

            greenCar = new Car(world, this, Color.Green);
            //greenCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            greenCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2 + 500);

            cars.Add(redCar); cars.Add(blueCar); cars.Add(greenCar);

            assetCreator = new AssetCreator(graphics.GraphicsDevice);
            assetCreator.LoadContent(this.Content);


            defaultViewport = GraphicsDevice.Viewport;
            topLeftViewport = defaultViewport;
            topRightViewport = defaultViewport;
            bottomLeftViewport = defaultViewport;
            topLeftViewport.Width = topLeftViewport.Width / 2-1;
            topRightViewport.Width = topRightViewport.Width / 2-1;
            bottomLeftViewport.Width = bottomLeftViewport.Width / 2-1;
            topLeftViewport.Height = topLeftViewport.Height / 2-1;
            topRightViewport.Height = topRightViewport.Height / 2-1;
            bottomLeftViewport.Height = bottomLeftViewport.Height / 2-1;
            topRightViewport.X = topLeftViewport.Width+2;
            bottomLeftViewport.Y = bottomLeftViewport.Height+2;


            cameraTopLeft = new Camera(topLeftViewport, Vector2.Zero, new Vector2(topLeftViewport.Width / 2, topLeftViewport.Height / 2), 0.95f, 0.0f);
            cameraTopRight = new Camera(topRightViewport, Vector2.Zero, new Vector2(topRightViewport.Width / 2, topRightViewport.Height / 2), 0.95f, 0.0f);
            cameraBottomLeft = new Camera(bottomLeftViewport, Vector2.Zero, new Vector2(bottomLeftViewport.Width / 2, bottomLeftViewport.Height / 2), 0.95f, 0.0f);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3.0f, 1.0f, 10000f);
            halfprojectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 2.0f / 3.0f, 1.0f, 10000f);



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
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            gps = GamePad.GetState(PlayerIndex.One);
            ks = Keyboard.GetState();
            PolygonPhysicsObject obstacle;

            // Allows the game to exit
            if (gps.Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

            for (int i = 0; i < cars.Count; i++)
            {
                // Update the position of the car
                cars[i].Update(GamePad.GetState(playerIndexes[i]), ks);
                // Find an obstacle (if any) drawn by the car and add it to the scene
                obstacle = cars[i].TrailObstacle(world, assetCreator);
                if (obstacle != null)
                {
                    polygonsList.Add(obstacle);
                }
            }

            cameraTopLeft.Update(gameTime);
            cameraTopLeft.Follow(redCar, 0.0f);

            cameraTopRight.Update(gameTime);
            cameraTopRight.Follow(blueCar, 0.0f);

            cameraBottomLeft.Update(gameTime);
            cameraBottomLeft.Follow(greenCar, 0.0f);

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            GraphicsDevice.Viewport = defaultViewport;
            GraphicsDevice.Clear(Color.Black);

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


            base.Draw(gameTime);
        }

        public void DrawSpritesDebug(Camera camera)
        {

            Vector2 _screenCenter = new Vector2(camera.View.Width / 2f, camera.View.Height / 2f);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(camera.View.Width) * (1 / (float)Math.Pow(camera.Zoom, 10)),
                                                              ConvertUnits.ToSimUnits(camera.View.Height) * (1 / (float)Math.Pow(camera.Zoom, 10)), 0f, 0f,
                                                              1f);
            Matrix view = Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(camera.Source.Position) + ConvertUnits.ToSimUnits(_screenCenter) * (1 / (float)Math.Pow(camera.Zoom, 10)), 0f));
            _debugView.RenderDebugData(ref projection, ref view);
        }

        public void DrawSprites(Camera camera){

            // Draw the race track and the starting line
            raceTrack.DrawSprites(camera, spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transform);

            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw(spriteBatch);
            }
            
            // draw cars and their trails
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }
}
