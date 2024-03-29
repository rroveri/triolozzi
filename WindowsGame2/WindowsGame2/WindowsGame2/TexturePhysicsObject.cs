﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.SamplesFramework;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics;

namespace WindowsGame2
{
    public class TexturePhysicsObject
    {
        public Body _compound;
        public Vector2 _origin;
        public Texture2D _polygonTexture;
        protected Vector2 textureScale;
        protected Vector2 vertScale;
        public Color color;

        protected Vector2 secondTextureScale;
        protected Vector2 secondVertScale;

        protected Vector2 currentScale;

        protected bool isSecondModeActive;

        public Vector2 Position
        {
            get { return ConvertUnits.ToDisplayUnits(_compound.Position); }
            set { _compound.Position = value * ConvertUnits.ToSimUnits(1);}
        }


        public TexturePhysicsObject(World world, Texture2D texture, Vector2 size, Color color)
        {
            this.textureScale = new Vector2(size.X / (float)texture.Width, size.Y / (float)texture.Height);
            this.color = color;

            //load texture that will represent the physics body
            _polygonTexture = texture;

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);
            

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 20f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);


            //scale the vertices from graphics space to sim space
            vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * textureScale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            _compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _compound.BodyType = BodyType.Dynamic;
        }

        public TexturePhysicsObject(World world, Texture2D texture, Vector2 size, Color color, Vector2 secondSize)
        {
            this.textureScale = new Vector2(size.X / (float)texture.Width, size.Y / (float)texture.Height);
            this.secondTextureScale = new Vector2(secondSize.X / (float)texture.Width, secondSize.Y / (float)texture.Height);
            this.color = color;

            //load texture that will represent the physics body
            _polygonTexture = texture;

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);


            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 20f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);


            //scale the vertices from graphics space to sim space
            vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * textureScale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            _compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _compound.BodyType = BodyType.Dynamic;

            secondVertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * secondTextureScale;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!isSecondModeActive)
            {
                spriteBatch.Draw(_polygonTexture, ConvertUnits.ToDisplayUnits(_compound.Position),
                                           null, color, _compound.Rotation, _origin, textureScale, SpriteEffects.None,
                                           0.9f);
            }
            else
            {
                spriteBatch.Draw(_polygonTexture, ConvertUnits.ToDisplayUnits(_compound.Position),
                                           null, color, _compound.Rotation, _origin, secondTextureScale, SpriteEffects.None,
                                           0.9f);

            }


        }

        public void activateSecondMode()
        {
            if (isSecondModeActive) return;
            isSecondModeActive = true;
            currentScale.X = 1 / vertScale.X;
            currentScale.Y = 1 / vertScale.Y;
            currentScale *= secondVertScale;
            for (int i = 0; i < _compound.FixtureList.Count(); i++)
            {
                PolygonShape polyShape = (PolygonShape)_compound.FixtureList[i].Shape;
                polyShape.Vertices.Scale(ref currentScale);
                polyShape.ComputeProperties();
                _compound.Mass *= 2.0f;
            }
        }

        public void deactivateSecondMode()
        {
            if (!isSecondModeActive) return;
            isSecondModeActive = false;
            currentScale.X = 1 / secondVertScale.X;
            currentScale.Y = 1 / secondVertScale.Y;
            currentScale *= vertScale;
            for (int i = 0; i < _compound.FixtureList.Count(); i++)
            {
                PolygonShape polyShape = (PolygonShape)_compound.FixtureList[i].Shape;
                polyShape.Vertices.Scale(ref currentScale);
                polyShape.ComputeProperties();
                _compound.Mass *= 0.5f;
            }
        }
    }
}
