
/*
 * ZiggyWare's Quad Render Component
 */

// Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WindowsGame2
{
    public partial class QuadRenderComponent :
                            Microsoft.Xna.Framework.DrawableGameComponent
    {
        // Private Members

        VertexDeclaration vertexDecl = null;
        VertexPositionTexture[] verts = null;
        short[] ib = null;

        


        // Constructor
        public QuadRenderComponent(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
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
                                new Vector3(0,0,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
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

        // void Render(Vector2 v1, Vector2 v2)
        public void Render(Vector2 v1, Vector2 v2)
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                base.Game.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;

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
