using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using FarseerPhysics.SamplesFramework;

namespace WindowsGame2.GameElements
{
    public class PopupMessage
    {
        private Car car;
        private float textureScale;
        private Texture2D textureBg;
        private Vector2 origin;
        public StringWriter stringWriter = new StringWriter();
        private string messageString = "1";
        
        public VertexPositionColorTexture[] bgTextureVertices;

        public bool isActive=false;
        public double timer;
        private double popupTime;
        private double popupTimeNormal;
        private double popupTimeDead;

        public Texture2D thumbsUp;
        public Texture2D thumbsDown;
        public Texture2D currentTexture;

        private Vector2 oldPos;
        private bool firstTime;

        public Texture2D rainbowTex;


        public PopupMessage(Car _car)
        {
            car = _car;
            textureScale = 0.4f;
            textureBg = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/onomatopeeBg");
            thumbsDown= GameServices.GetService<ContentManager>().Load<Texture2D>("Images/thumbs_down");
            thumbsUp = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/thumbs_up");
            rainbowTex = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/rainbow_texture");

            origin = new Vector2(textureBg.Width, textureBg.Height) * textureScale / 2f;

            stringWriter.nCharacters = 1;
            bgTextureVertices = new VertexPositionColorTexture[6];

            timer = 0.0f;
            popupTimeNormal = 1000f;
            popupTimeDead = 4000;
            popupTime = popupTimeNormal;

            currentTexture = thumbsUp;

            firstTime = true;


        }


        public void Update(GameTime gameTime)
        {

            if (isActive)
            {
                timer += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (timer > popupTime)
                {
                    disactivate();
                }
            }

            bgTextureVertices[0].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos), -0.1f);
            bgTextureVertices[1].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos + new Vector2(textureBg.Width *textureScale,0)), -0.1f);
            bgTextureVertices[2].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos + new Vector2(0,textureBg.Height * textureScale)), -0.1f);
            bgTextureVertices[3].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos), -0.1f);
            bgTextureVertices[4].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos + new Vector2(0,textureBg.Height * textureScale)), -0.1f);
            bgTextureVertices[5].Position = new Vector3(ConvertUnits.ToSimUnits(car.messageImagePos + new Vector2(textureBg.Width * textureScale,textureBg.Height * textureScale)), -0.1f);
            bgTextureVertices[0].Color = car.mColor;
            bgTextureVertices[1].Color = car.mColor;
            bgTextureVertices[2].Color = car.mColor;
            bgTextureVertices[3].Color = car.mColor;
            bgTextureVertices[4].Color = car.mColor;
            bgTextureVertices[5].Color = car.mColor;

        }

        public void activate(string newMessage, int isDead)
        {
            messageString = newMessage;

            isActive = true;
            timer = 0.0f;

            if (isDead == 1)
            {
                popupTime = popupTimeDead;
            }
            else
            {
                popupTime = popupTimeNormal;
            }

            firstTime = true;

        }

        public void disactivate(){
            isActive = false;
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            if (isActive)
            {
               // stringWriter.addString(messageString, Color.Black, 1f, ConvertUnits.ToSimUnits(car.messageImagePos), new Vector2(1, 0));

                if (firstTime)
                {
                    oldPos = car.messageImagePos;
                    firstTime = false;
                }

                Vector2 newPos = Vector2.Lerp(oldPos, car.messageImagePos,0.1f);

                spriteBatch.Draw(textureBg, newPos,
                                               null, car.mColor, 0, origin, Vector2.One * textureScale, SpriteEffects.None,
                                               0.0f);
                spriteBatch.Draw(currentTexture, newPos,
                                               null, Color.White, 0, origin, Vector2.One * textureScale, SpriteEffects.None,
                                               0.0f);
                /*
                Color transp = new Color(1, 1, 1, 0.4f);
                spriteBatch.Draw(rainbowTex, car.Position,
                                               null, transp, 0, origin, new Vector2(1,10000) * textureScale, SpriteEffects.None,
                                               0.0f);
                */
                oldPos = car.messageImagePos;
            }
        }
    }
}
