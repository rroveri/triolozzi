using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame2.Events;
using Microsoft.Xna.Framework.Content;
using WindowsGame2.GameElements;

namespace WindowsGame2
{
    class ScreenRenderer
    {
        private const int kMaximumPlayers = 4;

        public int PlayersCount { get; set; }

        private float nPoints = 54;
        public VertexPositionColorTexture[] postitVertices, pijiamaVertices,lapVertices, nLapsVertices;
        public VertexPositionColorTexture[][] barVertices;
        public VertexPositionColorTexture[][] bulletVertices;

        private Vector2 texNW, texNE, texOW, texOE;
        private Vector2 initPoint = new Vector2();

        private float offset = 0.0f;
        private float width = 0.25f, height = 0.30f;
        private float barOffsetW, barOffsetH, barWidth, barHeight, pointLength;

        private float bulletOffsetW = 0.03f, bulletOffsetH = 0, bulletWidth = 0.03f, bulletHeight = 0.25f;

        private float lapWidth = 0.125f, lapHeight = 0.15f;
        private float lapOffset = 0.0f;

        private float nLapOffsetW = 0.03f, nLapOffsetH = 0.05f;
        private float nLapWidth = 0.06f, nLapHeight = 0.06f;

        Effect screenEffect;
        GraphicsDevice device;

        public ScreenRenderer()
        {

            PlayersCount = kMaximumPlayers;
            
            barHeight = height * 0.0675f;
            barOffsetW = width * 0.041f;
            barOffsetH = height * 0.0455f;
            barWidth = width*0.787f;// barOffset;
            pointLength = barWidth / nPoints;

            texNW = new Vector2(0, 0);
            texNE = new Vector2(1, 0);
            texOW = new Vector2(0, 1);
            texOE = new Vector2(1, 1);

            postitVertices = new VertexPositionColorTexture[6 * kMaximumPlayers];
            pijiamaVertices = new VertexPositionColorTexture[6 * kMaximumPlayers];
            barVertices = new VertexPositionColorTexture[kMaximumPlayers][];
            bulletVertices = new VertexPositionColorTexture[kMaximumPlayers][];
            for (int i = 0; i < kMaximumPlayers; i++)
            {
                barVertices[i] = new VertexPositionColorTexture[6 * (int)nPoints * 4];
                bulletVertices[i] = new VertexPositionColorTexture[6];
            }


            lapVertices = new VertexPositionColorTexture[6];
            nLapsVertices = new VertexPositionColorTexture[6];

            for (int p = 0; p < kMaximumPlayers; p++) 
            {
                if (p == 0)
                {
                    initPoint.X = -1 + offset;
                    initPoint.Y = -1 + offset;
                }
                else if (p == 1)
                {
                    initPoint.X = 1 - offset - width;
                    initPoint.Y = -1 + offset;
                }
                else if (p == 2)
                {
                    initPoint.X = -1 + offset;
                    initPoint.Y = 1 - offset - height;
                }
                else if (p == 3)
                {
                    initPoint.X = 1 - offset - width;
                    initPoint.Y = 1 - offset - height;
                }

                postitVertices[p * 6 + 0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
                postitVertices[p * 6 + 1].Position = new Vector3(initPoint.X + width, initPoint.Y, 0);
                postitVertices[p * 6 + 2].Position = new Vector3(initPoint.X, initPoint.Y + height, 0);

                postitVertices[p * 6 + 3].Position = new Vector3(initPoint.X + width, initPoint.Y, 0);
                postitVertices[p * 6 + 4].Position = new Vector3(initPoint.X + width, initPoint.Y + height, 0);
                postitVertices[p * 6 + 5].Position = new Vector3(initPoint.X, initPoint.Y + height, 0);

                postitVertices[p * 6 + 0].TextureCoordinate = texNW;
                postitVertices[p * 6 + 1].TextureCoordinate = texNE;
                postitVertices[p * 6 + 2].TextureCoordinate = texOW;
                postitVertices[p * 6 + 3].TextureCoordinate = texNE;
                postitVertices[p * 6 + 4].TextureCoordinate = texOE;
                postitVertices[p * 6 + 5].TextureCoordinate = texOW;

                pijiamaVertices[p * 6 + 0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
                pijiamaVertices[p * 6 + 1].Position = new Vector3(initPoint.X + width, initPoint.Y, 0);
                pijiamaVertices[p * 6 + 2].Position = new Vector3(initPoint.X, initPoint.Y + height, 0);

                pijiamaVertices[p * 6 + 3].Position = new Vector3(initPoint.X + width, initPoint.Y, 0);
                pijiamaVertices[p * 6 + 4].Position = new Vector3(initPoint.X + width, initPoint.Y + height, 0);
                pijiamaVertices[p * 6 + 5].Position = new Vector3(initPoint.X, initPoint.Y + height, 0);

                pijiamaVertices[p * 6 + 0].TextureCoordinate = texNW;
                pijiamaVertices[p * 6 + 1].TextureCoordinate = texNE;
                pijiamaVertices[p * 6 + 2].TextureCoordinate = texOW;
                pijiamaVertices[p * 6 + 3].TextureCoordinate = texNE;
                pijiamaVertices[p * 6 + 4].TextureCoordinate = texOE;
                pijiamaVertices[p * 6 + 5].TextureCoordinate = texOW;

                setHappyToAllPlayers();

                if (p == 0)
                {
                    initPoint.X += barOffsetW;
                    initPoint.Y += barOffsetH;
                }
                else if (p == 1)
                {
                    initPoint.X += width - barOffsetW - barWidth;
                    initPoint.Y += barOffsetH;
                }
                else if (p == 2)
                {
                    initPoint.X += barOffsetW;
                    initPoint.Y += height - barOffsetH - barHeight;
                }
                else if (p == 3)
                {
                    initPoint.X += width - barOffsetW - barWidth;
                    initPoint.Y += height - barOffsetH - barHeight;
                }

                for (int i = 0; i < nPoints; i++)
                {
                    initPoint.X += pointLength;
                    
                    //initPoint.X += i * pointLength;
                    //initPoint.Y += 0;

                    barVertices[p][i * 6 + 0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
                    barVertices[p][i * 6 + 1].Position = new Vector3(initPoint.X + pointLength, initPoint.Y, 0);
                    barVertices[p][i * 6 + 2].Position = new Vector3(initPoint.X, initPoint.Y + barHeight, 0);

                    barVertices[p][i * 6 + 3].Position = new Vector3(initPoint.X + pointLength, initPoint.Y, 0);
                    barVertices[p][i * 6 + 4].Position = new Vector3(initPoint.X + pointLength, initPoint.Y + barHeight, 0);
                    barVertices[p][i * 6 + 5].Position = new Vector3(initPoint.X, initPoint.Y + barHeight, 0);

                    for(int c = 0; c < 6; c++)
                        barVertices[p][i * 6 + c].Color = Color.White;
                }

                if (p == 0)
                {
                    initPoint.X = -1 + width + bulletOffsetW;
                    initPoint.Y = -1 + bulletOffsetH;
                }
                else if (p == 1)
                {
                    initPoint.X = 1 - width - bulletOffsetW - bulletOffsetW;
                    initPoint.Y = -1 + bulletOffsetH;
                }
                else if (p == 2)
                {
                    initPoint.X = -1 + width + bulletOffsetW;
                    initPoint.Y = 1 - bulletOffsetH - bulletHeight;
                }
                else if (p == 3)
                {
                    initPoint.X = 1 - width - bulletOffsetW - bulletOffsetW;
                    initPoint.Y = 1 - bulletOffsetH - bulletHeight;
                }

                bulletVertices[p][0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
                bulletVertices[p][1].Position = new Vector3(initPoint.X + bulletWidth, initPoint.Y, 0);
                bulletVertices[p][2].Position = new Vector3(initPoint.X, initPoint.Y + bulletHeight, 0);

                bulletVertices[p][3].Position = new Vector3(initPoint.X + bulletWidth, initPoint.Y, 0);
                bulletVertices[p][4].Position = new Vector3(initPoint.X + bulletWidth, initPoint.Y + bulletHeight, 0);
                bulletVertices[p][5].Position = new Vector3(initPoint.X, initPoint.Y + bulletHeight, 0);

                if (p > 1)
                {
                    bulletVertices[p][0].TextureCoordinate = texNW;
                    bulletVertices[p][1].TextureCoordinate = texNE;
                    bulletVertices[p][2].TextureCoordinate = texOW;
                    bulletVertices[p][3].TextureCoordinate = texNE;
                    bulletVertices[p][4].TextureCoordinate = texOE;
                    bulletVertices[p][5].TextureCoordinate = texOW;
                }
                else
                {
                    bulletVertices[p][0].TextureCoordinate = texOW;
                    bulletVertices[p][1].TextureCoordinate = texOE;
                    bulletVertices[p][2].TextureCoordinate = texNW;
                    bulletVertices[p][3].TextureCoordinate = texOE;
                    bulletVertices[p][4].TextureCoordinate = texNE;
                    bulletVertices[p][5].TextureCoordinate = texNW;
                }
            }

            initPoint.X = -lapWidth / 2;
            initPoint.Y = lapOffset - 1;

            lapVertices[0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
            lapVertices[1].Position = new Vector3(initPoint.X + lapWidth, initPoint.Y, 0);
            lapVertices[2].Position = new Vector3(initPoint.X, initPoint.Y + lapHeight, 0);

            lapVertices[3].Position = new Vector3(initPoint.X + lapWidth, initPoint.Y, 0);
            lapVertices[4].Position = new Vector3(initPoint.X + lapWidth, initPoint.Y + lapHeight, 0);
            lapVertices[5].Position = new Vector3(initPoint.X, initPoint.Y + lapHeight, 0);

            lapVertices[0].TextureCoordinate = texNW;
            lapVertices[1].TextureCoordinate = texNE;
            lapVertices[2].TextureCoordinate = texOW;
            lapVertices[3].TextureCoordinate = texNE;
            lapVertices[4].TextureCoordinate = texOE;
            lapVertices[5].TextureCoordinate = texOW;

            initPoint.X = -lapWidth / 2 + nLapOffsetW;
            initPoint.Y = lapOffset - 1 + nLapOffsetH;

            nLapsVertices[0].Position = new Vector3(initPoint.X, initPoint.Y, 0);
            nLapsVertices[1].Position = new Vector3(initPoint.X + nLapWidth, initPoint.Y, 0);
            nLapsVertices[2].Position = new Vector3(initPoint.X, initPoint.Y + nLapHeight, 0);

            nLapsVertices[3].Position = new Vector3(initPoint.X + nLapWidth, initPoint.Y, 0);
            nLapsVertices[4].Position = new Vector3(initPoint.X + nLapWidth, initPoint.Y + nLapHeight, 0);
            nLapsVertices[5].Position = new Vector3(initPoint.X, initPoint.Y + nLapHeight, 0);

            nLapsVertices[0].TextureCoordinate = texNW / 3;
            nLapsVertices[1].TextureCoordinate = texNE / 3;
            nLapsVertices[2].TextureCoordinate = texOW / 3;
            nLapsVertices[3].TextureCoordinate = texNE / 3;
            nLapsVertices[4].TextureCoordinate = texOE / 3;
            nLapsVertices[5].TextureCoordinate = texOW / 3;

            LoadScreenEffect();
            device = GameServices.GetService<GraphicsDeviceManager>().GraphicsDevice;
        }

        public void SetColor(Color color, int playerIndex)
        {
            for (int i = 0; i < nPoints; i++)
            {
                for (int c = 0; c < 6; c++)
                {
                    barVertices[playerIndex][i * 6 + c].Color = color;
                    pijiamaVertices[playerIndex * 6 + c].Color = color;
                    bulletVertices[playerIndex][c].Color = color;
                }
            }
        }

        public void setSadToPlayer(object sender, EliminatedCarEventArgs e)
        {
            
            for (int c = 0; c < 6; c++)
                postitVertices[e.EliminatedCarIndex * 6 + c].Color = Color.Black;
        }

        public void setLap(object sender, FinishedLapEventArgs e)
        {
            int lap = e.LapNumber;
            int y = lap / 3;
            int x = lap % 3;
            nLapsVertices[0].TextureCoordinate.X = 0.33f * x;
            nLapsVertices[0].TextureCoordinate.Y = 0.33f * y;
            nLapsVertices[1].TextureCoordinate.X = 0.33f * (x + 1);
            nLapsVertices[1].TextureCoordinate.Y = 0.33f * y;
            nLapsVertices[2].TextureCoordinate.X = 0.33f * x;
            nLapsVertices[2].TextureCoordinate.Y = 0.33f * (y + 1);
            nLapsVertices[3].TextureCoordinate.X = 0.33f * (x + 1);
            nLapsVertices[3].TextureCoordinate.Y = 0.33f * y;
            nLapsVertices[4].TextureCoordinate.X = 0.33f * (x + 1);
            nLapsVertices[4].TextureCoordinate.Y = 0.33f * (y + 1);
            nLapsVertices[5].TextureCoordinate.X = 0.33f * x;
            nLapsVertices[5].TextureCoordinate.Y = 0.33f * (y + 1);
        }

        public void setHappyToPlayer(int player)
        {
            for (int c = 0; c < 6; c++)
                postitVertices[player * 6 + c].Color = Color.White;
        }

        public void setHappyToAllPlayers()
        {
            for (int i = 0; i < postitVertices.Count(); i++)
                    postitVertices[i].Color = Color.White;
        }

        public void setBulletShotToPlayer(int player)
        {
            for (int c = 0; c < 6; c++)
                bulletVertices[player][c].Color = Color.White;
        }

        public void setBulletNotShotToAllPlayers()
        {
            for (int player = 0; player < kMaximumPlayers; player++)
                for (int c = 0; c < 6; c++)
                    bulletVertices[player][c].Color = barVertices[player][0].Color;
        }

        public void drawHUD(ref List<Car> Cars)
        {
            if (PlayersCount >= 1)
            {
                screenEffect.CurrentTechnique.Passes["PostitPassNW"].Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, postitVertices, 0, 2);
                if (postitVertices[0].Color != Color.Black)
                {
                    screenEffect.CurrentTechnique.Passes["PigiamaPassNW"].Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, pijiamaVertices, 0, 2);
                }
            }

            if (PlayersCount >= 2)
            {
                screenEffect.CurrentTechnique.Passes["PostitPassNE"].Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, postitVertices, 6, 2);
                if (postitVertices[6].Color != Color.Black)
                {
                    screenEffect.CurrentTechnique.Passes["PigiamaPassNE"].Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, pijiamaVertices, 6, 2);
                }
            }

            if (PlayersCount >= 3)
            {
                screenEffect.CurrentTechnique.Passes["PostitPassSW"].Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, postitVertices, 12, 2);
                if (postitVertices[12].Color != Color.Black)
                {
                    screenEffect.CurrentTechnique.Passes["PigiamaPassSW"].Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, pijiamaVertices, 12, 2);
                }
            }

            if (PlayersCount >= 4)
            {
                screenEffect.CurrentTechnique.Passes["PostitPassSE"].Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, postitVertices, 18, 2);
                if (postitVertices[18].Color != Color.Black)
                {
                    screenEffect.CurrentTechnique.Passes["PigiamaPassSE"].Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, pijiamaVertices, 18, 2);
                }
            }

            screenEffect.CurrentTechnique.Passes["LapPass"].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, lapVertices, 0, 2);

            screenEffect.CurrentTechnique.Passes["NLapPass"].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, nLapsVertices, 0, 2);

            screenEffect.CurrentTechnique.Passes["BarPass"].Apply();
            for (int i = 0; i < Cars.Count(); i++)
            {
                device.DrawUserPrimitives(PrimitiveType.TriangleList, barVertices[i], 0, Cars[i].score * 2);
            }

            screenEffect.CurrentTechnique.Passes["PencilPass"].Apply();
            for (int i = 0; i < Cars.Count(); i++)
            {
                device.DrawUserPrimitives(PrimitiveType.TriangleList, bulletVertices[i], 0, 2);
            }
            
        }

        private void LoadScreenEffect()
        {
            ContentManager Content = GameServices.GetService<ContentManager>();

            screenEffect = Content.Load<Effect>("Shaders/ScreenEffect");
            screenEffect.CurrentTechnique = screenEffect.Techniques["ScreenTechinque"];

            Texture2D postitHappy = Content.Load<Texture2D>("Images/postitHappy");
            screenEffect.Parameters["postitHappy"].SetValue(postitHappy);

            Texture2D postitSad = Content.Load<Texture2D>("Images/postitSad");
            screenEffect.Parameters["postitSad"].SetValue(postitSad);

            Texture2D postitLap = Content.Load<Texture2D>("Images/postitBiancoLap");
            screenEffect.Parameters["lap"].SetValue(postitLap);

            Texture2D numbers = Content.Load<Texture2D>("Images/numbers");
            screenEffect.Parameters["numbers"].SetValue(numbers);

            Texture2D pencil = Content.Load<Texture2D>("Images/pencil");
            screenEffect.Parameters["pencil"].SetValue(pencil);

            // Load and set happy post it for each player
            screenEffect.Parameters["postitHappy_NW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitHappy_NW"));
            screenEffect.Parameters["postitHappy_NE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitHappy_NE"));
            screenEffect.Parameters["postitHappy_SW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitHappy_SW"));
            screenEffect.Parameters["postitHappy_SE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitHappy_SE"));

            // Load and set sad post it for each player
            screenEffect.Parameters["postitSad_NW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitSad_NW"));
            screenEffect.Parameters["postitSad_NE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitSad_NE"));
            screenEffect.Parameters["postitSad_SW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitSad_SW"));
            screenEffect.Parameters["postitSad_SE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/postitSad_SE"));

            // Load and set pijama post it for each player
            screenEffect.Parameters["pigiama_NW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/pigiamaNW"));
            screenEffect.Parameters["pigiama_NE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/pigiamaNE"));
            screenEffect.Parameters["pigiama_SW"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/pigiamaSW"));
            screenEffect.Parameters["pigiama_SE"].SetValue(Content.Load<Texture2D>("Images/PlayerPostits/pigiamaSE"));
        }
    }
}
