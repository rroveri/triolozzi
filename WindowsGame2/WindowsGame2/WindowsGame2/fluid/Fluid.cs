/*
    Fluid.cs - HLSL fluid shader handler class
    Copyright (C) 2013 Michael Stone (Neoaikon)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.IO;

namespace WindowsGame2
{
    class Fluid
    {
        GraphicsDevice graphicsDevice;
        SpriteBatch spriteBatch;

        Texture2D brush;
        float brushSize = 30.0f;
        Color brushColor;
        Vector4 velocityColor = new Vector4();

        VertexPositionTexture[] mainQuad = new VertexPositionTexture[6];
        Vector2 position = new Vector2(0), referencePosition = new Vector2(0);
        int renderWidth = 256, renderHeight = 256;
        float halfRenderWidth, halfRenderHeight;
        int iterations = 5;
        int gridSize = 128;
        float velocityDiffusion = 0.3f;
        float densityDiffusion = 0.9999f;

        // Render Targets
        RenderTarget2D InputVelocities, InputDensities;
        RenderTarget2D Velocity, Density;
        RenderTarget2D Divergence, Pressure;
        RenderTarget2D Temp0, Temp1, Temp2, Temp3;
        public RenderTarget2D FinalRender;
        Color[] finalData;
        HalfVector4[] densityData;

        // Spritebatch settings
        SpriteSortMode SortMode = SpriteSortMode.Immediate;
        BlendState Blending = BlendState.Opaque;
        SamplerState Sampling = new SamplerState();
        SurfaceFormat ColorFormat = SurfaceFormat.HdrBlendable;
        DepthFormat ZFormat = DepthFormat.None;        
        
        // Fluid shader and parameteres
        Effect Shader;

        MouseState ms;
        Vector2 mouse, lastMouse, lastCarPos;

        Color colorMuco;

        // Effect Parameters        
        EffectParameter pVelocitySrc;
        EffectParameter pDensitySrc;        
        EffectParameter pSplatColor;
        // Color splatted for the current spriteBatch during the VelocitySplat pass        
        public Vector4 SplatColor
        {
            get { return pSplatColor.GetValueVector4(); }
            set { pSplatColor.SetValue(value); }
        }
        private Vector2 texelSize;

        private int windowSize = 30;
        private Vector2[] sampleOffsetsH;
        private float[] sampleWeightsH;

        private Vector2[] sampleOffsetsV;
        private float[] sampleWeightsV;

        private RenderTarget2D backTex;

        private RenderTarget2D VTarget, HTarget;
        private Texture2D HResolve, FinalBlur;

        private Effect GaussianEffect;

        public Fluid(ContentManager c, GraphicsDevice gd, SpriteBatch sb)
        {
            // Get the content manager and graphics device from the creating entity
            brush = c.Load<Texture2D>("Images/brush");

            Shader               = c.Load<Effect>("Shaders/Fluid");
            graphicsDevice       = gd;
            spriteBatch          = sb;

            // Setup EffectParameters
            pSplatColor          = Shader.Parameters["VelocityColor"];
            pVelocitySrc         = Shader.Parameters["VelocitySrc"];
            pDensitySrc          = Shader.Parameters["DensitySrc"];            

            // Setup SamplerStates
            Sampling.AddressU = TextureAddressMode.Clamp;
            Sampling.AddressV = TextureAddressMode.Clamp;
            Sampling.AddressW = TextureAddressMode.Clamp;
            Sampling.Filter = TextureFilter.Point;
            for(int i = 0; i < 16; i++)
                graphicsDevice.SamplerStates[i] = Sampling;

            // Setup fluid parameters
            Shader.Parameters["FluidSize"].SetValue(gridSize);
            Shader.Parameters["VelocityDiffusion"].SetValue(velocityDiffusion);
            Shader.Parameters["DensityDiffusion"].SetValue(densityDiffusion);

            // Setup RenderTarget2D's
            InputVelocities = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, false, ColorFormat, ZFormat);
            InputDensities  = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, false, ColorFormat, ZFormat);
            Velocity        = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Density         = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Pressure        = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Divergence      = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Temp0           = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Temp1           = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Temp2           = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            Temp3           = new RenderTarget2D(graphicsDevice, gridSize, gridSize, false, ColorFormat, ZFormat);
            FinalRender = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, false, ColorFormat, ZFormat);
            finalData = new Color[renderWidth * renderWidth];
            densityData = new HalfVector4[gridSize*gridSize];

            halfRenderWidth = (float)renderWidth / (float)GameServices.GetService<GraphicsDeviceManager>().PreferredBackBufferWidth;
            halfRenderHeight = (float)renderHeight / (float)GameServices.GetService<GraphicsDeviceManager>().PreferredBackBufferHeight;

            mainQuad[0].Position = new Vector3(0);
            mainQuad[1].Position = new Vector3(0);
            mainQuad[2].Position = new Vector3(0);

            mainQuad[3].Position = new Vector3(0);
            mainQuad[4].Position = new Vector3(0);
            mainQuad[5].Position = new Vector3(0); 

            mainQuad[0].TextureCoordinate = new Vector2(0, 0);
            mainQuad[1].TextureCoordinate = new Vector2(1, 1);
            mainQuad[2].TextureCoordinate = new Vector2(0, 1);

            mainQuad[3].TextureCoordinate = new Vector2(0, 0);
            mainQuad[4].TextureCoordinate = new Vector2(1, 0);
            mainQuad[5].TextureCoordinate = new Vector2(1, 1);

            GaussianEffect = c.Load<Effect>("Shaders/GaussianBlur");

            backTex = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            VTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            HTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);

            sampleOffsetsH = new Vector2[windowSize];
            sampleWeightsH = new float[windowSize];

            sampleOffsetsV = new Vector2[windowSize];
            sampleWeightsV = new float[windowSize];

            texelSize = new Vector2(1.0f / renderWidth, 1.0f / renderHeight);

            SetBlurParameters(texelSize.X, 0, ref sampleOffsetsH, ref sampleWeightsH);
            SetBlurParameters(0, texelSize.Y, ref sampleOffsetsV, ref sampleWeightsV);

            colorMuco = new Color(0.1f,0.6f,0.1f);
            brushColor = colorMuco;

            Texture2D Densitytd = c.Load<Texture2D>("Images/mucus/color_scrofa");
            Densitytd.GetData<HalfVector4>(densityData);
            Density.SetData<HalfVector4>(densityData);
            
        }

        // Starts the VelocitySplat pass
        private void BeginVelocityPass()
        {
            Shader.CurrentTechnique = Shader.Techniques["VelocityColorize"];
            graphicsDevice.SetRenderTarget(InputVelocities);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, BlendState.AlphaBlend, Sampling, null, null, Shader);
        }

        // Starts the DensitySplat pass
        private void BeginDensityPass()
        {
            Shader.CurrentTechnique = Shader.Techniques["VelocityColorize"];
            graphicsDevice.SetRenderTarget(InputDensities);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, BlendState.AlphaBlend, Sampling, null, null);
        }

        // Ends either the VelocitySplat or DensitySplat passes
        private void EndPass()
        {
            spriteBatch.End();
        }

        private void updatePosition()
        {
            mainQuad[0].Position.X = position.X; mainQuad[0].Position.Y = position.Y;
            mainQuad[1].Position.X = position.X + halfRenderWidth * 2; mainQuad[1].Position.Y = position.Y + halfRenderHeight * 2;
            mainQuad[2].Position.X = position.X; mainQuad[2].Position.Y = position.Y + halfRenderHeight * 2;

            mainQuad[3].Position.X = position.X; mainQuad[3].Position.Y = position.Y;
            mainQuad[4].Position.X = position.X + halfRenderWidth * 2; mainQuad[4].Position.Y = position.Y;
            mainQuad[5].Position.X = position.X + halfRenderWidth * 2; mainQuad[5].Position.Y = position.Y + halfRenderHeight * 2;
        }

        public float fluidLevelAtPosition(Vector2 position)
        {
            position -= referencePosition;
            if (position.X < 0 || position.Y < 0 || position.X >= renderWidth - 1 || position.Y >= renderHeight - 1) return 0;
            int index = (int)position.Y * renderHeight + (int)position.X;
            return finalData[index].ToVector3().LengthSquared();
        }

        // Render the fluid to the screen
        public void Update(Vector2 carPos)
        {
            for (int i = 0; i < 16; i++)
                graphicsDevice.SamplerStates[i] = Sampling;

            if (Keyboard.GetState().IsKeyDown(Keys.F1)) brushColor = colorMuco;
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) brushColor = Color.Aqua;
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) brushColor = Color.Yellow;
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) brushColor = Color.Green;
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) brushColor = Color.Red;
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) brushColor = Color.White;

            ms = Mouse.GetState();
            mouse = new Vector2(ms.X, ms.Y) - referencePosition;
            carPos -= referencePosition;
            //velocityColor = new Vector4(lastMouse - mouse, 0.0f, 1.0f);
            velocityColor = new Vector4(lastCarPos - carPos, 0.0f, 1.0f);
            lastMouse = mouse;
            lastCarPos = carPos;

            BeginDensityPass();
            if (ms.LeftButton == ButtonState.Pressed)
                spriteBatch.Draw(brush, mouse, null, brushColor, 0.0f, new Vector2(32.0f, 32.0f), brushSize / 64.0f, SpriteEffects.None, 0.0f);
            EndPass();

            BeginVelocityPass();
            SplatColor = velocityColor;
            spriteBatch.Draw(brush, carPos, null, Color.White, 0.0f, new Vector2(32.0f, 32.0f), brushSize / 64.0f, SpriteEffects.None, 0.0f);
            EndPass();

            // Add the velocity and density splats into the fluid
            Shader.CurrentTechnique = Shader.Techniques["DoAddSources"];
            graphicsDevice.SetRenderTargets(Temp0, Temp1);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.Textures[1] = Velocity; // We can either remove this, or the draw call
            graphicsDevice.Textures[2] = Density;
            graphicsDevice.Textures[5] = InputVelocities;
            graphicsDevice.Textures[6] = InputDensities;
            spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
            //Shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(Velocity, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Advect the velocity and density
            // Other quantities can be advected too
            Shader.CurrentTechnique = Shader.Techniques["DoAdvection"];
            graphicsDevice.SetRenderTargets(Velocity, Density);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.Textures[1] = Temp0;
            graphicsDevice.Textures[2] = Temp1;
            spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
            spriteBatch.Draw(Temp0, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Calculate the divergence of the velocity
            Shader.CurrentTechnique = Shader.Techniques["DoDivergence"];
            graphicsDevice.SetRenderTarget(Divergence);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
            //Shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(Velocity, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Update the shaders copy of the divergence
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Textures[3] = Divergence;

            // Iterate over the grid calculating the pressure
            Shader.CurrentTechnique = Shader.Techniques["DoJacobi"];
            for (int i = 0; i < iterations; i++)
            {
                graphicsDevice.SetRenderTarget(Pressure);
                graphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
                spriteBatch.Draw(Temp2, Vector2.Zero, Color.White);
                spriteBatch.End();

                graphicsDevice.SetRenderTarget(Temp2);
                graphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
                spriteBatch.Draw(Pressure, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            // Update the shaders copy of the pressure
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Textures[4] = Pressure;

            // Subtract the pressure from the velocity
            Shader.CurrentTechnique = Shader.Techniques["Subtract"];
            graphicsDevice.SetRenderTarget(Temp0);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, Blending, Sampling, null, null, Shader);
            spriteBatch.Draw(Velocity, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Swap Velocity and Temp0
            graphicsDevice.SetRenderTarget(Velocity);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, Blending, Sampling, null, null);
            spriteBatch.Draw(Temp0, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Gaussian Blurr
            graphicsDevice.SetRenderTarget(HTarget);
            Blur(Density, sampleOffsetsH, sampleWeightsH);

            graphicsDevice.SetRenderTarget(VTarget);
            HResolve = HTarget;

            Blur(HResolve, sampleOffsetsV, sampleWeightsV);
            graphicsDevice.SetRenderTarget(null);

            FinalBlur = VTarget;

            FinalBlur.GetData(finalData);

            graphicsDevice.SetRenderTarget(null);


            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                Stream pngFile = File.OpenWrite("color_" + "scrofa" + ".png");
                Density.SaveAsPng(pngFile, Density.Width, Density.Height);
                pngFile.Close();
            }

        }

        public void Draw(Vector2 position)
        {
            referencePosition = position;
            this.position.X = position.X / GameServices.GetService<GraphicsDeviceManager>().PreferredBackBufferWidth * 2 - 1;
            this.position.Y = position.Y / GameServices.GetService<GraphicsDeviceManager>().PreferredBackBufferHeight * 2 - 1;
            updatePosition();
            graphicsDevice.Clear(Color.White);
            Shader.CurrentTechnique = Shader.Techniques["Final"];
            Shader.CurrentTechnique.Passes["Final"].Apply();
            Shader.Parameters["finalTexture"].SetValue(FinalBlur);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, mainQuad, 0, 2);
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

        private void Blur(Texture2D BlurTexture, Vector2[] sampleOffsets, float[] sampleWeights)
        {
            graphicsDevice.Clear(Color.White);

            GaussianEffect.Parameters["sampleWeights"].SetValue(sampleWeights);
            GaussianEffect.Parameters["sampleOffsets"].SetValue(sampleOffsets);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            GaussianEffect.CurrentTechnique.Passes[0].Apply();

            graphicsDevice.SamplerStates[0] = Sampling;

            spriteBatch.Draw(BlurTexture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, renderWidth, renderHeight),
                Color.White);
            spriteBatch.End();
        }
    }
}
