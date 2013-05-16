using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WindowsGame2.GameElements;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using System.ComponentModel;

namespace WindowsGame2.Screens
{
    public class SandboxScreen : AbstractScreen
    {
        #region Fields

        private World _world;
        private Matrix _camera;

        private Texture2D _backgroundTexture;
        private Texture2D _sandboxTitle;
        private Rectangle _titlePosition;

        private List<SandboxCar> _cars;
        private List<Tuple<Texture2D, Color>> _gameCars;
        private const string message = "ready";
        private int _carsCount;
        private int _readyCount;

        private StringWriter _writer;
        private Effect _effect;
        private Matrix _projectionMatrix, _viewMatrix;
        Viewport viewport;


        #endregion

        #region Initialization

        /// <summary>
        /// Initialize a sandbox screen with the
        /// specified cars, colors and positions.
        /// </summary>
        public SandboxScreen(List<Tuple<Texture2D, Color>> cars, Rectangle[] positions)
        {
            _world = new World(Vector2.Zero);
            _camera = Matrix.CreateScale(Vector3.One) * Matrix.CreateTranslation(Vector3.Zero);
            _cars = new List<SandboxCar>(cars.Count);
            _gameCars = cars;
            _carsCount = cars.Count;
            _readyCount = 0;
            _writer = new StringWriter();

            Vector2 v;
            for (int i = 0; i < cars.Count; i++)
            {
                _cars.Add(new SandboxCar(_world, cars[i].Item1, cars[i].Item2));
                
                // Position the car where it was on the screen before
                // TODO: still not right...
                v = new Vector2(positions[i].Center.X, positions[i].Center.Y);
                _cars[i].Position = Vector2.Transform(v, Matrix.Invert(_camera));
                // Make the car point upwards
                _cars[i]._compound.Rotation = -(float)Math.PI/2;

                _cars[i].OnReadyToPlay += CarReadyToPlay;
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            _backgroundTexture = ScreenManager.Game.Content.Load<Texture2D>("Images/bgNew");
            _sandboxTitle = ScreenManager.Game.Content.Load<Texture2D>("Images/SandboxScreen/sandbox_title");

            _effect = ScreenManager.Game.Content.Load<Effect>("Shaders/PaperEffect");
            _effect.CurrentTechnique = _effect.Techniques["DoodleTechinque"];
            _effect.Parameters["alphabet"].SetValue(ScreenManager.Game.Content.Load<Texture2D>("Images/alphabet"));

           viewport = ScreenManager.GraphicsDevice.Viewport;

            _titlePosition = new Rectangle(viewport.Width / 2 - _sandboxTitle.Width / 2, 100, _sandboxTitle.Width, _sandboxTitle.Height);
            
            SetupWorldBorders(viewport);

            // TODO: adjust matrices
            _projectionMatrix = _camera; //Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(viewport.Width) * (1 / (float)Math.Pow(Zoom, 10)),
                //                            ConvertUnits.ToSimUnits(viewport.Height) * (1 / (float)Math.Pow(Zoom, 10)), 0f, 0f, 1f);
            _viewMatrix = _camera; // Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(objectPosition) + ConvertUnits.ToSimUnits(_screenCenter) * (1 / (float)Math.Pow(Zoom, 10)), 0f));
        }

        private void SetupWorldBorders(Viewport viewport)
        {
            float width = ConvertUnits.ToSimUnits(viewport.Width);
            float height = ConvertUnits.ToSimUnits(viewport.Height);

            Vertices borders = new Vertices(4);
            borders.Add(Vector2.Zero);
            borders.Add(new Vector2(0, height));
            borders.Add(new Vector2(width, height));
            borders.Add(new Vector2(width, 0));

            Body anchor = BodyFactory.CreateLoopShape(_world, borders);
            anchor.CollisionCategories = Category.All;
            anchor.CollidesWith = Category.All;
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            for (int i = 0; i < _cars.Count; i++)
            {
                _cars[i].Update(gameTime, (PlayerIndex)i);
            }

            _world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));

            if (_readyCount == _carsCount)
            {
                _readyCount = -1;
                ScreenManager.ShowScreen<LoadingScreen>();
                ScreenManager.GetScreen<LoadingScreen>().SetBusy();

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += StartGame;
                worker.RunWorkerCompleted += StartGameCompleted;
                worker.RunWorkerAsync();
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            base.HandleInput(gameTime, input);

            for (int i = 0; i < _cars.Count; i++)
            {
                _cars[i].HandleInput(gameTime, input);
            }
        }

        private void StartGame(object sender, DoWorkEventArgs e)
        {
            ScreenManager.AddScreen(new GameScreen(), null, false);
            ScreenManager.GetScreen<GameScreen>().InitializeGame(_carsCount, ref _gameCars);
        }

        private void StartGameCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScreenManager.GetScreen<LoadingScreen>().SetReady();
        }

        private void CarReadyToPlay(object sender, ReadyToPlayEventArgs e)
        {



            Vector3 pos3d = new Vector3( _cars[e.CarIndex]._compound.Position,1);
            Vector3.Transform(pos3d, _camera);
            Vector2 pos2d = new Vector2(pos3d.X,pos3d.Y);
            pos2d = new Vector2(pos2d.X/viewport.Width *2-1,pos2d.Y/viewport.Height *2 -1);
            _writer.addString(message, Color.Black, 20f, pos2d, _cars[e.CarIndex]._direction);
            _readyCount++;
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, _camera);
            ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            ScreenManager.SpriteBatch.Draw(_sandboxTitle, _titlePosition, null, Color.White);

            for (int i = 0; i < _cars.Count; i++)
            {
                _cars[i].Draw(ScreenManager.SpriteBatch);
            }

            // TODO: ask for help...?
            _effect.CurrentTechnique.Passes["AlphabetPass"].Apply();
            _effect.Parameters["Projection"].SetValue(Matrix.Identity);
            _effect.Parameters["View"].SetValue(Matrix.Identity);
            ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _writer.stringVertices, 0, _writer.stringVertices.Count() / 3);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        #endregion
    }
}
