using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace WindowsGame2.GameElements
{
    public class GameInput
    {

        public float[] horizontal = new float[4], vertical = new float[4];

        public GameInput()
        {
            
        }

        public void update()
        {
            resetGameInput();

            KeyboardState ks = Keyboard.GetState();
            for (int i = 0; i < 4; i++)
            {
                GamePadState gps = GamePad.GetState((PlayerIndex)i);
                horizontal[i] = gps.ThumbSticks.Right.X;
                vertical[i] = gps.ThumbSticks.Left.Y;

                if (ks.IsKeyDown(Keys.Right))
                {
                    horizontal[i] = 1.0f;
                }
                if(ks.IsKeyDown(Keys.Left))
                {
                    horizontal[i] = -1.0f;
                }

                if (ks.IsKeyDown(Keys.Up))
                {
                    vertical[i] = 1.0f;
                }
                if (ks.IsKeyDown(Keys.Down))
                {
                    vertical[i] = -1.0f;
                }
            }
        }

        private void resetGameInput()
        {
            for (int i = 0; i < 4; i++)
            {
                horizontal[i] = 0.0f;
                vertical[i] = 0.0f;
            }
        }
    }
}
