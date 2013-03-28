using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace WindowsGame2.GameElements
{

    /// <summary>
    /// Enum describes the track to be created.
    /// </summary>
    public enum TrackType
    {
        PenisTrack,
        Track2,
        Track3,
        Track4,
    }

    class Track
    {

        List<Vector2> backgrounds;
        List<DrawablePhysicsObject> bordersList;
        
        Texture2D squaredBg;
        Texture2D dummyTexture;
        World world;

        float bgScale = 0.9f;

        public static Track CreateTrack(TrackType track)
        {
            Track result = new Track();
            switch (track)
            {
                case TrackType.PenisTrack:
                    result.BuildPenisTrack();
                    return result;
                case TrackType.Track2:
                    return result;
                case TrackType.Track3:
                    return result;
                case TrackType.Track4:
                    return result;
                default:
                    throw new NotSupportedException();
            }
        }

        public Track()
        {
            backgrounds = new List<Vector2>();
            bordersList = new List<DrawablePhysicsObject>();
            
            // Load textures
            dummyTexture = new Texture2D(GameServices.GetService<GraphicsDevice>(), 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });

            ContentManager Content = GameServices.GetService<ContentManager>();
            squaredBg = Content.Load<Texture2D>("Images/squaredBg2");

            // Define the worls in which the track is going to be drawn
            world = GameServices.GetService<World>();
        }

        #region Track Creation

        private void BuildPenisTrack()
        {
            leftRightTile(0, 0);
            leftRightTile(1, 0);
            leftTopTile(2, 0);
            rightBottomTile(2, -1);
            leftTopTile(3, -1);
            leftBottomTile(3, -2);
            rightTopTile(2, -2);
            bottomTopTile(2, -3);
            leftBottomTile(2, -4);
            leftRightTile(1, -4);
            leftRightTile(0, -4);
            rightBottomTile(-1, -4);
            bottomTopTile(-1, -3);
            leftTopTile(-1, -2);
            rightTopTile(-2, -2);
            leftBottomTile(-2, -3);
            rightBottomTile(-3, -3);
            bottomTopTile(-3, -2);
            bottomTopTile(-3, -1);
            bottomTopTile(-3, 0);
            bottomTopTile(-3, 1);
            rightTopTile(-3, 2);
            leftTopTile(-2, 2);
            bottomTopTile(-2, 1);
            bottomTopTile(-2, 0);
            rightBottomTile(-2, -1);
            leftBottomTile(-1, -1);
            rightTopTile(-1, 0);
        }

        #endregion

        #region Update and Draw

        public void DrawSprites(Camera camera, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transform);

            // draw backgrounds
            for (int i = 0; i < backgrounds.Count; i++)
            {
                spriteBatch.Draw(squaredBg, backgrounds[i], null, Color.White, 0.0f, Vector2.Zero, Vector2.One * bgScale, SpriteEffects.None, 1f);
            }

            //draw starting line
            spriteBatch.Draw(dummyTexture, new Vector2(squaredBg.Width / 2 * bgScale - 100, 0), null, Color.Yellow, 0.0f, Vector2.Zero, new Vector2(100, squaredBg.Height * bgScale), SpriteEffects.None, 1f);

            // draw walls
            for (int i = 0; i < bordersList.Count; i++)
            {
                bordersList[i].Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        #endregion

        #region Racetrack Creation Helpers

        private void rightTopTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;

            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY - height / 4);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        private void rightBottomTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;

            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY + height / 4);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        private void leftBottomTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;

            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY + height / 4);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        private void leftTopTile(int cellX, int cellY)
        {
            float height = 800.0f;
            float width = 800f;

            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);
            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale - height / 2), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY - height / 4);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        private void bottomTopTile(int cellX, int cellY)
        {

            float width = 800f;

            DrawablePhysicsObject rightWall;
            rightWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale), 1000.0f, Color.Black);
            rightWall.Position = new Vector2(squaredBg.Width * bgScale - width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY);
            rightWall.body.BodyType = BodyType.Static;
            bordersList.Add(rightWall);
            DrawablePhysicsObject leftWall;
            leftWall = new DrawablePhysicsObject(world, dummyTexture, new Vector2(width / 2, squaredBg.Height * bgScale), 1000.0f, Color.Black);
            leftWall.Position = new Vector2(width / 4 + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale / 2.0f + squaredBg.Height * bgScale * cellY);
            leftWall.body.BodyType = BodyType.Static;
            bordersList.Add(leftWall);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        private void leftRightTile(int cellX, int cellY)
        {
            float height = 800.0f;

            DrawablePhysicsObject floor;
            floor = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            floor.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, squaredBg.Height * bgScale - height / 4 + squaredBg.Height * bgScale * cellY);
            floor.body.BodyType = BodyType.Static;
            bordersList.Add(floor);
            DrawablePhysicsObject ceil;
            ceil = new DrawablePhysicsObject(world, dummyTexture, new Vector2(squaredBg.Width * bgScale, height / 2), 1000.0f, Color.Black);
            ceil.Position = new Vector2(squaredBg.Width * bgScale / 2.0f + squaredBg.Width * bgScale * cellX, height / 4 + squaredBg.Height * bgScale * cellY);
            ceil.body.BodyType = BodyType.Static;
            bordersList.Add(ceil);

            backgrounds.Add(new Vector2(cellX * squaredBg.Width * bgScale, cellY * squaredBg.Height * bgScale));

        }

        #endregion
    }
}
