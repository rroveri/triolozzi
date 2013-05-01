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
using FarseerPhysics.Dynamics.Contacts;
using X2DPE;
using X2DPE.Helpers;
using WindowsGame2.Events;
//using fluid;

namespace WindowsGame2.Screens

{
    class GameScreen : AbstractScreen
    {

        #region Fields

        GraphicsDevice GraphicsDevice;
        ContentManager Content;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SoundManager soundManager;

        List<PlayerIndex> playerIndexes;

        string[] paperEffects = { "redCarPos", "blueCarPos", "greenCarPos", "pinkCarPos" };

        KeyboardState prevKeyboardState;
        Random Random;
        float[] randomArray;

        KeyboardState ks;
        GameInput gameInput;

        private GameLogic Logic;

        private RankingScreen RankScreen;
        float pauseAlpha;

        private PauseMenuScreen PauseScreen;

        Effect paperEffect, screenEffect;
        ScreenRenderer screenRenderer;
        StringWriter stringWriter = new StringWriter();
        Matrix projection;
        Matrix view;

        Fluid fluid;
        Vector2 fluidPos = new Vector2(-1);

        public int PlayersCount { get; set; }

        private Texture2D dummyTexture;

        private SneezesManager mySneezesManager;

        private Physics Physics;

        VertexPositionColorTexture[] basicVert;
        int maxNumberOfTriangles = 10000;

        #endregion

        // Done
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

            RankScreen = new RankingScreen("");
            RankScreen.Accepted += RankScreenAccepted;

            PauseScreen = new PauseMenuScreen();

            playerIndexes = new List<PlayerIndex>();
            playerIndexes.Add(PlayerIndex.One); playerIndexes.Add(PlayerIndex.Two);
            playerIndexes.Add(PlayerIndex.Three); playerIndexes.Add(PlayerIndex.Four);
            Random = new Random(DateTime.Now.Millisecond);

            PlayersCount = 4;
            
            dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            byte whiteAlpha = 50;
            Color dummyTextureColor = new Color(whiteAlpha, whiteAlpha, whiteAlpha);
            dummyTexture.SetData(new Color[] { dummyTextureColor });

            mySneezesManager = new SneezesManager();
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

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Logic = new GameLogic();
            Logic.DidFinishMiniRace += DidFinishMiniRace;

            Physics = new Physics(Logic);
            Physics.LoadContent();
            gameInput = new GameInput();
            Physics.gameInput = gameInput;
            
            prevKeyboardState = Keyboard.GetState();

            mySneezesManager.randomTrack = Physics.randomRaceTrack;


            paperEffect = Content.Load<Effect>("Shaders/PaperEffect");
            paperEffect.CurrentTechnique = paperEffect.Techniques["DoodleTechinque"];
            Texture2D trailSketch = Content.Load<Texture2D>("Materials/trailSketch");
            Texture2D objectSketch = Content.Load<Texture2D>("Materials/pencil");
            Texture2D ink = Content.Load<Texture2D>("Materials/ink_texture");
            Texture2D startLine = Content.Load<Texture2D>("Materials/squares");
            Texture2D alphabet = Content.Load<Texture2D>("Images/alphabet");
            Texture2D messageBg = Content.Load<Texture2D>("Images/onomatopeeBg");
            Texture2D externalTex = Content.Load<Texture2D>("Images/external_bg");
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
            Texture2D randomTex = new Texture2D(graphics.GraphicsDevice, 16, 16);
            randomTex.SetData(randomCol);
            paperEffect.Parameters["random"].SetValue(randomTex);

            screenEffect = Content.Load<Effect>("Shaders/ScreenEffect");
            screenEffect.CurrentTechnique = screenEffect.Techniques["ScreenTechinque"];
            Texture2D postitHappy = Content.Load<Texture2D>("Images/postitHappy");
            screenEffect.Parameters["postitHappy"].SetValue(postitHappy);
            Texture2D postitSad = Content.Load<Texture2D>("Images/postitSad");
            screenEffect.Parameters["postitSad"].SetValue(postitSad);
            Texture2D postitLap = Content.Load<Texture2D>("Images/postitLap");
            screenEffect.Parameters["lap"].SetValue(postitLap);
            Texture2D numbers = Content.Load<Texture2D>("Images/numbers");
            screenEffect.Parameters["numbers"].SetValue(numbers);

            screenRenderer = new ScreenRenderer();
            Logic.DidEliminateCar += screenRenderer.setSadToPlayer;
            Logic.DidFinishLap += screenRenderer.setLap;

            basicVert = new VertexPositionColorTexture[maxNumberOfTriangles];
            for (int i = 0; i < maxNumberOfTriangles; i++) basicVert[i].TextureCoordinate = new Vector2(-1);


            fluid = new Fluid(Content,GraphicsDevice, spriteBatch);
            mySneezesManager.fluid = fluid;
        }


        public override void Unload()
        {
            base.Unload();
            GameServices.DeleteService<World>();
            GameServices.DeleteService<Camera>();
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

            Physics.gameTime = gameTime;
            Physics.gameInput.update();
            Physics.shouldStart = true;

            if (ShouldPauseGame())
            {
                ScreenManager.ShowScreen<PauseMenuScreen>();
                soundManager.PauseSong();
                return;
            }

            if (Logic.isGameOver())
            {
                // TODO (call stop on physics)
                //RankScreen.UpdateRankings(Cars);
                ScreenManager.ShowScreen<RankingScreen>();
                return;
            }

            mySneezesManager.Update(gameTime,Physics.Cars);
        }

        private void DidFinishMiniRace(object sender, MiniRaceOverEventArgs e)
        {
            screenRenderer.setHappyToAllPlayers();
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

            GraphicsDevice.Viewport = Physics.defaultViewport;

            List<Car> Cars = Physics.Cars;
            Camera cameraFollowing = Physics.cameraFollowing;
            RandomTrack randomRaceTrack = Physics.randomRaceTrack;
            List<PolygonPhysicsObject> polygonsList = Physics.polygonsList;

            Vector2 greenPosition = Vector2.Transform(Cars[0].Position, cameraFollowing.Transform);
            fluid.carPos = greenPosition;
            fluid.Update();

            //compute camera matrices
            projection = cameraFollowing.ProjectionMatrix;
            view = cameraFollowing.ViewMatrix;

            //draw 2D (!keep DepthStencilState to None in order to see shaders!)


            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, cameraFollowing.Transform);

            // Draw the race track and the starting line
            randomRaceTrack.DrawSprites(cameraFollowing, spriteBatch);


            // draw cars and their trails
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].Draw(spriteBatch);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, 130 * 2);
            }

            spriteBatch.End();
            
            Vector2 fluidScreenPosition = Vector2.Transform(mySneezesManager.sneezePosition, cameraFollowing.Transform);
            fluid.Draw(fluidScreenPosition);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            paperEffect.Parameters["Projection"].SetValue(projection);
            paperEffect.Parameters["View"].SetValue(view);

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
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].trailVertices, 0, Car.mMaximumTrailPoints * Car.paintersCount * 2);
                }
                else if (Cars[i].mTrailPoints > 0)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].trailVertices, 0, Cars[i].mTrailPoints * Car.paintersCount * 2);
                }

            }

            paperEffect.CurrentTechnique.Passes["TrailPass"].Apply();
            for (int i = 0; i < Cars.Count; i++)
            {
                if (Cars[i].burnoutCounter > 0)
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].burnoutsVertices, 0, Cars[i].burnoutCounter * 2);
            }


            //now draw 3D (shaders)

            //reset GraphicsDevice states (might be slow)


            int counter = 0;

            paperEffect.CurrentTechnique.Passes["StartLinePass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.startLineVertices, 0, 2);

            paperEffect.CurrentTechnique.Passes["BorderPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.myArray, 0, randomRaceTrack.myArray.Count() / 3);


            paperEffect.CurrentTechnique.Passes["ExternalSketchPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.verticesBordersInternal, 0, randomRaceTrack.verticesBordersInternal.Count() / 3);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, randomRaceTrack.verticesBordersExternal, 0, randomRaceTrack.verticesBordersExternal.Count() / 3);

            paperEffect.CurrentTechnique.Passes["ObjectPass"].Apply();

            // draw polygons
            for (int i = 0; i < polygonsList.Count; i++)
            {
                polygonsList[i].Draw(ref projection, ref view, cameraFollowing.Transform, ref basicVert, ref counter);
            }

            if (counter > 0)
            {

                //draw shader
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, basicVert, 0, counter);
            }

            paperEffect.CurrentTechnique.Passes["PopupMessagePass"].Apply();
            for (int i = 0; i < Cars.Count; i++)
            {
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Cars[i].message.bgTextureVertices, 0, 2);
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


            screenEffect.CurrentTechnique.Passes["PostitPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.postitVertices, 0, PlayersCount * 2);

            screenEffect.CurrentTechnique.Passes["LapPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.lapVertices, 0, 2);

            screenEffect.CurrentTechnique.Passes["NLapPass"].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.nLapsVertices, 0, 2);

            screenEffect.CurrentTechnique.Passes["BarPass"].Apply();
            for (int i = 0; i < PlayersCount; i++)
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenRenderer.barVertices[i], 0, Cars[i].score * 2);
            
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
            base.Draw(gameTime);
        }

        public void RankScreenAccepted(object sender, PlayerIndexEventArgs e)
        {
            RankScreen.Accepted -= RankScreenAccepted;
            ScreenManager.RemoveScreen(RankScreen);
            ScreenManager.QuitGame();
        }

        public void InitializeGame(int playersCount, ref int[] selectedCars, ref Texture2D[] availableCars, ref int[] selectedColors, ref Color[] availableColors)
        {
            Physics.InitializeGame(playersCount, ref selectedCars, ref availableCars, ref selectedColors, ref availableColors);
            // Call the Jean Charles
            GC.Collect();
            Physics.start(3);
        }
    }
}