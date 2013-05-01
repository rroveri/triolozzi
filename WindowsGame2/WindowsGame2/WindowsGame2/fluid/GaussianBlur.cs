using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace WindowsGame2
{
    class GaussianBlur
    {
        private Game game;
        private int width, height;
        private Vector2 texelSize;

        private Vector2[] sampleOffsetsH = new Vector2[15];
        private float[] sampleWeightsH = new float[15];

        private Vector2[] sampleOffsetsV = new Vector2[15];
        private float[] sampleWeightsV = new float[15];

        private RenderTarget2D backTex;

        private RenderTarget2D VTarget, HTarget;
        private Texture2D HResolve, FinalBlur;

        private Effect GaussianEffect;

        private SpriteBatch sprite;
        byte[] screenData;

        public GaussianBlur(Game game)
        {
            this.game = game;

            this.width = game.Window.ClientBounds.Width;
            this.height = game.Window.ClientBounds.Height;

            Load();
            Init();
        }

        private void Load()
        {
            GaussianEffect = game.Content.Load<Effect>("Effects/GaussianBlur");

            backTex = new RenderTarget2D(game.GraphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, game.GraphicsDevice.PresentationParameters.DepthStencilFormat);
            VTarget = new RenderTarget2D(game.GraphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, game.GraphicsDevice.PresentationParameters.DepthStencilFormat);
            HTarget = new RenderTarget2D(game.GraphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, game.GraphicsDevice.PresentationParameters.DepthStencilFormat);

            sprite = new SpriteBatch(game.GraphicsDevice);
        }

        private void Init()
        {
            texelSize = new Vector2(1.0f / this.width, 1.0f / this.height);

            SetBlurParameters(texelSize.X, 0, ref sampleOffsetsH, ref sampleWeightsH);
            SetBlurParameters(0, texelSize.Y, ref sampleOffsetsV, ref sampleWeightsV);
        }

        private void SetBlurParameters(float dx, float dy, ref Vector2[] vSampleOffsets, ref float[] fSampleWeights)
        {
            fSampleWeights[0] = ComputeGaussian(0);
            vSampleOffsets[0] = new Vector2(0);

            float totalWeights = fSampleWeights[0];

            for (int i = 0; i < 15 / 2; i++)
            {
                float weight = ComputeGaussian(i + 1);

                fSampleWeights[i * 2 + 1] = weight;
                fSampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                vSampleOffsets[i * 2 + 1] = delta;
                vSampleOffsets[i * 2 + 2] = -delta;
            }

            for (int i = 0; i < fSampleWeights.Length; i++)
            {
                fSampleWeights[i] /= totalWeights;
            }
        }

        private float ComputeGaussian(float n)
        {
            float theta = 2.0f + float.Epsilon;

            return theta = (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        public Texture2D Render(RenderTarget2D buffer)
        {

            game.GraphicsDevice.SetRenderTarget(HTarget);
            Blur(buffer, sampleOffsetsH, sampleWeightsH);

            game.GraphicsDevice.SetRenderTarget(VTarget);
            HResolve = HTarget;

            Blur(HResolve, sampleOffsetsV, sampleWeightsV);
            game.GraphicsDevice.SetRenderTarget(null);

            FinalBlur = VTarget;

            return FinalBlur;

            //screenData = new byte[game.GraphicsDevice.PresentationParameters.BackBufferWidth * game.GraphicsDevice.PresentationParameters.BackBufferHeight * 4];

            //game.GraphicsDevice.GetBackBufferData<byte>(screenData);
            //backTex.SetData(screenData);

            //game.GraphicsDevice.SetRenderTarget(HTarget);
            //Blur(backTex, sampleOffsetsH, sampleWeightsH);

            //game.GraphicsDevice.SetRenderTarget(VTarget);
            //HResolve = HTarget;

            //Blur(HResolve, sampleOffsetsV, sampleWeightsV);
            //game.GraphicsDevice.SetRenderTarget(null);

            //FinalBlur = VTarget;

            //return FinalBlur;
        }

        private void Blur(Texture2D BlurTexture, Vector2[] sampleOffsets, float[] sampleWeights)
        {
            game.GraphicsDevice.Clear(Color.White);

            GaussianEffect.Parameters["sampleWeights"].SetValue(sampleWeights);
            GaussianEffect.Parameters["sampleOffsets"].SetValue(sampleOffsets);

            sprite.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            GaussianEffect.CurrentTechnique.Passes[0].Apply();

            sprite.Draw(BlurTexture, 
                new Microsoft.Xna.Framework.Rectangle(0, 0, this.width, this.height),
                Color.White);
            sprite.End();
        }
    }
}
