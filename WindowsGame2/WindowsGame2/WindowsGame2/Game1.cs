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

        List<TexturePhysicsObject> carsList;
        List<DrawablePhysicsObject> bordersList;
        List<PolygonPhysicsObject> polygonsList;


        TexturePhysicsObject[] carsArray;
        DrawablePhysicsObject[] bordersArray;

        
        KeyboardState prevKeyboardState;
        Random random;

        TexturePhysicsObject redDrawable;
        TexturePhysicsObject blueDrawable;
        TexturePhysicsObject greenDrawable;

        Texture2D dummyTexture;

        List<Vector2> redTrail;

        PolygonPhysicsObject myBigObject;
        Vertices vertices;

        Rectangle dummyRectangle;

        Texture2D squaredBg;

        private bool found;

        KeyboardState ks;
        MouseState ms;
        GamePadState gps;

        Vector2 dir;
        Vector2 forceVector;

        float force;

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
            // TODO: Add your initialization logic here

            
            redTrail=new List<Vector2>();
            polygonsList = new List<PolygonPhysicsObject>();

            found = false;

            random = new Random();

            dir = new Vector2();
            forceVector =new Vector2();
            force = 1f;

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

            squaredBg = Content.Load<Texture2D>("Images/squaredBg");


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
            

            //load cars
            carsList = new List<TexturePhysicsObject>();

            redDrawable = new TexturePhysicsObject(world, Content.Load<Texture2D>("Images/penis"), new Vector2(65.0f, 40.0f),Color.Red);
            redDrawable.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            carsList.Add(redDrawable);
            blueDrawable = new TexturePhysicsObject(world, Content.Load<Texture2D>("Images/penis"), new Vector2(65.0f, 40.0f), Color.Blue);
            blueDrawable.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            carsList.Add(blueDrawable);
            greenDrawable = new TexturePhysicsObject(world, Content.Load<Texture2D>("Images/greenCarXna"), new Vector2(65.0f, 40.0f), Color.Green);
            greenDrawable.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), random.Next(50, GraphicsDevice.Viewport.Height - 50));
            carsList.Add(greenDrawable);

            carsArray = new TexturePhysicsObject[3];
            carsArray[0] = redDrawable;
            carsArray[1] = blueDrawable;
            carsArray[2] = greenDrawable;
            bordersArray = new DrawablePhysicsObject[4];
            bordersArray[0] = floor;
            bordersArray[1] = ceil;
            bordersArray[2] = leftWall;
            bordersArray[3] = rightWall;

           
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

            // TODO: Add your update logic here

            gps = GamePad.GetState(PlayerIndex.One);
            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            // Allows the game to exit
            if (gps.Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (ks.IsKeyDown(Keys.Escape))
                Exit();

            //move red car
            redDrawable._compound.LinearDamping = 1;
            redDrawable._compound.AngularDamping = 1;
            if (ks.IsKeyDown(Keys.Right) || gps.ThumbSticks.Right.X > 0)
            {
               // redDrawable.body.Rotation += 0.1f;
                redDrawable._compound.ApplyTorque(0.1f);
            }
            if (ks.IsKeyDown(Keys.Left) || gps.ThumbSticks.Right.X < 0)
            {
               // redDrawable.body.Rotation -= 0.1f;
                redDrawable._compound.ApplyTorque(-0.1f);
            }
            
           
            dir.X = (float)Math.Cos(redDrawable._compound.Rotation);
            dir.Y = (float)Math.Sin(redDrawable._compound.Rotation);
            
            forceVector = dir * force;

            if (ks.IsKeyDown(Keys.Up) || gps.ThumbSticks.Left.Y > 0)
            {
                redDrawable._compound.ApplyForce(forceVector, redDrawable._compound.WorldCenter);
            }
            if (ks.IsKeyDown(Keys.Down) || gps.ThumbSticks.Left.Y < 0)
            {
                redDrawable._compound.ApplyForce(-forceVector, redDrawable._compound.WorldCenter);
            }
            
            // compute red trail
            if (ks.IsKeyDown(Keys.F) || gps.Triggers.Right > 0)
            {
                redTrail.Add(redDrawable.Position);     
            }
            else
            {
                redTrail.Clear(); 
                found = false;
            }
            
            //create red shapes

            //min dist from the car
            int lowBound=50;
            //distance to check if there is an intersection
            float distToCheckIntersection = 5.0f;

            if (redTrail.Count>lowBound){
                
                for (int i = 0; i < redTrail.Count - 1-lowBound; i++)
                {
                    if (found)
                    {
                        //already one shape with this trail, do not draw other shapes
                        break;
                    }
                    
                    if (Vector2.Distance(redDrawable.Position, redTrail[i]) < distToCheckIntersection)
                    {
                        // intersection found
                        found = true;

                        // compute polygon vertices
                        vertices = new Vertices();

                        for (int ii = i; ii < redTrail.Count - 1; ii++)
                        {
                            if (ii % 10 == 0)
                            {
                                vertices.Add(redTrail[ii]);
                            }
                        }
                        //if the shape is a polygon, create a new object
                        if (vertices.Count > 2)
                        {
                            myBigObject = new PolygonPhysicsObject(world, vertices, dummyTexture);
                            polygonsList.Add(myBigObject);
                        }
                        
                        break;
                    }

                    
                }
            }

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
           // color.A = 50;
            batch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

         //   spriteBatch.Draw(squaredBg, Vector2.Zero,null, Color.White, 0.0f, Vector2.Zero, Vector2.One*1.8f,SpriteEffects.None,0f);

            // draw red trail
            for (int i = 1; i < redTrail.Count; i++)
            {
                Vector2 position1 = redTrail[i];
                Vector2 position2 = redTrail[i-1];
                DrawLine(spriteBatch, dummyTexture,5,Color.Red,position1,position2);
            }

            // draw cars
            for (int i = 0; i < carsArray.Length; i++)
            {
                carsArray[i].Draw(spriteBatch);
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
