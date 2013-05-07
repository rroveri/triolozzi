using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace WindowsGame2
{
    public class CounterIndicator
    {
        public Vector2 position;
        private Texture2D texture3;
        private Texture2D texture2;
        private Texture2D texture1;
        private Texture2D textureGo;
        public Texture2D currentTexture;
        public Vector2 outPosition;
        public Vector2 inPosition;
        public Vector2 currentPostion;
        private GraphicsDeviceManager _graphicsDevice;

        public float textureScale;

        SoundEffect bip;
        SoundEffect bap;

        public int oldSound;

        public CounterIndicator()
        {

            texture3 = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Counter/countDown3");
            texture2 = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Counter/countDown2");
            texture1 = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Counter/countDown1");
            textureGo = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/Counter/countDownGO");

            currentTexture = texture3;

            _graphicsDevice = GameServices.GetService<GraphicsDeviceManager>();

            

            textureScale = 0.4f;
            float horizontalDisplacement = currentTexture.Width / 2f * textureScale;
            float verticalDisplacement = currentTexture.Height*textureScale;

            outPosition = new Vector2(_graphicsDevice.PreferredBackBufferWidth / 2f-verticalDisplacement/2f, -verticalDisplacement);
            inPosition = new Vector2(_graphicsDevice.PreferredBackBufferWidth / 2f - verticalDisplacement / 2f, 0);
            currentPostion = outPosition;

            bip = GameServices.GetService<ContentManager>().Load<SoundEffect>("Sounds/bip_converted");
            bap = GameServices.GetService<ContentManager>().Load<SoundEffect>("Sounds/bap_converted");

            oldSound = -1;

        }

        public void changeTexture(int newTex, bool playSound)
        {
            if (newTex == 3)
            {
                currentTexture = texture3;
                if (playSound && oldSound != 3)
                {
                    oldSound = 3;
                    bip.Play(1,1f,0);
                }
            }
            else if (newTex == 2)
            {
                currentTexture = texture2;
                if (playSound && oldSound != 2)
                {
                    oldSound = 2;
                    bip.Play(1, 1f, 0);
                }
            }
            else if (newTex == 1)
            {
                currentTexture = texture1;
                if (playSound && oldSound != 1)
                {
                    oldSound = 1;
                    bip.Play(1, 1f, 0);
                }
            }
            else if (newTex == 0)
            {
                currentTexture = textureGo;
                if (playSound && oldSound != 0)
                {
                    oldSound = 0;
                    bap.Play(1,1,0);
                }
            }
        }

        public void enter()
        {
            currentPostion = Vector2.Lerp(currentPostion, inPosition, 0.1f);
        }

        public void exit()
        {
            currentPostion = Vector2.Lerp(currentPostion, outPosition, 0.1f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(currentTexture, currentPostion, null, Color.White, 0, Vector2.Zero, new Vector2(textureScale, textureScale), SpriteEffects.None, 0.0f);
        }
    }
}
