using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WindowsGame2.GameElements;
using WindowsGame2.Screens;
using FarseerPhysics;
using X2DPE;


namespace WindowsGame2
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        ScreenManager ScreenManager;
        ParticleComponent particleComponent;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            GameServices.AddService<GraphicsDeviceManager>(graphics);
            Content.RootDirectory = "Content";
           
            graphics.PreferredBackBufferHeight = (int)(1080);
            graphics.PreferredBackBufferWidth = (int)(1920);

            graphics.IsFullScreen = false;

            Window.Title = "The Drunken Dream Maker (With a Cold)";

            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            particleComponent = new ParticleComponent(this);
            Components.Add(particleComponent);

           // FrameRateCounter myFrameCounter = new FrameRateCounter(this, new Vector2(25, 25), Color.White, Color.Black);
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
            Settings.EnableDiagnostics = false;
            Settings.VelocityIterations = 6; //6
            Settings.PositionIterations = 2; //2
            Settings.ContinuousPhysics = true;
            // IsFixedTimeStep = false;

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
            GameServices.AddService<ParticleComponent>(particleComponent);

            //ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(new GameScreen(), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
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
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
