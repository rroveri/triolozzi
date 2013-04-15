using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame2.Events;

namespace WindowsGame2
{
    class ScreenRenderer
    {
        public int PlayersCount { get; set; }

        private float nPoints = 27;
        public VertexPositionColorTexture[] postitVertices, lapVertices, nLapsVertices;
        public VertexPositionColorTexture[][] barVertices;

        private Vector2 texNW, texNE, texOW, texOE;
        private Vector2 initPoint = new Vector2();

        private Color currentColor;

        private float offset = 0.0f;
        private float width = 0.25f, height = 0.30f;
        private float barOffsetW, barOffsetH, barWidth, barHeight, pointLength;

        private float lapWidth = 0.15f, lapHeight = 0.1f;
        private float lapOffset = 0.1f;

        private float nLapOffset = 0.1f;
        private float nLapWidth = 0.10f, nLapHeight = 0.15f;

        public ScreenRenderer(int nPlayers)
        {

            PlayersCount = nPlayers;

            barHeight = height * 0.0675f;
            barOffsetW = width / 10;
            barOffsetH = height*0.2075f;//0.062f;
            barWidth = width*0.717f;// barOffset;
            pointLength = barWidth / nPoints;

            texNW = new Vector2(0, 0);
            texNE = new Vector2(1, 0);
            texOW = new Vector2(0, 1);
            texOE = new Vector2(1, 1);

            postitVertices = new VertexPositionColorTexture[6 * nPlayers];
            barVertices = new VertexPositionColorTexture[nPlayers][];
            for (int i = 0; i < nPlayers; i++)
            {
                barVertices[i] = new VertexPositionColorTexture[6 * (int)nPoints * 4];
            }


            lapVertices = new VertexPositionColorTexture[6];
            nLapsVertices = new VertexPositionColorTexture[6];

            for (int p = 0; p < nPlayers; p++) 
            {
                if (p == 0)
                {
                    initPoint.X = -1 + offset;
                    initPoint.Y = -1 + offset;

                    currentColor = Color.Red;
                }
                else if (p == 1)
                {
                    initPoint.X = 1 - offset - width;
                    initPoint.Y = -1 + offset;

                    currentColor = Color.Blue;
                }
                else if (p == 2)
                {
                    initPoint.X = -1 + offset;
                    initPoint.Y = 1 - offset - height;

                    currentColor = Color.Green;
                }
                else if (p == 3)
                {
                    initPoint.X = 1 - offset - width;
                    initPoint.Y = 1 - offset - height;

                    currentColor = Color.Brown;
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

                setHappyToAllPlayers();

                initPoint.X += barOffsetW;
                initPoint.Y += height - barOffsetH - barHeight;

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
                        barVertices[p][i * 6 + c].Color = currentColor;
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

            initPoint.X = -nLapWidth / 2;
            initPoint.Y = nLapOffset + lapOffset - 1;

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
    }
}
