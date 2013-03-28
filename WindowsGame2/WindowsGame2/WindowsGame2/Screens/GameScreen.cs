﻿using System;
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

        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public GameScreen()
        {
            polygonsList = new List<PolygonPhysicsObject>();
            cars = new List<Car>();
            playerIndexes = new List<PlayerIndex>();
            playerIndexes.Add(PlayerIndex.One); playerIndexes.Add(PlayerIndex.Two);
            playerIndexes.Add(PlayerIndex.Three); playerIndexes.Add(PlayerIndex.Four);
            random = new Random();
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
            raceTrack = Track.CreateTrack(TrackType.PenisTrack);

            prevKeyboardState = Keyboard.GetState();

            redCar = new Car(world, Color.Red);
            redCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            blueCar = new Car(world, Color.Blue);
            blueCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2 + 250);

            greenCar = new Car(world, Color.Green);
            greenCar.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2 + 500);

            cars.Add(redCar); cars.Add(blueCar); cars.Add(greenCar);

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

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            gps = GamePad.GetState(PlayerIndex.One);
            ks = Keyboard.GetState();
            PolygonPhysicsObject obstacle;

            // Allows the game to exit
            if (gps.Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                ExitScreen();

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

            // TODO: should this be executed before?
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
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

        public void DrawSprites(Camera camera)
        {

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