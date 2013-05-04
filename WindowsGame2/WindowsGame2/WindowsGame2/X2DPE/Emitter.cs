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
using X2DPE.Helpers;
using WindowsGame2;

namespace X2DPE
{
	public class Emitter
	{
		// TODO: multiple emitters per loop !!
		EmitterHelper emitterHelper = new EmitterHelper();

		public List<Particle> Particles { get; set; }
		public bool EmittedNewParticle { get; set; }
		public Particle LastEmittedParticle { get; set; }
		public List<Texture2D> TextureList { get; set; }
		public bool Active { get; set; }
		public int ParticleLifeTime { get; set; }
		public Vector2 Position { get; set; }
		public RandomMinMax ParticleDirection { get; set; }	// in degrees (0-359)
		public RandomMinMax ParticleSpeed { get; set; }	// in ms
		public RandomMinMax RandomEmissionInterval { get; set; }	// in ms
		public RandomMinMax ParticleRotation { get; set; }
		public RandomMinMax RotationSpeed { get; set; }
		public ParticleFader ParticleFader { get; set; } // Fader settings
		public ParticleScaler ParticleScaler { get; set; } // scale settings
		public int Opacity { get; set; }
        public Color TextureColor { get; set; }

		private int i = 0;
		private double emitterFrequency = 0;	// in ms
		private double timeSinceLastEmission = 0;

        private Camera camera;
        private Queue<Particle> PooledParticles;

		public Emitter()
		{
			Active = true;
			Particles = new List<Particle>();
			TextureList = new List<Texture2D>();
			Opacity = 255;
            camera = GameServices.GetService<Camera>();
            PooledParticles = new Queue<Particle>();
		}

		public void UpdateParticles(GameTime gameTime)
		{
			EmittedNewParticle = false;
			if (gameTime.ElapsedGameTime.TotalMilliseconds > 0)
			{
				if (Active)
				{
					timeSinceLastEmission += gameTime.ElapsedGameTime.Milliseconds;

					if (emitterFrequency == 0 || timeSinceLastEmission >= emitterFrequency)
					{
						emitterFrequency = emitterHelper.RandomizedDouble(RandomEmissionInterval);
						if (emitterFrequency == 0)
						{
							throw new Exception("emitter frequency cannot be below 0.1d !!");
						}
						for (int i = 0; i < Math.Round(timeSinceLastEmission / emitterFrequency); i++)
						{
							EmitParticle();
						}
						timeSinceLastEmission = 0;
					}
				}
				else
				{
					emitterFrequency = 0;
				}

                for (int i = 0; i < Particles.Count; i++)
				{
                    float y = -1 * ((float)Math.Cos(MathHelper.ToRadians(Particles[i].Direction))) * Particles[i].Speed;
                    float x = (float)Math.Sin(MathHelper.ToRadians(Particles[i].Direction)) * Particles[i].Speed;

                    Particles[i].TotalLifetime += gameTime.ElapsedGameTime.Milliseconds;
                    Particles[i].Position += new Vector2(x, y);
                    Particles[i].Rotation += Particles[i].RotationSpeed;
                    ParticleScaler.Scale(Particles[i], ParticleLifeTime);
                    Particles[i].Fade = ParticleFader.Fade(Particles[i], ParticleLifeTime);
                    //  particle.Color = new Color(particle.Fade * TextureColor.R, particle.Fade * TextureColor.G, particle.Fade * TextureColor.B, particle.Fade * TextureColor.A);
                    Particles[i].SetColor(TextureColor.R, TextureColor.G, TextureColor.B, Particles[i].Fade);

                    if (Particles[i].TotalLifetime > ParticleLifeTime)
					{
                        PooledParticles.Enqueue(Particles[i]);
                        Particles.Remove(Particles[i]);
					}
				}
			}
		}

		private void EmitParticle()
		{
			if (i > TextureList.Count - 1) i = 0;

            Particle particle;
            float speed = (float)emitterHelper.RandomizedDouble(ParticleSpeed);
            float direction = (float)emitterHelper.RandomizedDouble(ParticleDirection);
            float rotation = MathHelper.ToRadians((float)emitterHelper.RandomizedDouble(ParticleRotation));
            float rotationSpeed = (float)emitterHelper.RandomizedDouble(RotationSpeed);

            if (PooledParticles.Count > 0)
            {
                particle = PooledParticles.Dequeue();
                particle.Reuse(TextureList[i], Position, speed, direction, rotation, rotationSpeed, Opacity);
            }
            else
            {
                particle = new Particle(TextureList[i], Position, speed, direction, rotation, rotationSpeed, Opacity);
            }

			Particles.Add(particle);
			EmittedNewParticle = true;
			LastEmittedParticle = particle;
			i++;
		}

		public void DrawParticles(GameTime gameTime, SpriteBatch spriteBatch)
		{
            for (int i = 0; i < Particles.Count; i++)
			{
                Vector2 screenPosition = Vector2.Transform(Particles[i].Position, camera.Transform);
                spriteBatch.Draw(Particles[i].Texture,
                                                 screenPosition,
												 null,
                                                 Particles[i].Color,
                                                 Particles[i].Rotation,
                                                 Particles[i].Center,
                                                 Particles[i].Scale,
												 SpriteEffects.None,
												 -0.1f);
			}
		}
	}
}
