using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowsGame2.GameElements;
using FarseerPhysics.SamplesFramework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame2
{
    public class SneezesManager
    {

        public double timer;
        Random random;
        double nextSneezeTime;
        public RandomTrack randomTrack;
        public Fluid fluid;

        public Vector2 sneezePosition;
        public bool drawFluid;

        public bool fluidFree;

        public Camera camera;

        GraphicsDeviceManager graphics;

        SoundEffect sneezeSound;

        private bool soundPlayed;

        public SneezesManager()
        {
            timer = 0;
            random = new Random(DateTime.Now.Millisecond);
            nextSneezeTime = Math.Min( random.NextDouble()*25000,15000);
            drawFluid = false;

            fluidFree = true;


            graphics = GameServices.GetService<GraphicsDeviceManager>();

            sneezeSound = GameServices.GetService<ContentManager>().Load<SoundEffect>("Sounds/mucus/sneeze");

            soundPlayed = false;
        }

        public void Update(GameTime gameTime, List<Car> Cars)
        {
            if (fluidFree)
            {
                timer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timer > nextSneezeTime - 3000 && !soundPlayed)
                {
                    sneezeSound.Play();
                    soundPlayed = true;
                }
                if (timer > nextSneezeTime)
                {
                    int maxMiddlePoint = 0;
                    for (int i = 0; i < Cars.Count; i++)
                    {
                        if (Cars[i].currentMiddlePoint > maxMiddlePoint)
                        {
                            maxMiddlePoint = Cars[i].currentMiddlePoint;
                        }
                    }
                    fluid.shouldResetDensity = true;
                    fluid.renderPosition = ConvertUnits.ToDisplayUnits(randomTrack.curvePointsMiddle[maxMiddlePoint % randomTrack.curvePointsMiddle.Count]);

                    


                    timer = 0;
                    nextSneezeTime = Math.Min(1000, random.NextDouble() * 35000);

                    soundPlayed = false;

                    drawFluid = true;

                    fluidFree = false;
                }
            
            }

            if (fluidFree == false)
            {
                bool fluidOffScreen = true;
                for (int i=-1;i<2; i=i+1){
                    for (int j=-1;j<2; j=j+1){
                        Vector2 fluidCorner=fluid.renderPosition+new Vector2( i*fluid.renderHeight,j*fluid.renderWidth );
                        Vector2 fluidScreenPosCorner = Vector2.Transform(fluidCorner, camera.Transform);

                        if (fluidScreenPosCorner.X >= 0 && fluidScreenPosCorner.X < graphics.PreferredBackBufferWidth && fluidScreenPosCorner.Y >= 0 && fluidScreenPosCorner.Y < graphics.PreferredBackBufferHeight)
                        {
                            fluidOffScreen = false;
                        }

                    }
                }

                if (fluidOffScreen)
                {
                    fluidFree = true;
                    timer = 0;
                }
                
                 
            }

            
            

        }

    }
}
