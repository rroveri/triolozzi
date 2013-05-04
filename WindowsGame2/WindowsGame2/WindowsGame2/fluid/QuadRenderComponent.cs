
/*
 * ZiggyWare's Quad Render Component
 */

// Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace WindowsGame2
{
    public partial class QuadRenderComponent :
                            Microsoft.Xna.Framework.DrawableGameComponent
    {
        // Private Members

        VertexDeclaration vertexDecl = null;
        VertexPositionTexture[] verts = null;
        short[] ib = null;


        Camera camera;
        GraphicsDevice device;
        Effect quadRenderEffect;

        int screenWidth, screenHeight;
        Vector2 v1 = new Vector2(), v2 = new Vector2();
        Vector2 rightDir = new Vector2(), bottomDir = new Vector2();

        // Constructor
        public QuadRenderComponent(Game game, Camera camera, int screenWidth, int screenHeight)
            : base(game)
        {
            // TODO: Construct any child components here
            this.camera = camera;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.device = GameServices.GetService<GraphicsDeviceManager>().GraphicsDevice;
            quadRenderEffect = GameServices.GetService<ContentManager>().Load<Effect>("QuadRenderEffect");
        }
        


        // LoadGraphicsContent

        protected override void LoadContent()
        {
            base.LoadContent();

            if (true)
            {
                IGraphicsDeviceService graphicsService =
                    (IGraphicsDeviceService)base.Game.Services.GetService(
                                                typeof(IGraphicsDeviceService));

                vertexDecl = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

                verts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(0,0,-0.1f),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,-0.1f),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,-0.1f),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,-0.1f),
                                new Vector2(1,0))
                        };

                ib = new short[] { 0, 1, 2, 2, 3, 0 };
            }

        }
        

        // UnloadGraphicsContent
        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (true)
            {
                if (vertexDecl != null)
                    vertexDecl.Dispose();

                vertexDecl = null;
            }
        }

        public void renderMainQuad()
        {
            Render(Vector2.One * -1, Vector2.One);
        }

        public void renderFromDisplayUnits(Vector2 v1, float dx, float dy)
        {
            v1 = Vector2.Transform(v1, camera.Transform);
            v1.X = v1.X / (float)screenWidth * 2 - 1;
            v1.Y = 1 - v1.Y / (float)screenHeight * 2;
            dx = dx / (float)screenWidth * 2;
            dy = dy / (float)screenHeight * 2;

            v2.X = v1.X + dx;
            v2.Y = v1.Y + dy;

            Render(v1, v2);
        }

        public void renderSprite(Texture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
        {
            v1 = Vector2.Transform(position + origin, camera.Transform);
            v1.X = v1.X / (float)screenWidth * 2 - 1;
            v1.Y = 1 - v1.Y / (float)screenHeight * 2;
            float dx = texture.Width / (float)screenWidth * 2;
            float dy = texture.Height / (float)screenHeight * 2;

            rightDir.X = (float)Math.Cos((double)rotation);
            rightDir.Y = (float)Math.Sin((double)rotation);

            bottomDir.X = rightDir.Y;
            bottomDir.Y = -rightDir.X;

            rightDir *= dx * scale.X;
            bottomDir *= dy * scale.Y;

            verts[0].Position.X = v1.X + rightDir.X;
            verts[0].Position.Y = v1.Y + rightDir.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X + bottomDir.X;
            verts[2].Position.Y = v2.Y + bottomDir.Y;

            verts[3].Position.X = v2.X + bottomDir.X + rightDir.X;
            verts[3].Position.Y = v2.Y + bottomDir.Y + rightDir.Y;

            quadRenderEffect.CurrentTechnique = quadRenderEffect.Techniques["BasicTechnique"];
            quadRenderEffect.CurrentTechnique.Passes["BasicPass"].Apply();
            quadRenderEffect.Parameters["currentTex"].SetValue(texture);
            quadRenderEffect.Parameters["multColor"].SetValue(color.ToVector4());
            device.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }

        public void renderFromScreenUnits(Vector2 v1, float dx, float dy)
        {
            v1.X = v1.X / (float)screenWidth * 2 - 1;
            v1.Y = 1 - v1.Y / (float)screenHeight * 2;
            dx = dx / (float)screenWidth * 2;
            dy = dy / (float)screenHeight * 2;

            v2.X = v1.X + dx;
            v2.Y = v1.Y + dy;

            Render(v1, v2);
        }

        // void Render(Vector2 v1, Vector2 v2)
        public void Render(Vector2 v1, Vector2 v2)
        {
            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }
        
    }
}
