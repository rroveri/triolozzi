using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame2
{
    class ScreenRenderer
    {
        private int nPlayers;

        private float nPoints = 27;
        public VertexPositionColorTexture[] postitVertices, lapVertices, numberLapsVertices;
        public VertexPositionColorTexture[][] barVertices;

        private Vector2 texNW, texNE, texOW, texOE;
        private Vector2 initPoint = new Vector2();

        private Color currentColor;

        private float offset = 0.05f;
        private float width = 0.25f, height = 0.15f;
        private float barOffset, barWidth, barHeight = 0.025f, pointLength;

        public ScreenRenderer(int nPlayers)
        {

            this.nPlayers = nPlayers;

            barOffset = 0.03f;
            barWidth = 0.15f;// barOffset;
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
            numberLapsVertices = new VertexPositionColorTexture[6];

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
                postitVertices[p * 6 + 3].TextureCoordinate = texOW;
                postitVertices[p * 6 + 4].TextureCoordinate = texOE;
                postitVertices[p * 6 + 5].TextureCoordinate = texNE;

                setHappyToAllPlayers();

                initPoint.X += barOffset;
                initPoint.Y += height - barOffset - barHeight;

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
        }

        public void setSadToPlayer(int player)
        {
            for (int c = 0; c < 6; c++)
                postitVertices[player * 6 + c].Color = Color.Black;
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
