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
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

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
        private Texture2D readyTexture;
        private bool[] readyCars = new bool[4];
        private int _carsCount;
        private int _readyCount;

        private Effect _effect;
        private Matrix _projectionMatrix, _viewMatrix;
        Viewport viewport;
        Vector2 screenCenter;


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
            readyTexture = ScreenManager.Game.Content.Load<Texture2D>("Images/SandboxScreen/sandbox_ready");

            _effect = ScreenManager.Game.Content.Load<Effect>("Shaders/PaperEffect");
            _effect.CurrentTechnique = _effect.Techniques["DoodleTechinque"];
            _effect.Parameters["alphabet"].SetValue(ScreenManager.Game.Content.Load<Texture2D>("Images/alphabet"));

           viewport = ScreenManager.GraphicsDevice.Viewport;

            _titlePosition = new Rectangle(viewport.Width / 2 - _sandboxTitle.Width / 2, 100, _sandboxTitle.Width, _sandboxTitle.Height);
            screenCenter = new Vector2(1920 / 2, 1080 / 2);
            
            SetupWorldBorders();

            _projectionMatrix = _camera;
            _viewMatrix = _camera;
        }

        private void SetupWorldBorders()
        {
            float width = ConvertUnits.ToSimUnits(1920);
            float height = ConvertUnits.ToSimUnits(1080);

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
                _cars[i].HandleInput(gameTime, (PlayerIndex)i, input);
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
            _readyCount++;
            readyCars[e.CarIndex] = true;
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, _camera);
            ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
            ScreenManager.SpriteBatch.Draw(_sandboxTitle, _titlePosition, null, Color.White);
            ScreenManager.SpriteBatch.End();

            ScreenManager.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, ScreenManager.scaleMatrix);

            for (int i = 0; i < _cars.Count; i++)
            {
                _cars[i].Draw(ScreenManager.SpriteBatch);
                if (readyCars[i])
                {
                    Vector2 pos = (screenCenter - _cars[i].Position);
                    pos.Normalize();
                    if (pos.X < 0) pos.X -= readyTexture.Width;
                    else pos.X += readyTexture.Width / 2;
                    if (pos.Y < 0) pos.Y -= readyTexture.Height;
                    else pos.Y += readyTexture.Height / 2;
                    pos += _cars[i].Position;
                    ScreenManager.SpriteBatch.Draw(readyTexture, pos, null, _cars[i].color);
                }
            }

            

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        #endregion
    }
}
