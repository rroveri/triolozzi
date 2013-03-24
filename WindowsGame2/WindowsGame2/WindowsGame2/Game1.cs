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

namespace WindowsGame2
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        World world;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Car> cars;
        List<PlayerIndex> playerIndexes;
        List<DrawablePhysicsObject> bordersList;
        List<PolygonPhysicsObject> polygonsList;

        DrawablePhysicsObject[] bordersArray;

        
        KeyboardState prevKeyboardState;
        Random random;

        Car redCar;

        Texture2D dummyTexture;

        Texture2D squaredBg;

        KeyboardState ks;
        GamePadState gps;
        
        Vector2[] redTrailArray;
        int maxTrailPoints;
        int redTrailCounter;
        bool redTrailLoop;
        bool showRedTrail;

        car[] carStructsArray;

        AssetCreator assetCreator;

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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });

            squaredBg = Content.Load<Texture2D>("Images/squaredBg2");


            // TODO: use this.Content to load your game content here

            world = new World(new Vector2(0,0));
            prevKeyboardState = Keyboard.GetState();
            
            //load walls
            bordersList = new List<DrawablePhysicsObject>();
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Images/triangle"), new Vector2(GraphicsDevice.Viewport.Width, 100.0f), 1000.0f, Color.Black);
            floor.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Images/triangle"), new Vector2(GraphicsDevice.Viewport.Width, 100.0f), 1000.0f, Color.Black);
            ceil.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, 0);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Images/triangle"), new Vector2(100.0f, GraphicsDevice.Viewport.Height - 201), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height/2.0f);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Images/triangle"), new Vector2(100.0f, GraphicsDevice.Viewport.Height - 201), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(0, GraphicsDevice.Viewport.Height / 2.0f);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);

            redCar = new Car(world, this, Color.Red);
            redCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));

            Car blueCar = new Car(world, this, Color.Blue);
            blueCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));

            Car greenCar = new Car(world, this, Color.Green);
            greenCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));

            cars.Add(redCar); cars.Add(blueCar); cars.Add(greenCar);

            bordersArray = new DrawablePhysicsObject[4];
            bordersArray[0] = floor;
            bordersArray[1] = ceil;
            bordersArray[2] = leftWall;
            bordersArray[3] = rightWall;

            car redCarStruct;
            redCarStruct.carBody = redDrawable._compound;
            redCarStruct.carTexture = Content.Load<Texture2D>("Images/penis");
            redCarStruct.carColor = Color.Red;
            car blueCarStruct;
            blueCarStruct.carBody = blueDrawable._compound;
            blueCarStruct.carTexture = Content.Load<Texture2D>("Images/penis");
            blueCarStruct.carColor = Color.Blue;
            car greenCarStruct;
            greenCarStruct.carBody = greenDrawable._compound;
            greenCarStruct.carTexture = Content.Load<Texture2D>("Images/greenCarXna");
            greenCarStruct.carColor = Color.Green;

            carStructsArray = new car[3];
            carStructsArray[0] = redCarStruct;
            carStructsArray[1] = blueCarStruct;
            carStructsArray[2] = greenCarStruct;

            redDrawable._compound.LinearDamping = 1;
            redDrawable._compound.AngularDamping = 1;

            assetCreator = new AssetCreator(graphics.GraphicsDevice);
            assetCreator.LoadContent(this.Content);
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
                obstacle = cars[i].TrailObstacle(world);
                if (obstacle != null)
                {
                    polygonsList.Add(obstacle);
                }
            }

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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // draw cars and their trails
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].Draw(spriteBatch);
            }

            // draw walls
            for (int i = 0; i < bordersArray.Length; i++)
            {
                bordersArray[i].Draw(spriteBatch);
            }

            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
