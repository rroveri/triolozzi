/* 
 * Basically a C# port of Mick West's 2D fluid solver presented
 * in a Game Developer Magazine's article. I've simplified it 
 * a little and added RGBA ink advection.
 */

#region Using Statemens

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace WindowsGame2
{
    public class Fluid
    {

        #region Public Fields

        public bool update = true;
        public Vector2 renderPosition = new Vector2();
      
        public int m_w;
        public int m_h;
        
        public float[] mp_xv0;
        public float[] mp_yv0;

        public Vector4[] mp_ink0;

        public int m_diffusion_iterations;

        public float m_velocity_diffusion;
        public float m_pressure_diffusion;
        public float m_ink_diffusion;

        public float m_velocity_friction_a;
        public float m_velocity_friction_b;
        public float m_velocity_friction_c;
        public float m_vorticity;
        public float m_pressure_acc;
        public float m_ink_advection;
        public float m_velocity_advection;
        public float m_pressure_advection;

        #endregion

        #region Private Fields

        private float[] mp_xv1;
        private float[] mp_xv2;
        private float[] mp_yv1;
        private float[] mp_yv2;

        private float[] mp_p0;
        private float[] mp_p1;

        private Vector4[] mp_ink1;

        private Vector4[][] densities;

        private int[] mp_sources;
        private float[] mp_source_fractions;
        private float[] mp_fraction;

        private float m_dt;
        private int size;
        private Vector4 currentColor;

        public Color[] texData;
        public float renderWidth = 256;
        public float renderHeight = 256;
        public bool shouldResetDensity;
        private int currentResetTexture;

        #endregion

        #region Constructor

        public Fluid()
        {
        }

        #endregion

        #region Init

        public void Init()
        {
#if !XBOX360
            m_w = 64;
            m_h = 64;
            size = m_w * m_h;
            mp_xv0 = new float[size];
            mp_yv0 = new float[size];
            mp_xv1 = new float[size];
            mp_yv1 = new float[size];
            mp_xv2 = new float[size];
            mp_yv2 = new float[size];
            mp_p0 = new float[size];
            mp_p1 = new float[size];
            mp_ink0 = new Vector4[size];
            mp_ink1 = new Vector4[size];
            mp_sources = new int[size];
            mp_source_fractions = new float[size * 4];
            mp_fraction = new float[size];
            Reset();

            m_diffusion_iterations = 1;

            m_velocity_diffusion = .5f;
            m_pressure_diffusion = .5f;
            m_ink_diffusion = .1f;

            m_vorticity = 1.0f;

            m_pressure_acc = 0.5f;

            m_ink_advection = 120.0f;
            m_velocity_advection = 80.0f;
            m_pressure_advection = 80.0f;
#else
            m_w = 64;
            m_h = 64;
            size = m_w * m_h;
            mp_xv0 = new float[size];
            mp_yv0 = new float[size];
            mp_xv1 = new float[size];
            mp_yv1 = new float[size];
            mp_xv2 = new float[size];
            mp_yv2 = new float[size];
            mp_p0 = new float[size];
            mp_p1 = new float[size];
            mp_ink0 = new Vector4[size];
            mp_ink1 = new Vector4[size];
            mp_sources = new int[size];
            mp_source_fractions = new float[size * 4];
            mp_fraction = new float[size];
            Reset();

            m_diffusion_iterations = 1;

            m_velocity_diffusion = .5f;
            m_pressure_diffusion = .5f;
            m_ink_diffusion = 1.0f;

            m_vorticity = 1.0f;

            m_pressure_acc = 0.5f;

            m_ink_advection = 120.0f;
            m_velocity_advection = 100.0f;
            m_pressure_advection = 80.0f;
#endif

            currentColor = new Vector4(0.1f, 0.6f, 0.1f, 1.0f);

            texData = new Color[m_w * m_h];

            loadDensities();

            currentResetTexture = 0;
        }

        public void saveDensity()
        {
            GraphicsDevice device = GameServices.GetService<GraphicsDeviceManager>().GraphicsDevice;
            Texture2D tex = new Texture2D(device, m_w, m_h, false, SurfaceFormat.Color);
            if (File.Exists("mucus" + m_w + "x" + m_h + ".png")) return;
            Stream stream = File.OpenWrite("mucus" + m_w + "x" + m_h + ".png");
            tex.SetData(texData);
            tex.SaveAsPng(stream, m_w, m_h);
        }

        public void loadDensities()
        {
            int size = m_w * m_h;
            densities = new Vector4[4][];
            for (int t = 0; t < densities.Length; t++)
            {
                densities[t] = new Vector4[size];

                Texture2D densTex = GameServices.GetService<ContentManager>().Load<Texture2D>("Images/mucus/mucus" + m_w + "x" + m_h + "n" + t);

                Color[] texData = new Color[m_w * m_h];
                densTex.GetData(texData);

                for (int i = 0; i < size; i++)
                {
                    densities[t][i] = texData[i].ToVector4();
                } 
            }
        }

        #endregion

        #region Reset

        public void Reset()
        {
	        int size = m_w * m_h;
	        for (int i = 0; i<size; i++)
	        {
		        mp_xv0[i] = 0.0f;
		        mp_yv0[i] = 0.0f;
		        mp_xv1[i] = 0.0f;
		        mp_yv1[i] = 0.0f;
		        mp_xv2[i] = 0.0f;
		        mp_yv2[i] = 0.0f;
		        mp_p0[i]  = 1.0f;
		        mp_p1[i]  = 1.0f;
		        mp_ink0[i]  = Vector4.Zero;
	        }
        }

        #endregion

        #region Update 

        public void Update(float dt)
        {
            KeyboardState keyState = Keyboard.GetState();

            DateTime startTime = DateTime.Now;
            
	        if (!update)
		        return;

	        if (keyState.IsKeyDown(Keys.S))
		        Reset();

	        m_dt = dt;

            if (m_velocity_diffusion != 0.0f)
            {
                for (int i = 0; i < m_diffusion_iterations; i++)
                {
                    Diffusion(ref mp_xv0, ref mp_xv1, m_velocity_diffusion / (float)m_diffusion_iterations);
                    swap(ref mp_xv0, ref mp_xv1);
                    Diffusion(ref mp_yv0, ref mp_yv1, m_velocity_diffusion / (float)m_diffusion_iterations);
                    swap(ref mp_yv0, ref mp_yv1);
                }
            }

            if (m_pressure_diffusion != 0.0f)
            {
                for (int i = 0; i < m_diffusion_iterations; i++)
                {
                    Diffusion(ref mp_p0, ref mp_p1, m_pressure_diffusion / (float)m_diffusion_iterations);
                    swap(ref mp_p0, ref mp_p1);
                }
            }

            if (m_ink_diffusion != 0.0f)
            {
                for (int i = 0; i < m_diffusion_iterations; i++)
                {
                    Diffusion(ref mp_ink0, ref mp_ink1, m_ink_diffusion / (float)m_diffusion_iterations);
                    swap(ref mp_ink0, ref mp_ink1);
                }
            }
        
            if (m_vorticity != 0.0f)
                VorticityConfinement(m_vorticity);

            if (m_pressure_acc != 0.0f)
            {
                PressureAcceleration(m_pressure_acc);
            }
            if (m_velocity_friction_a != 0.0f || m_velocity_friction_b != 0.0f || m_velocity_friction_c != 0.0f)
            {
                VelocityFriction(m_velocity_friction_a, m_velocity_friction_b, m_velocity_friction_c);
            }

            // Advection
            float advection_scale = m_w / 100.0f;

            ForwardAdvection(ref mp_ink0, ref mp_ink1, m_ink_advection * advection_scale);
            swap(ref mp_ink0, ref mp_ink1);
            ReverseAdvection(ref mp_ink0, ref mp_ink1, m_ink_advection * advection_scale);
            swap(ref mp_ink0, ref mp_ink1);
  
            ForwardAdvection(ref mp_xv0, ref mp_xv1, m_velocity_advection * advection_scale);
	        ForwardAdvection(ref mp_yv0, ref mp_yv1, m_velocity_advection * advection_scale);
         
	        ReverseSignedAdvection(ref mp_xv1, ref mp_xv2, m_velocity_advection * advection_scale);
	        ReverseSignedAdvection(ref mp_yv1, ref mp_yv2, m_velocity_advection * advection_scale);
	        swap(ref mp_xv2, ref mp_xv0);
	        swap(ref mp_yv2, ref mp_yv0);
         
	        //EdgeVelocities();

            // Pressure advection
            ForwardAdvection(ref mp_p0, ref mp_p1, m_pressure_advection * advection_scale);
            swap(ref mp_p0, ref mp_p1);

            ReverseAdvection(ref mp_p0, ref mp_p1, m_pressure_advection * advection_scale);
            swap(ref mp_p0, ref mp_p1);

            for (int i = 0; i < m_w * m_h; i++)
            {
                texData[i].R = (byte)(Math.Min(mp_ink0[i].X, 1) * 255);
                texData[i].G = (byte)(Math.Min(mp_ink0[i].Y, 1) * 255);
                texData[i].B = (byte)(Math.Min(mp_ink0[i].Z, 1) * 255);
                texData[i].A = (byte)(Math.Min(mp_ink0[i].W, 1) * 255);
            }

            if (shouldResetDensity) resetDensity();
        }

        public float fluidLevelAtPosition(float relativePosX, float relativePosY)
        {
            if (relativePosX < 0 || relativePosY < 0 || relativePosX >= m_w - 1 || relativePosY >= m_h - 1) return 0;
            int index = Cell((int)relativePosX,(int)relativePosY);
            Vector4 densityColor = mp_ink0[index];
            return densityColor.X * densityColor.X + densityColor.Y * densityColor.Y + densityColor.Z * densityColor.Z;
        }

        #endregion

        #region Private Methods

        private void EdgeVelocities()
        {
	        for (int y = 0; y<m_h;y++)
	        {
		        int left_cell = Cell(0,y); 
		        if (mp_xv0[left_cell] < 0.0f)
		        {
			        mp_xv0[left_cell] = -mp_xv0[left_cell];
		        }
		        int right_cell = Cell(m_w-1,y); 
		        if (mp_xv0[right_cell] > 0.0f)
		        {
			        mp_xv0[right_cell] = -mp_xv0[right_cell];
		        }
	        }

	        for (int x = 0; x<m_w;x++)
	        {
		        int top_cell = Cell(x,0); 
		        if (mp_yv0[top_cell] < 0.0f)
		        {
			        mp_yv0[top_cell] = -mp_yv0[top_cell];
		        }
		        int bot_cell = Cell(x,m_h-1); 
		        if (mp_yv0[bot_cell] > 0.0f)
		        {
			        mp_yv0[bot_cell] = -mp_yv0[bot_cell];
		        }
	        }
        }

        private void ReverseSignedAdvection(ref float[] p_in, ref float[] p_out, float scale)
        {
	        float a = - m_dt * scale ;

	        int size = m_w*m_h;

	        CopyField(ref p_in, ref p_out);

	        for (int x = 0; x < m_w; x++)
	        {
		        for (int y = 0; y < m_h; y++)
		        {
			        int cell = Cell(x,y);
			        float vx = mp_xv0[cell];
			        float vy = mp_yv0[cell];
			        if (vx != 0.0f || vy != 0.0f)
			        {
				        float x1 = x + vx * a;
				        float y1 = y + vy * a;
				        Collide(x,y, ref x1, ref y1);
				        int cell1 = Cell((int)x1,(int)y1);
                        if (cell1 < 0 || cell1 >= mp_xv0.Length) return;
        				
				        float fx = x1-(int)x1;
				        float fy = y1-(int)y1;

				        float ia = (1.0f-fy)*(1.0f-fx) * p_in[cell1];
				        float ib = (1.0f-fy)*(fx)      * p_in[cell1+1];
				        float ic = (fy)     *(1.0f-fx) * p_in[cell1+m_w];
				        float id = (fy)     *(fx)      * p_in[cell1+m_w+1];

				        p_out[cell] += ia + ib + ic + id ;
				        
				        p_out[cell1]      -= ia;
				        p_out[cell1+1]    -= ib;
				        p_out[cell1+m_w]  -= ic;
				        p_out[cell1+m_w+1]-= id;
			        }
		        }
	        }
        }
        
        private void ReverseAdvection(ref float[] p_in, ref float[] p_out, float scale)
        {
            float a = -m_dt * scale;

            int size = m_w * m_h;

            CopyField(ref p_in, ref p_out);

            ZeroField(ref mp_fraction);

            for (int x = 0; x < m_w; x++)
	        {
		        for (int y = 0; y < m_h; y++)
		        {
			        int cell = Cell(x,y);
			        float vx = mp_xv0[cell];
			        float vy = mp_yv0[cell];
			        if (vx != 0.0f || vy != 0.0f)
			        {
				        float x1 = x + vx * a;
				        float y1 = y + vy * a;
				        Collide(x,y, ref x1, ref y1);
				        int cell1 = Cell((int)x1,(int)y1);
                        if (cell1 < 0 || cell1 >= mp_xv0.Length) return;
        				
				        float fx = x1-(int)x1;
				        float fy = y1-(int)y1;
                        float ia = (1.0f - fy) * (1.0f - fx);
                        float ib = (1.0f - fy) * (fx);
                        float ic = (fy) * (1.0f - fx);
                        float id = (fy) * (fx);
                        mp_sources[cell] = cell1;
                        
                        mp_source_fractions[cell * 4 + 0] = ia;
                        mp_source_fractions[cell * 4 + 1] = ib;
                        mp_source_fractions[cell * 4 + 2] = ic;
                        mp_source_fractions[cell * 4 + 3] = id;

                        mp_fraction[cell1] += ia;
                        mp_fraction[cell1 + 1] += ib;
                        mp_fraction[cell1 + m_w] += ic;
                        mp_fraction[cell1 + m_w + 1] += id;
                        
                    }
                    else
                    {
                        mp_sources[cell] = -1;   
                    }
                }
            }

            for (int cell=0;cell<size;cell++)
	        {
		        int cell1 = mp_sources[cell];
		        if (cell1 != -1)
		        {
			        float ia = mp_source_fractions[cell*4+0];
			        float ib = mp_source_fractions[cell*4+1];
			        float ic = mp_source_fractions[cell*4+2];
			        float id = mp_source_fractions[cell*4+3];
			        
			        float fa = mp_fraction[cell1];
			        float fb = mp_fraction[cell1+1];
			        float fc = mp_fraction[cell1+m_w];
			        float fd = mp_fraction[cell1+1+m_w];
			                
			        if (fa<1.0f) fa = 1.0f;
			        if (fb<1.0f) fb = 1.0f;
			        if (fc<1.0f) fc = 1.0f;
			        if (fd<1.0f) fd = 1.0f;
        
			        ia /= fa;
			        ib /= fb;
			        ic /= fc;
			        id /= fd;
			        p_out[cell] += ia * p_in[cell1] + ib * p_in[cell1+1] + ic * p_in[cell1+m_w] + id * p_in[cell1+1+m_w];
			        p_out[cell1]      -= ia * p_in[cell1];
			        p_out[cell1+1]    -= ib * p_in[cell1+1];
			        p_out[cell1+m_w]  -= ic * p_in[cell1+m_w];
			        p_out[cell1+m_w+1]-= id * p_in[cell1+m_w+1];

		        }
	        }
        }

        private void ReverseAdvection(ref Vector4[] p_in, ref Vector4[] p_out, float scale)
        {
            float a = -m_dt * scale;

            int size = m_w * m_h;

            CopyField(ref p_in, ref p_out);

            ZeroField(ref mp_fraction);

            for (int x = 0; x < m_w; x++)
            {
                for (int y = 0; y < m_h; y++)
                {
                    int cell = Cell(x, y);
                    float vx = mp_xv0[cell];
                    float vy = mp_yv0[cell];
                    if (vx != 0.0f || vy != 0.0f)
                    {
                        float x1 = x + vx * a;
                        float y1 = y + vy * a;
                        Collide(x, y, ref x1, ref y1);
                        int cell1 = Cell((int)x1, (int)y1);
                        if (cell1 < 0 || cell1 >= mp_xv0.Length) return;

                        float fx = x1 - (int)x1;
                        float fy = y1 - (int)y1;
                        float ia = (1.0f - fy) * (1.0f - fx);
                        float ib = (1.0f - fy) * (fx);
                        float ic = (fy) * (1.0f - fx);
                        float id = (fy) * (fx);
                        
                        mp_sources[cell] = cell1;  
                        
                        mp_source_fractions[cell * 4 + 0] = ia;
                        mp_source_fractions[cell * 4 + 1] = ib;
                        mp_source_fractions[cell * 4 + 2] = ic;
                        mp_source_fractions[cell * 4 + 3] = id;

                        mp_fraction[cell1] += ia;
                        mp_fraction[cell1 + 1] += ib;
                        mp_fraction[cell1 + m_w] += ic;
                        mp_fraction[cell1 + m_w + 1] += id;            
                    }
                    else
                    {
                        mp_sources[cell] = -1;  // 
                    }
                }
            }

            for (int cell = 0; cell < size; cell++)
            {
                int cell1 = mp_sources[cell];
                if (cell1 != -1)
                {
                    float ia = mp_source_fractions[cell * 4 + 0];
                    float ib = mp_source_fractions[cell * 4 + 1];
                    float ic = mp_source_fractions[cell * 4 + 2];
                    float id = mp_source_fractions[cell * 4 + 3];
                   
                    float fa = mp_fraction[cell1];
                    float fb = mp_fraction[cell1 + 1];
                    float fc = mp_fraction[cell1 + m_w];
                    float fd = mp_fraction[cell1 + 1 + m_w];
                    
                    if (fa < 1.0f) fa = 1.0f;
                    if (fb < 1.0f) fb = 1.0f;
                    if (fc < 1.0f) fc = 1.0f;
                    if (fd < 1.0f) fd = 1.0f;

                    ia /= fa;
                    ib /= fb;
                    ic /= fc;
                    id /= fd;

                    p_out[cell].X += ia * p_in[cell1].X + ib * p_in[cell1 + 1].X + ic * p_in[cell1 + m_w].X + id * p_in[cell1 + 1 + m_w].X;
                    p_out[cell].Y += ia * p_in[cell1].Y + ib * p_in[cell1 + 1].Y + ic * p_in[cell1 + m_w].Y + id * p_in[cell1 + 1 + m_w].Y;
                    p_out[cell].Z += ia * p_in[cell1].Z + ib * p_in[cell1 + 1].Z + ic * p_in[cell1 + m_w].Z + id * p_in[cell1 + 1 + m_w].Z;
                    p_out[cell].W += ia * p_in[cell1].W + ib * p_in[cell1 + 1].W + ic * p_in[cell1 + m_w].W + id * p_in[cell1 + 1 + m_w].W;
                    
                    p_out[cell1].X -= ia * p_in[cell1].X;
                    p_out[cell1].Y -= ia * p_in[cell1].Y;
                    p_out[cell1].Z -= ia * p_in[cell1].Z;
                    p_out[cell1].W -= ia * p_in[cell1].W;

                    p_out[cell1 + 1].X -= ib * p_in[cell1 + 1].X;
                    p_out[cell1 + 1].Y -= ib * p_in[cell1 + 1].Y;
                    p_out[cell1 + 1].Z -= ib * p_in[cell1 + 1].Z;
                    p_out[cell1 + 1].W -= ib * p_in[cell1 + 1].W;

                    p_out[cell1 + m_w].X -= ic * p_in[cell1 + m_w].X;
                    p_out[cell1 + m_w].Y -= ic * p_in[cell1 + m_w].Y;
                    p_out[cell1 + m_w].Z -= ic * p_in[cell1 + m_w].Z;
                    p_out[cell1 + m_w].W -= ic * p_in[cell1 + m_w].W;

                    p_out[cell1 + m_w + 1].X -= id * p_in[cell1 + m_w + 1].X;
                    p_out[cell1 + m_w + 1].Y -= id * p_in[cell1 + m_w + 1].Y;
                    p_out[cell1 + m_w + 1].Z -= id * p_in[cell1 + m_w + 1].Z;
                    p_out[cell1 + m_w + 1].W -= id * p_in[cell1 + m_w + 1].W;
                }
            }
        }

        private void ForwardAdvection(ref float[] p_in, ref float[] p_out, float scale)
        {
		    float a = m_dt * scale ;

	        int w=m_w;
	        int h=m_h;

	        CopyField(ref p_in, ref p_out);

	        if (scale == 0.0f)
		        return;

	        for (int x = 0; x < w; x++)
	        {
		        for (int y = 0; y < h; y++)
		        {
			        int cell = Cell(x,y);
			        float vx = mp_xv0[cell];
			        float vy = mp_yv0[cell];
			        if (vx != 0.0f || vy != 0.0f)
			        {
				        float x1 = x + vx * a;
				        float y1 = y + vy * a;
				        Collide(x,y, ref x1, ref y1);
				        int cell1 = Cell((int)x1,(int)y1);
                        if (cell1 < 0 || cell1 >= mp_xv0.Length) return;
				        
				        float fx = x1-(int)x1;
				        float fy = y1-(int)y1;

				        float ins = p_in[cell];

				        float ia = (1.0f-fy)*(1.0f-fx) * ins;
				        float ib = (1.0f-fy)*(fx)      * ins;
				        float ic = (fy)     *(1.0f-fx) * ins;
                        float id = (fy) * (fx) * ins;

				        p_out[cell] -= (ia+ib+ic+id);
				        
				        p_out[cell1]		+= ia;
				        p_out[cell1+1]		+= ib;
				        p_out[cell1+w]		+= ic;
				        p_out[cell1+w+1]	+= id;
			        }
		        }
	        }
        }

        private void ForwardAdvection(ref Vector4[] p_in, ref Vector4[] p_out, float scale)
        {
            float a = m_dt * scale;

            int w = m_w;
            int h = m_h;

            CopyField(ref p_in, ref p_out);

            if (scale == 0.0f)
                return;


            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int cell = Cell(x, y);
                    float vx = mp_xv0[cell];
                    float vy = mp_yv0[cell];
                    if (vx != 0.0f || vy != 0.0f)
                    {
                        float x1 = x + vx * a;
                        float y1 = y + vy * a;
                        Collide(x, y, ref x1, ref y1);
                        int cell1 = Cell((int)x1, (int)y1);
                        if(cell1 < 0 || cell1 >= mp_xv0.Length) return;
                        
                        float fx = x1 - (int)x1;
                        float fy = y1 - (int)y1;

                        Vector4 ia;
                        ia.X = (1.0f - fy) * (1.0f - fx) * p_in[cell].X;
                        ia.Y = (1.0f - fy) * (1.0f - fx) * p_in[cell].Y;
                        ia.Z = (1.0f - fy) * (1.0f - fx) * p_in[cell].Z;
                        ia.W = (1.0f - fy) * (1.0f - fx) * p_in[cell].W;

                        Vector4 ib;
                        ib.X = (1.0f - fy) * (fx) * p_in[cell].X;
                        ib.Y = (1.0f - fy) * (fx) * p_in[cell].Y;
                        ib.Z = (1.0f - fy) * (fx) * p_in[cell].Z;
                        ib.W = (1.0f - fy) * (fx) * p_in[cell].W;

                        Vector4 ic;
                        ic.X = (fy) * (1.0f - fx) * p_in[cell].X;
                        ic.Y = (fy) * (1.0f - fx) * p_in[cell].Y;
                        ic.Z = (fy) * (1.0f - fx) * p_in[cell].Z;
                        ic.W = (fy) * (1.0f - fx) * p_in[cell].W;

                        Vector4 id;
                        id.X = (fy) * (fx) * p_in[cell].X;
                        id.Y = (fy) * (fx) * p_in[cell].Y;
                        id.Z = (fy) * (fx) * p_in[cell].Z;
                        id.W = (fy) * (fx) * p_in[cell].W;

                        p_out[cell].X -= (ia.X + ib.X + ic.X + id.X);
                        p_out[cell].Y -= (ia.Y + ib.Y + ic.Y + id.Y);
                        p_out[cell].Z -= (ia.Z + ib.Z + ic.Z + id.Z);
                        p_out[cell].W -= (ia.W + ib.W + ic.W + id.W);
                        
                        p_out[cell1].X += ia.X;
                        p_out[cell1].Y += ia.Y;
                        p_out[cell1].Z += ia.Z;
                        p_out[cell1].W += ia.W;

                        p_out[cell1 + 1].X += ib.X;
                        p_out[cell1 + 1].Y += ib.Y;
                        p_out[cell1 + 1].Z += ib.Z;
                        p_out[cell1 + 1].W += ib.W;

                        p_out[cell1 + w].X += ic.X;
                        p_out[cell1 + w].Y += ic.Y;
                        p_out[cell1 + w].Z += ic.Z;
                        p_out[cell1 + w].W += ic.W;

                        p_out[cell1 + w + 1].X += id.X;
                        p_out[cell1 + w + 1].Y += id.Y;
                        p_out[cell1 + w + 1].Z += id.Z;
                        p_out[cell1 + w + 1].W += id.W;
                    }
                }
            }
        }

        private void Collide(float x,float y, ref float x1, ref float y1)
        {
	        float left_bound = m_w-1.0001f;
	        float bot_bound = m_h-1.0001f;

            if (x1 < 0) x1 = 0;
            else if (x1 > left_bound) x1 = left_bound;
            if (y1 < 0) y1 = 0;
            else if (y1 > bot_bound) y1 = bot_bound;
        }

        private void CopyField(ref float[] p_in, ref float[] p_out)
        {
	        int size = m_w * m_h;
            
            for (int x = 0; x<size;x++)
            {
	            p_out[x] = p_in[x];
            }
        }

        private void CopyField(ref Vector4[] p_in, ref Vector4[] p_out)
        {
            int size = m_w * m_h;

            for (int x = 0; x<size;x++)
            {
                p_out[x].X = p_in[x].X;
                p_out[x].Y = p_in[x].Y;
                p_out[x].Z = p_in[x].Z;
                p_out[x].W = p_in[x].W;
            }
        }

        private void SetField(ref float[] p_field, float f)
        {
	        int size = m_w * m_h;
	        for (int x = 0; x<size;x++)
	        {
		        p_field[x] = f;
	        }
        }

        private void VelocityFriction(float a, float b, float c )
        {
	        for (int iterate = 0; iterate<1;iterate++)
	        {
		        for (int x = 0; x < m_w; x++)
		        {
			        for (int y = 0; y < m_h; y++)
			        {
				        int cell = Cell(x,y);

				        Vector2  v = new Vector2(mp_xv0[cell],mp_yv0[cell]);
				        float len2 = v.Length();
				        float len = (float)Math.Sqrt(len2);
				        float sign = 1.0f;
				        if (len <0.0f)
				        {
					        len = -len;
					        sign = -1.0f;
				        }
        
				        len -= m_dt*(a*len2 + b*len +c);
        
				        if (len<0.0f)
					        len = 0.0f;

				        if (len < 0.0f) len = 0.0f;
                        if(v.X != 0.0f || v.Y != 0.0f)
                            v.Normalize();
				        v = Vector2.Multiply(v,len);
                       
				        mp_xv0[cell] = v.X;
				        mp_yv0[cell] = v.Y;
			        }
		        }
	        }

        }
        
        private void PressureAcceleration(float force)
        {
            float a = m_dt * force;

            for (int x = 0; x < m_w; x++)
            {
                for (int y = 0; y < m_h; y++)
                {
                    int cell = Cell(x, y);
                    mp_xv1[cell] = mp_xv0[cell];
                    mp_yv1[cell] = mp_yv0[cell];
                }
            }

            for (int x = 0; x < m_w-1; x++)
		    {
			    for (int y = 0; y < m_h-1; y++)
			    {
				    int cell = Cell(x,y);

				    float force_x =  mp_p0[cell]   - mp_p0[cell+1];  
				    float force_y =  mp_p0[cell]   - mp_p0[cell+m_w];  

					mp_xv1[cell]     +=  a * force_x;
					mp_xv1[cell+1]   +=  a * force_x;
    					
					mp_yv1[cell]     +=  a * force_y;
					mp_yv1[cell+m_w] +=  a * force_y;
			    }
		    }

            float[] t = mp_xv1;
            mp_xv1 = mp_xv0;
            mp_xv0 = t;

            t = mp_yv1;
            mp_yv1 = mp_yv0;
            mp_yv0 = t;
        }

        private void ForceFrom(ref float[] p_from, ref float[] p_to, float f)
        {
	        f *= m_dt;
	        int size = m_w * m_h;
	        for (int cell = 0; cell < size; cell++)
	        {
                p_to[cell] += p_from[cell]*f;
	        }
        }

        private void ForceFrom(ref Vector4[] p_from, ref Vector4[] p_to, float f)
        {
            f *= m_dt;
            int size = m_w * m_h;
            for (int cell = 0; cell < size; cell++)
            {
                p_to[cell].X += p_from[cell].X * f;
                p_to[cell].Y += p_from[cell].Y * f;
                p_to[cell].Z += p_from[cell].Z * f;
                p_to[cell].W += p_from[cell].W * f;
            }
        }

        private void QuadraticDecay(ref float[] p_in, ref float[] p_out, float a, float b, float c)
        {
	        float dt = m_dt;
	        int size = m_w*m_h;
	        for (int cell=0;cell<size;cell++)
	        {
		        float v = p_in[cell];
		        float v2 = v*v;
		        v-= dt*(a*v2 + b*v + c);
		        if (v <0.0f)
			        v= 0.0f;
		        p_in[cell] = v;
	        }
        }

        private void swap(ref float[] p0, ref float[] p1)
        {
	        float[] t = p0;
	        p0 = p1;
	        p1 = t;
        }

        private void swap(ref Vector4[] p0, ref Vector4[] p1)
        {
            Vector4[] t = p0;
            p0 = p1;
            p1 = t;
        }

        private void Diffusion(ref float[] p_in, ref float[] p_out, float scale)
        {
	        float a = m_dt * scale ;
	        int cell;
       
            for (int x = 1; x < m_w-1; x++)
	        {
		        cell = Cell(x,0);
		        p_out[cell] = p_in[cell] + a * (p_in[cell-1] + p_in[cell+1] + p_in[cell+m_h] - 3.0f * p_in[cell]);
		        cell = Cell(x,m_h-1);
		        p_out[cell] = p_in[cell] + a * (p_in[cell-1] + p_in[cell+1] + p_in[cell-m_h] - 3.0f * p_in[cell]);
	        }
        
	        for (int y = 1; y < m_h-1; y++)
	        {
		        cell = Cell(0,y);
		        p_out[cell] = p_in[cell] + a * (p_in[cell-m_w] + p_in[cell+m_w] + p_in[cell+1] - 3.0f * p_in[cell]);
		        cell = Cell(m_w-1,y);
		        p_out[cell] = p_in[cell] + a * (p_in[cell-m_w] + p_in[cell+m_w] + p_in[cell-1] - 3.0f * p_in[cell]);
	        }
      
	        cell = Cell(0,0);
	        p_out[cell] = p_in[cell] + a * (p_in[cell+1] + p_in[cell+m_w] - 2.0f * p_in[cell]);
	        cell = Cell(m_w-1,0);
	        p_out[cell] = p_in[cell] + a * (p_in[cell-1] + p_in[cell+m_w] - 2.0f * p_in[cell]);
	        cell = Cell(0,m_h-1);
	        p_out[cell] = p_in[cell] + a * (p_in[cell+1] + p_in[cell-m_w] - 2.0f * p_in[cell]);
	        cell = Cell(m_w-1,m_h-1);
	        p_out[cell] = p_in[cell] + a * (p_in[cell-1] + p_in[cell-m_w] - 2.0f * p_in[cell]);
        
	        for (int x = 1; x < m_w-1; x++)
	        {
		        for (int y = 1; y < m_h-1; y++)
		        {
			        cell = Cell(x,y);
			        p_out[cell] = p_in[cell] + a * (p_in[Cell(x,y+1)] + p_in[Cell(x,y-1)] + p_in[Cell(x+1,y)] + p_in[Cell(x-1,y)] - 4.0f * p_in[cell]);

		        }
	        }

        }

        private void Diffusion(ref Vector4[] p_in, ref Vector4[] p_out, float scale)
        {
            float a = m_dt * scale;
            int cell;

            for (int x = 1; x < m_w - 1; x++)
            {
                cell = Cell(x, 0);
                p_out[cell].X = p_in[cell].X + a * (p_in[cell - 1].X + p_in[cell + 1].X + p_in[cell + m_h].X - 3.0f * p_in[cell].X);
                p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - 1].Y + p_in[cell + 1].Y + p_in[cell + m_h].Y - 3.0f * p_in[cell].Y);
                p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - 1].Z + p_in[cell + 1].Z + p_in[cell + m_h].Z - 3.0f * p_in[cell].Z);
                p_out[cell].W = p_in[cell].W + a * (p_in[cell - 1].W + p_in[cell + 1].W + p_in[cell + m_h].W - 3.0f * p_in[cell].W);

                cell = Cell(x, m_h - 1);
                p_out[cell].X = p_in[cell].X + a * (p_in[cell - 1].X + p_in[cell + 1].X + p_in[cell - m_h].X - 3.0f * p_in[cell].X);
                p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - 1].Y + p_in[cell + 1].Y + p_in[cell - m_h].Y - 3.0f * p_in[cell].Y);
                p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - 1].Z + p_in[cell + 1].Z + p_in[cell - m_h].Z - 3.0f * p_in[cell].Z);
                p_out[cell].W = p_in[cell].W + a * (p_in[cell - 1].W + p_in[cell + 1].W + p_in[cell - m_h].W - 3.0f * p_in[cell].W);
            }
            
            for (int y = 1; y < m_h - 1; y++)
            {
                cell = Cell(0, y);
                p_out[cell].X = p_in[cell].X + a * (p_in[cell - m_w].X + p_in[cell + m_w].X + p_in[cell + 1].X - 3.0f * p_in[cell].X);
                p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - m_w].Y + p_in[cell + m_w].Y + p_in[cell + 1].Y - 3.0f * p_in[cell].Y);
                p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - m_w].Z + p_in[cell + m_w].Z + p_in[cell + 1].Z - 3.0f * p_in[cell].Z);
                p_out[cell].W = p_in[cell].W + a * (p_in[cell - m_w].W + p_in[cell + m_w].W + p_in[cell + 1].W - 3.0f * p_in[cell].W);

                cell = Cell(m_w - 1, y);
                p_out[cell].X = p_in[cell].X + a * (p_in[cell - m_w].X + p_in[cell + m_w].X + p_in[cell - 1].X - 3.0f * p_in[cell].X);
                p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - m_w].Y + p_in[cell + m_w].Y + p_in[cell - 1].Y - 3.0f * p_in[cell].Y);
                p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - m_w].Z + p_in[cell + m_w].Z + p_in[cell - 1].Z - 3.0f * p_in[cell].Z);
                p_out[cell].W = p_in[cell].W + a * (p_in[cell - m_w].W + p_in[cell + m_w].W + p_in[cell - 1].W - 3.0f * p_in[cell].W);
            }
            
            cell = Cell(0, 0);
            p_out[cell].X = p_in[cell].X + a * (p_in[cell + 1].X + p_in[cell + m_w].X - 2.0f * p_in[cell].X);
            p_out[cell].Y = p_in[cell].Y + a * (p_in[cell + 1].Y + p_in[cell + m_w].Y - 2.0f * p_in[cell].Y);
            p_out[cell].Z = p_in[cell].Z + a * (p_in[cell + 1].Z + p_in[cell + m_w].Z - 2.0f * p_in[cell].Z);
            p_out[cell].W = p_in[cell].W + a * (p_in[cell + 1].W + p_in[cell + m_w].W - 2.0f * p_in[cell].W);

            cell = Cell(m_w - 1, 0);
            p_out[cell].X = p_in[cell].X + a * (p_in[cell - 1].X + p_in[cell + m_w].X - 2.0f * p_in[cell].X);
            p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - 1].Y + p_in[cell + m_w].Y - 2.0f * p_in[cell].Y);
            p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - 1].Z + p_in[cell + m_w].Z - 2.0f * p_in[cell].Z);
            p_out[cell].W = p_in[cell].W + a * (p_in[cell - 1].W + p_in[cell + m_w].W - 2.0f * p_in[cell].W);

            cell = Cell(0, m_h - 1);
            p_out[cell].X = p_in[cell].X + a * (p_in[cell + 1].X + p_in[cell - m_w].X - 2.0f * p_in[cell].X);
            p_out[cell].Y = p_in[cell].Y + a * (p_in[cell + 1].Y + p_in[cell - m_w].Y - 2.0f * p_in[cell].Y);
            p_out[cell].Z = p_in[cell].Z + a * (p_in[cell + 1].Z + p_in[cell - m_w].Z - 2.0f * p_in[cell].Z);
            p_out[cell].W = p_in[cell].W + a * (p_in[cell + 1].W + p_in[cell - m_w].W - 2.0f * p_in[cell].W);

            cell = Cell(m_w - 1, m_h - 1);
            p_out[cell].X = p_in[cell].X + a * (p_in[cell - 1].X + p_in[cell - m_w].X - 2.0f * p_in[cell].X);
            p_out[cell].Y = p_in[cell].Y + a * (p_in[cell - 1].Y + p_in[cell - m_w].Y - 2.0f * p_in[cell].Y);
            p_out[cell].Z = p_in[cell].Z + a * (p_in[cell - 1].Z + p_in[cell - m_w].Z - 2.0f * p_in[cell].Z);
            p_out[cell].W = p_in[cell].W + a * (p_in[cell - 1].W + p_in[cell - m_w].W - 2.0f * p_in[cell].W);

            for (int x = 1; x < m_w - 1; x++)
            {
                for (int y = 1; y < m_h - 1; y++)
                {
                    cell = Cell(x, y);
                    p_out[cell].X = p_in[cell].X + a * (p_in[Cell(x, y + 1)].X + p_in[Cell(x, y - 1)].X + p_in[Cell(x + 1, y)].X + p_in[Cell(x - 1, y)].X - 4.0f * p_in[cell].X);
                    p_out[cell].Y = p_in[cell].Y + a * (p_in[Cell(x, y + 1)].Y + p_in[Cell(x, y - 1)].Y + p_in[Cell(x + 1, y)].Y + p_in[Cell(x - 1, y)].Y - 4.0f * p_in[cell].Y);
                    p_out[cell].Z = p_in[cell].Z + a * (p_in[Cell(x, y + 1)].Z + p_in[Cell(x, y - 1)].Z + p_in[Cell(x + 1, y)].Z + p_in[Cell(x - 1, y)].Z - 4.0f * p_in[cell].Z);
                    p_out[cell].W = p_in[cell].W + a * (p_in[Cell(x, y + 1)].W + p_in[Cell(x, y - 1)].W + p_in[Cell(x + 1, y)].W + p_in[Cell(x - 1, y)].Y - 4.0f * p_in[cell].W);
                }
            }
        }

        private void VorticityConfinement(float scale)
        {

	        ZeroEdge(ref mp_p1);
	        ZeroField(ref mp_xv1);
	        ZeroField(ref mp_yv1);

	        float[] p_abs_curl = mp_p1;

            for (int i = 1; i <= m_w-1; i++)
            {
                for (int j = 1; j <= m_h-1; j++)
                {
                    p_abs_curl[Cell(i, j)] = Math.Abs(Curl(i, j));
                }
            }

            for (int x = 2; x < m_w-1; x++)
            {
                for (int y = 2; y < m_h-1; y++)
                {

                    int cell = Cell(x,y);
			        
                    float lr_curl = (p_abs_curl[cell+1] - p_abs_curl[cell-1]) * 0.5f;
			        float ud_curl = (p_abs_curl[cell+m_w] - p_abs_curl[cell+m_h]) * 0.5f;
     
                    float length = (float) Math.Sqrt(lr_curl * lr_curl + ud_curl * ud_curl) + 0.000001f;
                    lr_curl /= length;
                    ud_curl /= length;

                    float v = Curl(x, y);

			        mp_xv1[Cell(x, y)] = -ud_curl *  v;   
                    mp_yv1[Cell(x, y)] =  lr_curl *  v;
      
		        }
            }
	        ForceFrom(ref mp_xv1, ref mp_xv0,scale);
	        ForceFrom(ref mp_yv1, ref mp_yv0,scale);
        }

        private float Curl(int x, int y)
        {
	        float x_curl = 0.0f;
            if (y < this.m_h-1)
	            x_curl = (mp_xv0[Cell(x, y + 1)] - mp_xv0[Cell(x, y - 1)]) * 0.5f;

	        float y_curl = 0.0f;
            if (x < this.m_w-1)
                y_curl = (mp_yv0[Cell(x + 1, y)] - mp_yv0[Cell(x - 1, y)]) * 0.5f;

            return x_curl - y_curl;
        }

        private void ZeroEdge(ref float[] p_in)
        {
	        for (int x = 0; x<m_w;x++)
	        {
		        p_in[x] = 0;
		        p_in[m_w * (m_h-1) + x] = 0;
	        }

	        for (int y = 1; y<m_h-1;y++)
	        {
		        p_in[y * m_w] = 0;
		        p_in[y * m_w + (m_w-1)] = 0;
	        }
        }

        private void ZeroField(ref float[] p_field)
        {
	        int size = m_w*m_h;
            
            for (int i = 0; i < size; i++)
                p_field[i] = 0;
        }

        
        private int Cell(int x, int y) 
        { 
            return x + m_w * y;
        }

        #endregion

        #region Public Methods

        public void AddValue(float[] p_in, float x, float y, float v)
        {
	        if (x<0 || y<0 || x>(float)m_w-1.0001f || y>(float)m_h-1.0001f)
		        return;

	        float fx = x-(int)x;
	        float fy = y-(int)y;
	        int cell = Cell((int)x,(int)y);
     
	        float ia = (1.0f-fy)*(1.0f-fx) * v;
	        float ib = (1.0f-fy)*(fx)      * v;
	        float ic = (fy)     *(1.0f-fx) * v;
	        float id = (fy)     *(fx)      * v;

	        p_in[cell] += ia;
	        p_in[cell+1] += ib;
	        p_in[cell+m_w] += ic;
	        p_in[cell+m_w+1] += id;
        }

        public void AddValue(Vector4[] p_in, float x, float y, float ink)
        {
            Vector4 v = currentColor * ink;
            if (x < 0 || y < 0 || x > (float)m_w - 1.0001f || y > (float)m_h - 1.0001f)
                return;

            float fx = x - (int)x;
            float fy = y - (int)y;
            int cell = Cell((int)x, (int)y);

            Vector4 ia;
            ia.X = (1.0f - fy) * (1.0f - fx) * v.X;
            ia.Y = (1.0f - fy) * (1.0f - fx) * v.Y;
            ia.Z = (1.0f - fy) * (1.0f - fx) * v.Z;
            ia.W = (1.0f - fy) * (1.0f - fx) * v.W;

            Vector4 ib;
            ib.X = (1.0f - fy) * (fx) * v.X;
            ib.Y = (1.0f - fy) * (fx) * v.Y;
            ib.Z = (1.0f - fy) * (fx) * v.Z;
            ib.W = (1.0f - fy) * (fx) * v.W;

            Vector4 ic;
            ic.X = (fy) * (1.0f - fx) * v.X;
            ic.Y = (fy) * (1.0f - fx) * v.Y;
            ic.Z = (fy) * (1.0f - fx) * v.Z;
            ic.W = (fy) * (1.0f - fx) * v.W;

            Vector4 id;
            id.X = (fy) * (fx) * v.X;
            id.Y = (fy) * (fx) * v.Y;
            id.Z = (fy) * (fx) * v.Z;
            id.W = (fy) * (fx) * v.W;

            p_in[cell].X += ia.X;
            p_in[cell].Y += ia.Y;
            p_in[cell].Z += ia.Z;
            p_in[cell].W += ia.W;

            p_in[cell + 1].X += ib.X;
            p_in[cell + 1].Y += ib.Y;
            p_in[cell + 1].Z += ib.Z;
            p_in[cell + 1].W += ib.W;

            p_in[cell + m_w].X += ic.X;
            p_in[cell + m_w].Y += ic.Y;
            p_in[cell + m_w].Z += ic.Z;
            p_in[cell + m_w].W += ic.W;

            p_in[cell + m_w + 1].X += id.X;
            p_in[cell + m_w + 1].Y += id.Y;
            p_in[cell + m_w + 1].Z += id.Z;
            p_in[cell + m_w + 1].W += id.W;
        }

        public void makeImpulse(float X, float Y, float dX, float dY, bool throwInk)
        {
            if (fluidLevelAtPosition((int)X, (int)Y) < 0.09f) return;
            float step = 0.1f;
#if !XBOX360
            float scale = 0.00007f;
#else
            float scale = 0.00047f;
#endif

            for (float x0 = -0.5f; x0 < 0.5f; x0 += step)
            {
                for (float y0 = -0.5f; y0 < 0.5f; y0 += step)
                {
                    float rr = (float)Math.Sqrt(x0 * x0 + y0 * y0);
                    if (!throwInk)
                    {
                        AddValue(mp_xv0, x0 * 10 + X, y0 * 10 + Y, dX * scale);
                        AddValue(mp_yv0, x0 * 10 + X, y0 * 10 + Y, dY * scale);
                    }
                    else
                    {
                        AddValue(mp_xv0, x0 + X, y0 + Y, dX * (1.0f - rr) * scale);
                        AddValue(mp_yv0, x0 + X, y0 + Y, dY * (1.0f - rr) * scale);

                        float r_ink = 0.005f * m_w;
                        float ink = 0.0002f * m_w;

                        AddValue(mp_ink0, x0 * r_ink + X, y0 * r_ink + Y, ink);
                    }
                }
            }
        }

        public void resetDensity()
        {
            //Buffer.BlockCopy(src,0,dst,0,);
            int size = m_w * m_h;
            for (int i = 0; i < size; i++)
            {
                mp_xv0[i] = 0.0f;
                mp_yv0[i] = 0.0f;
                mp_xv1[i] = 0.0f;
                mp_yv1[i] = 0.0f;
                mp_xv2[i] = 0.0f;
                mp_yv2[i] = 0.0f;
                mp_p0[i] = 1.0f;
                mp_p1[i] = 1.0f;
                mp_ink0[i] = densities[currentResetTexture][i];
            }
            currentResetTexture++;
            if (currentResetTexture >= densities.Length) currentResetTexture = 0;
            shouldResetDensity = false;
        }

        #endregion

    }
}
