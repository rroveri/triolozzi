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
    public class Fluid : Thread
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
        HalfVector4[] densityData, resetArray;

        HalfVector4[][] densityTextures = new HalfVector4[1][];

        // Spritebatch settings
        SpriteSortMode SortMode = SpriteSortMode.Immediate;
        BlendState Blending = BlendState.Opaque;
        SamplerState Sampling = new SamplerState();
        SurfaceFormat ColorFormat = SurfaceFormat.HdrBlendable;
        DepthFormat ZFormat = DepthFormat.None;        
        
        // Fluid shader and parameteres
        Effect Shader;

        MouseState ms;
        public Vector2 carPos, lastCarPos;

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

        private GraphicsDeviceManager _graphicsManager;

        public bool shouldResetDensity = false;
        public bool shouldWait = false;
        public bool inCriticalSection = false;

        public Fluid(ContentManager c, GraphicsDevice gd, SpriteBatch sb)
        {
            // Get the content manager and graphics device from the creating entity
            brush = c.Load<Texture2D>("Images/brush");

            Shader               = c.Load<Effect>("Shaders/Fluid");
            graphicsDevice       = gd;
            spriteBatch          = sb;
            _graphicsManager = GameServices.GetService<GraphicsDeviceManager>();

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
            resetArray = new HalfVector4[gridSize * gridSize];

            halfRenderWidth = (float)renderWidth / (float)_graphicsManager.PreferredBackBufferWidth;
            halfRenderHeight = (float)renderHeight / (float)_graphicsManager.PreferredBackBufferHeight;

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

            //GaussianEffect = c.Load<Effect>("Shaders/GaussianBlur");

            //backTex = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
            //    SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            //VTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
            //    SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
            //HTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, true,
            //    SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);

            //sampleOffsetsH = new Vector2[windowSize];
            //sampleWeightsH = new float[windowSize];

            //sampleOffsetsV = new Vector2[windowSize];
            //sampleWeightsV = new float[windowSize];

            //texelSize = new Vector2(1.0f / renderWidth, 1.0f / renderHeight);

            //SetBlurParameters(texelSize.X, 0, ref sampleOffsetsH, ref sampleWeightsH);
            //SetBlurParameters(0, texelSize.Y, ref sampleOffsetsV, ref sampleWeightsV);

            colorMuco = new Color(0.1f,0.6f,0.1f);
            brushColor = colorMuco;

           // Texture2D Densitytd = c.Load<Texture2D>("Images/mucus/color_scrofa");
           // Densitytd.GetData<HalfVector4>(densityData);
           // Density.SetData<HalfVector4>(densityData);
            
            Texture2D densTex = c.Load<Texture2D>("Images/mucus/color_scrofa" + gridSize);
            initDensity(0,densTex);
            resetDensity();
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
            //position -= referencePosition;
            //if (position.X < 0 || position.Y < 0 || position.X >= renderWidth - 1 || position.Y >= renderHeight - 1) return 0;
            //int index = (int)position.Y * renderHeight + (int)position.X;
            //return finalData[index].ToVector3().LengthSquared();

            position -= referencePosition;
            if (position.X < 0 || position.Y < 0 || position.X >= gridSize - 1 || position.Y >= gridSize - 1) return 0;
            int index = (int)position.Y * gridSize + (int)position.X;
            Vector4 densityColor = densityData[index].ToVector4();
            return densityColor.X * densityColor.X + densityColor.Y * densityColor.Y + densityColor.Z * densityColor.Z;
        }

        // Render the fluid to the screen
        public void Update()
        {
            for (int i = 0; i < 16; i++)
                graphicsDevice.SamplerStates[i] = Sampling;

            //ms = Mouse.GetState();
            //mouse = new Vector2(ms.X, ms.Y) - referencePosition;
            carPos -= referencePosition;
            //velocityColor = new Vector4(lastMouse - mouse, 0.0f, 1.0f);
            velocityColor = new Vector4(lastCarPos - carPos, 0.0f, 1.0f);
            lastCarPos = carPos;

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

            Density.GetData(densityData);

            graphicsDevice.SetRenderTarget(null);

        }

        public void Draw(Vector2 position)
        {
            referencePosition = position;
            this.position.X = position.X / _graphicsManager.PreferredBackBufferWidth * 2 - 1;
            this.position.Y = position.Y / _graphicsManager.PreferredBackBufferHeight * 2 - 1;
            updatePosition();
            Shader.CurrentTechnique = Shader.Techniques["Final"];
            Shader.CurrentTechnique.Passes["Final"].Apply();
            graphicsDevice.Textures[7] = Density;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, mainQuad, 0, 2);
            graphicsDevice.Textures[7] = null;
        }

        private void initDensity(int index, Texture2D densTex)
        {
            Color[] array = new Color[gridSize * gridSize];
            densTex.GetData(array);
            HalfVector4 hcol;
            for (int i = 0; i < densityData.Count(); i++)
            {
                hcol = new HalfVector4(array[i].ToVector4());
                resetArray[i] = hcol;
            }
            densityTextures[index] = resetArray;
        }

        public void resetDensity()
        {
            Density.SetData(densityTextures[0]);
        }

        private void saveDensityToTexture()
        {
            Stream pngFile = File.OpenWrite("color_" + "scrofa" + ".png");
            Density.SaveAsPng(pngFile, Density.Width, Density.Height);
            pngFile.Close();
        }

        protected override void threadFunction()
        {
            while (shouldWait) { }
            inCriticalSection = true;
            if (shouldResetDensity) resetDensity();
            Update();
            inCriticalSection = false;
        }

    }
}
