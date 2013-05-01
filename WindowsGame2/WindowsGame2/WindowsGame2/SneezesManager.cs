using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowsGame2.GameElements;
using FarseerPhysics.SamplesFramework;

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

        public SneezesManager()
        {
            timer = 0;
            random = new Random(DateTime.Now.Millisecond);
            nextSneezeTime = random.NextDouble()*10000;
            drawFluid = false;
        }

        public void Update(GameTime gameTime, List<Car> Cars)
        {
            timer += gameTime.ElapsedGameTime.TotalMilliseconds;
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
                sneezePosition = ConvertUnits.ToDisplayUnits(randomTrack.curvePointsMiddle[maxMiddlePoint % randomTrack.curvePointsMiddle.Count]);
                timer = 0;
                nextSneezeTime = random.NextDouble() * 10000;
                //fluid.resetDensity();

                drawFluid = true;
            }

        }

    }
}
