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
using FarseerPhysics;


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
        List<DrawablePhysicsObject> bordersList;
        List<PolygonPhysicsObject> polygonsList;

        DrawablePhysicsObject[] bordersArray;

        
        KeyboardState prevKeyboardState;
        Random random;

        Car redCar;
        Car blueCar;
        Car greenCar;

        Texture2D dummyTexture;

        Texture2D squaredBg;

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

        List<Vector2> backgrounds;

        float bgScale;
        float trackWidth;
        
        Matrix projection;
        Matrix view;
        Vector2 _screenCenter;

        BasicEffect polygonsColorShader;

        Body _anchor;

        VertexPositionColor[] basicVert;
        short[] triangleListIndices;

        int maxNumberOfTriangles = 10000;

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

            bgScale = 0.9f;
            backgrounds = new List<Vector2>();

            _screenCenter = new Vector2();

            Settings.EnableDiagnostics = false;
            Settings.VelocityIterations = 1; //6
            Settings.PositionIterations = 1; //2
            Settings.ContinuousPhysics = false;
           // IsFixedTimeStep = false;


            basicVert = new VertexPositionColor[maxNumberOfTriangles];
            triangleListIndices = new short[maxNumberOfTriangles * 3];


            
            base.Initialize();
        }


        void rightTopTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;
            
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);  
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX,squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY - height / 4);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);
            
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        void rightBottomTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;
            
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width*bgScale- width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY + height / 4);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);
            
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        void leftBottomTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;
            
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY + height / 4);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);
            
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        void leftTopTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;
            
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);          
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width/2, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(width/4 + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);           
            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY - height / 4);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);
            
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }
        void bottomTopTile(int cellX, int cellY)
        {
          
            float width = 800f;
            

            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width/2, squaredBg.Height * bgScale), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width/2, squaredBg.Height * bgScale), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);
            
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));
             
        }
        void leftRightTile(int cellX, int cellY)
        {
            float height = 800.0f;

            
            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);

             
            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));
            
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

            trackWidth = squaredBg.Height - 30;


            // TODO: use this.Content to load your game content here

            world = new World(new Vector2(0,0));
            prevKeyboardState = Keyboard.GetState();
            
            //load walls
            bordersList = new List<DrawablePhysicsObject>();

            
            //create track
            leftRightTile(0, 0);
            leftRightTile(1, 0);
            leftTopTile(2, 0);
            rightBottomTile(2, -1);
            leftTopTile(3, -1);
            leftBottomTile(3, -2);
            rightTopTile(2, -2);
            bottomTopTile(2, -3);
            leftBottomTile(2, -4);
            leftRightTile(1, -4);
            leftRightTile(0, -4);
            rightBottomTile(-1, -4);
            bottomTopTile(-1, -3);
            leftTopTile(-1, -2);
            rightTopTile(-2, -2);
            leftBottomTile(-2, -3);
            rightBottomTile(-3, -3);
            bottomTopTile(-3, -2);
            bottomTopTile(-3, -1);
            bottomTopTile(-3, 0);
            bottomTopTile(-3, 1);
            rightTopTile(-3,2);
            leftTopTile(-2,2);
            bottomTopTile(-2, 1);
            bottomTopTile(-2, 0);
            rightBottomTile(-2,-1);
            leftBottomTile(-1,-1);
            rightTopTile(-1, 0);

            for (int i = 0; i < bordersList.Count; i++)
            {
                bordersList[i].body.CollisionGroup = -1;
            }
            




            polygonsColorShader = new BasicEffect(GraphicsDevice);
            polygonsColorShader.VertexColorEnabled = true;


            redCar = new Car(world, this, Color.Red, polygonsColorShader);
            //redCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            redCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            blueCar = new Car(world, this, Color.Blue, polygonsColorShader);
            //blueCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            blueCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2+250);

            greenCar = new Car(world, this, Color.Green, polygonsColorShader);
            //greenCar.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            greenCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2 + 500);

            cars.Add(redCar); cars.Add(blueCar); cars.Add(greenCar);

            bordersArray = new DrawablePhysicsObject[4];
          //  bordersArray[0] = floor;
          //  bordersArray[1] = ceil;
          //  bordersArray[2] = leftWall;
          //  bordersArray[3] = rightWall;

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


         //   cameraTopLeft = new Camera(defaultViewport, Vector2.Zero, new Vector2(defaultViewport.Width / 2, defaultViewport.Height / 2), 0.95f, 0.0f);
            cameraTopLeft = new Camera(topLeftViewport, Vector2.Zero, new Vector2(topLeftViewport.Width / 2, topLeftViewport.Height / 2), 0.95f, 0.0f);
            cameraTopRight = new Camera(topRightViewport, Vector2.Zero, new Vector2(topRightViewport.Width / 2, topRightViewport.Height / 2), 0.95f, 0.0f);
            cameraBottomLeft = new Camera(bottomLeftViewport, Vector2.Zero, new Vector2(bottomLeftViewport.Width / 2, bottomLeftViewport.Height / 2), 0.95f, 0.0f);

            cameraTopLeft.Follow(redCar, 0.0f);
            cameraTopRight.Follow(blueCar, 0.0f);
            cameraBottomLeft.Follow(greenCar, 0.0f);


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




            /*
            float halfWidth = ConvertUnits.ToSimUnits(defaultViewport.Width) / 2f ;
            float halfHeight = ConvertUnits.ToSimUnits(defaultViewport.Height) / 2f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, -halfHeight));
            borders.Add(new Vector2(-halfWidth, -halfHeight));


            _anchor = BodyFactory.CreateLoopShape(world, borders);
            _anchor.BodyType = BodyType.Static;
            */
  
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
                if (cars[i].TrailObstacle(world))
                {
                    obstacle = cars[i].TrailIntersection(world);
                    if (obstacle != null)
                    {
                        polygonsList.Add(obstacle);
                    }
                }
               
            }

            cameraTopLeft.Update(gameTime);
            cameraTopRight.Update(gameTime);
            cameraBottomLeft.Update(gameTime);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
          // world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
           // world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            


            //GraphicsDevice.Clear(Color.White);

            GraphicsDevice.Viewport = defaultViewport;
            GraphicsDevice.Clear(Color.White);

            GraphicsDevice.Viewport = topLeftViewport;
            //if not debug
            DrawSprites(cameraTopLeft);
            //if debug view
          //  DrawSpritesDebug(cameraTopLeft);

            
            GraphicsDevice.Viewport = topRightViewport;
            //if not debug
            DrawSprites(cameraTopRight);
            //if debug
          //  DrawSpritesDebug(cameraTopRight);

            GraphicsDevice.Viewport = bottomLeftViewport;
            //if not debug
            DrawSprites(cameraBottomLeft);
            //if debug
        //  DrawSpritesDebug(cameraBottomLeft);
            

            base.Draw(gameTime);
        }



        public void DrawSpritesDebug(Camera camera)
        {

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            //draw debug
            _debugView.RenderDebugData(ref projection, ref view);
        }

        public void DrawSprites(Camera camera){

            //compute camera matrices
            projection = camera.ProjectionMatrix;
            view = camera.ViewMatrix;

            
            //draw 2D (!keep DepthStencilState to None in order to see shaders!)

            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Transform);

            // draw backgrounds
            for (int i = 0; i < backgrounds.Count; i++)
            {
                spriteBatch.Draw(squaredBg, backgrounds[i], null, Color.White, 0.0f, Vector2.Zero, Vector2.One * bgScale, SpriteEffects.None, 1f);
            }

            //draw starting line
            spriteBatch.Draw(dummyTexture, new Vector2(squaredBg.Width / 2 * bgScale-100, 0), null, Color.Yellow, 0.0f, Vector2.Zero, new Vector2(100, squaredBg.Height*bgScale), SpriteEffects.None, 1f);


            // draw walls
            for (int i = 0; i < bordersList.Count; i++)
            {
                bordersList[i].Draw(spriteBatch);
            }

            // draw cars and their trails
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].Draw(spriteBatch);
            }

            spriteBatch.End();


            //now draw 3D (shaders)

            //reset GraphicsDevice states (might be slow)
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            int counter = 0;
            
            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw( ref projection, ref view, camera.Transform, ref basicVert,ref counter);
            }

            if (counter > 0)
            {
                polygonsColorShader.Projection = projection;
                polygonsColorShader.View = view;
                polygonsColorShader.CurrentTechnique.Passes[0].Apply();

                //draw shader

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, counter);
            }

        }
    }
}
