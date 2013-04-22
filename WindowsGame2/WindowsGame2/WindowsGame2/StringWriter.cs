using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame2
{
    class StringWriter
    {
        private int nCharacters = 500;

        public VertexPositionColorTexture[] stringVertices;

        private int index;

        private Vector2 initPosition, upDirection, texCoords, vecDx, vecDy;
        private Vector3 initPosition3d, upDirection3d, direction3d;
        private float texDx, texDy;
        private int nCharX = 10, nCharY = 4;

        private char[,] chars;

        public StringWriter()
        {
            stringVertices = new VertexPositionColorTexture[6 * nCharacters];

            index = 0;

            texDx = 1f / nCharX;
            texDy = 1f / nCharY;

            chars = new char[nCharX,nCharY];
            initPosition = new Vector2();
            upDirection = new Vector2();
            texCoords = new Vector2();

            initPosition = new Vector2(-0.2f);
            upDirection = new Vector2(-0.2f);
            texCoords = new Vector2(-0.2f);

            vecDx = new Vector2(texDx, 0);
            vecDy = new Vector2(0, texDy);

            initChars();
        }

        public void addString(string sentence, Color color, float fontSize, Vector2 position, Vector2 direction)
        {
            direction.Normalize();
          
            direction *= fontSize;
            upDirection3d.X = -direction.Y;
            upDirection3d.Y = direction.X;

            direction3d.X = direction.X;
            direction3d.Y = direction.Y;

            initPosition3d.X = position.X;
            initPosition3d.Y = position.Y;

            for (int i = 0; i < sentence.Count(); i++)
            {

                if(charToTexCoords(sentence[i]))
                {
                    stringVertices[index + 0].Position = initPosition3d;
                    stringVertices[index + 1].Position = initPosition3d + direction3d;
                    stringVertices[index + 2].Position = initPosition3d + upDirection3d;

                    stringVertices[index + 3].Position = initPosition3d + direction3d;
                    stringVertices[index + 4].Position = initPosition3d + upDirection3d;
                    stringVertices[index + 5].Position = initPosition3d + direction3d + upDirection3d;

                    for (int col = 0; col < 6; col++) stringVertices[index + col].Color = color;

                    stringVertices[index + 0].TextureCoordinate = texCoords;
                    stringVertices[index + 1].TextureCoordinate = texCoords + vecDx;
                    stringVertices[index + 2].TextureCoordinate = texCoords + vecDy;

                    stringVertices[index + 3].TextureCoordinate = texCoords + vecDx;
                    stringVertices[index + 4].TextureCoordinate = texCoords + vecDy;
                    stringVertices[index + 5].TextureCoordinate = texCoords + vecDx + vecDy;

                    //stringVertices[index + 0].TextureCoordinate = new Vector2(0, 1);
                    //stringVertices[index + 1].TextureCoordinate = new Vector2(1, 1);
                    //stringVertices[index + 2].TextureCoordinate = new Vector2(0, 0);

                    //stringVertices[index + 3].TextureCoordinate = new Vector2(1, 1);
                    //stringVertices[index + 4].TextureCoordinate = new Vector2(0, 0);
                    //stringVertices[index + 5].TextureCoordinate = new Vector2(1, 0);

                    index += 6;

                    if (index >= nCharacters * 6) index = 0;
                }

                initPosition3d += direction3d;
            }
        }

        private bool charToTexCoords(char c)
        {
            if (c == ' ') return false;
            bool exit = false;
            for (int y = 0; y < nCharY; y++)
            {
                for (int x = 0; x < nCharX; x++)
                {
                    if (c == chars[x, y])
                    {
                        exit = true;
                        texCoords.X = x * texDx;
                        texCoords.Y = y * texDy;

                        break;
                    }
                }
                if (exit) break;
            }
            return true;
        }

        private void initChars()
        {
            char c = 'a';
            bool exit = false;
            for (int y = 0; y < nCharY; y++)
            {
                for (int x = 0; x < nCharX; x++)
                {
                    chars[x,y] = c;
                    if (c == 'z')
                    {
                        exit = true;
                        break;
                    }
                    c++;
                }
                if (exit) break;
            }
            
            
            chars[6, 2] = '?';
            chars[7, 2] = '%';
            chars[8, 2] = '!';
            chars[9, 2] = '&';

            c = '0';
            for (int i = 0; i < 10; i++)
            {
                chars[i,3] = c;
                c++;
            }
        }
    }
}
