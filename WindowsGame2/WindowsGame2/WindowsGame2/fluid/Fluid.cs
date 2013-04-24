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

        // Render Targets
        RenderTarget2D InputVelocities, InputDensities;
        RenderTarget2D Velocity, Density;
        RenderTarget2D Divergence, Pressure;
        RenderTarget2D Temp0, Temp1, Temp2, Temp3;
        public RenderTarget2D FinalRender;

        // Spritebatch settings
        SpriteSortMode SortMode = SpriteSortMode.Immediate;
        BlendState Blending = BlendState.Opaque;
        SamplerState Sampling = new SamplerState();
        SurfaceFormat ColorFormat = SurfaceFormat.HdrBlendable;
        DepthFormat ZFormat = DepthFormat.None;        
        
        // Fluid shader and parameteres
        Effect Shader;
        FluidParams Params = new FluidParams();

        MouseState ms;
        Vector2 mouse, lastMouse;

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

        private int width, height;
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
        byte[] screenData;

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
            Shader.Parameters["FluidSize"].SetValue(Params.GridSize);
            Shader.Parameters["VelocityDiffusion"].SetValue(Params.VelocityDiffusion);
            Shader.Parameters["DensityDiffusion"].SetValue(Params.DensityDiffusion);

            // Setup RenderTarget2D's
            InputVelocities = new RenderTarget2D(graphicsDevice, (int)Params.ScreenSize.X, (int)Params.ScreenSize.Y, false, ColorFormat, ZFormat);
            InputDensities = new RenderTarget2D(graphicsDevice, (int)Params.ScreenSize.X, (int)Params.ScreenSize.Y, false, ColorFormat, ZFormat);
            Velocity        = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Density         = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Pressure        = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Divergence      = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Temp0           = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Temp1           = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Temp2           = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            Temp3           = new RenderTarget2D(graphicsDevice, Params.GridSize, Params.GridSize, false, ColorFormat, ZFormat);
            FinalRender = new RenderTarget2D(graphicsDevice, (int)Params.ScreenSize.X, (int)Params.ScreenSize.Y, false, ColorFormat, ZFormat);

            mainQuad[0].Position = new Vector3(-1, -1, 0);
            mainQuad[1].Position = new Vector3(1, 1, 0);
            mainQuad[2].Position = new Vector3(-1, 1, 0);

            mainQuad[3].Position = new Vector3(-1, -1, 0);
            mainQuad[4].Position = new Vector3(1, -1, 0);
            mainQuad[5].Position = new Vector3(1, 1, 0); 

            mainQuad[0].TextureCoordinate = new Vector2(0, 0);
            mainQuad[1].TextureCoordinate = new Vector2(1, 1);
            mainQuad[2].TextureCoordinate = new Vector2(0, 1);

            mainQuad[3].TextureCoordinate = new Vector2(0, 0);
            mainQuad[4].TextureCoordinate = new Vector2(1, 0);
            mainQuad[5].TextureCoordinate = new Vector2(1, 1);

            width = (int)Params.ScreenSize.X;
            height = (int)Params.ScreenSize.Y;

            GaussianEffect = c.Load<Effect>("Shaders/GaussianBlur");

            backTex = new RenderTarget2D(graphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            VTarget = new RenderTarget2D(graphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            HTarget = new RenderTarget2D(graphicsDevice, this.width, this.height, true,
                SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);

            sampleOffsetsH = new Vector2[windowSize];
            sampleWeightsH = new float[windowSize];

            sampleOffsetsV = new Vector2[windowSize];
            sampleWeightsV = new float[windowSize];

            texelSize = new Vector2(1.0f / this.width, 1.0f / this.height);

            SetBlurParameters(texelSize.X, 0, ref sampleOffsetsH, ref sampleWeightsH);
            SetBlurParameters(0, texelSize.Y, ref sampleOffsetsV, ref sampleWeightsV);

            colorMuco = new Color(0.1f,0.6f,0.1f);
            brushColor = colorMuco;
        }

        // Starts the VelocitySplat pass
        public void BeginVelocityPass()
        {
            Shader.CurrentTechnique = Shader.Techniques["VelocityColorize"];
            graphicsDevice.SetRenderTarget(InputVelocities);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, BlendState.AlphaBlend, Sampling, null, null, Shader);
        }

        // Starts the DensitySplat pass
        public void BeginDensityPass()
        {
            Shader.CurrentTechnique = Shader.Techniques["VelocityColorize"];
            graphicsDevice.SetRenderTarget(InputDensities);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SortMode, BlendState.AlphaBlend, Sampling, null, null);
        }

        // Ends either the VelocitySplat or DensitySplat passes
        public void EndPass()
        {
            spriteBatch.End();
        }

        // Render the fluid to the screen
        public void Update()
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
            mouse = new Vector2(ms.X, ms.Y);
            velocityColor = new Vector4(lastMouse - mouse, 0.0f, 1.0f);
            lastMouse = mouse;

            BeginDensityPass();
            if (ms.LeftButton == ButtonState.Pressed)
                spriteBatch.Draw(brush, mouse, null, brushColor, 0.0f, new Vector2(32.0f, 32.0f), brushSize / 64.0f, SpriteEffects.None, 0.0f);
            EndPass();

            BeginVelocityPass();
            if (ms.RightButton == ButtonState.Pressed || true)
            {
                SplatColor = velocityColor;
                spriteBatch.Draw(brush, mouse, null, Color.White, 0.0f, new Vector2(32.0f, 32.0f), brushSize / 64.0f, SpriteEffects.None, 0.0f);
            }
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
            for (int i = 0; i < Params.Iterations; i++)
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

            graphicsDevice.SetRenderTarget(null);

        }

        public void Draw()
        {
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
                new Microsoft.Xna.Framework.Rectangle(0, 0, this.width, this.height),
                Color.White);
            spriteBatch.End();
        }
    }

    public class FluidParams
    {
        // Iterations to perform
        public int Iterations;
        // Size of the fluid grid  
        public int GridSize;
        // Size of the screen the fluid is to be rendered on
        public Vector2 ScreenSize;
        // Delta time, MARKED FOR REMOVAL
        public float dT;
        // Diffusion rate of velocity
        public float VelocityDiffusion;
        // Diffusion rate for density
        public float DensityDiffusion;

        public FluidParams()
        {
            Iterations = 20;
            GridSize = 256;
            ScreenSize = new Vector2(1024, 768);
            dT = 1.0f;
            VelocityDiffusion = 0.99f;
            DensityDiffusion = 0.9999f;
        }
    }
}
