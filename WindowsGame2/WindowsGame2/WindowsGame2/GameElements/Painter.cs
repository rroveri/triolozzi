using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowsGame2.GameElements
{
    public class Painter
    {
        public float dx;
        public float dy;
        public float ax;
        public float ay;
        public float div;
        public float ease;
        public Vector3 oldWVert;
        public Vector3 oldEVert;

        public float ease2;
        public float ease3;
        

        public Painter(Random seed)
        {
            dx = 0f;
            dy = 0f;
            ax = 0f;
            ay = 0f;
            //far away but nice
            //div=0.1f
            //ease = (float)seed.NextDouble() * 0.2f + 0.5f;
            
            //close, perfect if drawing everyframe 
            //div = 0.2f; 
            //ease = (float)seed.NextDouble() * 0.4f + 0.5f;

            //close, perfect if drawing every two frames 
           // div = 0.3f; 
           // ease = (float)seed.NextDouble() * 0.4f + 0.5f;

           // ease2 = (float)seed.NextDouble() * 0.4f + 0.5f;
           // ease3 = (float)seed.NextDouble() * 0.4f + 0.5f;

            //close without brush
            div = 0.3f;
            ease = (float)seed.NextDouble() * 0.4f + 0.5f;
        }
    }
}
