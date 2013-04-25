/*
    Fluid.fx - An efficient HLSL fluid shader
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

// Samplers, from DrawFluid
sampler2D buffer : register(s0);
sampler2D Velocity : register(s1);
sampler2D Density : register(s2);
sampler2D Divergence : register(s3);
sampler2D Pressure : register(s4);
sampler2D VelocitySources : register(s5);
sampler2D DensitySources : register(s6);

Texture2D finalTexture;

sampler finalSampler = sampler_state
{
    Texture = <finalTexture>;
	MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

// Dimensions of the textures making up the fluid stages
float FluidSize = 256.0f;
float dT = 1.0f; // Delta time
float VelocityDiffusion = 1.0f;
float DensityDiffusion = 1.0f;
float4 VelocityColor;

// Struct for when we use two render targets
struct DoubleOutput
{
	float4 Vel : COLOR0;
	float4 Den : COLOR1;
};

struct FinalVertexInput
{
	float4 Position : POSITION;
	float2 uv		: TEXCOORD0;
};

struct FinalVertexOutput
{
	float4 Position : POSITION;
	float2 uv		: TEXCOORD1;
};

// Bilerps between the 4 closest texels
float4 QuadLerp(sampler2D samp, float2 s)
{
  float x0 = floor(s.x*FluidSize);
  float x2 = x0 + 1.f;
  float y0 = floor(s.y*FluidSize);
  float y1 = y0 + 1.f;
  
  float4 tex12 = tex2D(samp, float2(x0/FluidSize, y1/FluidSize)); 
  float4 tex22 = tex2D(samp, float2(x2/FluidSize, y1/FluidSize));   
  float4 tex11 = tex2D(samp, float2(x0/FluidSize, y0/FluidSize)); 
  float4 tex21 = tex2D(samp, float2(x2/FluidSize, y0/FluidSize)); 
  
  float fx = ((s.x*FluidSize) - x0);
  float fy = ((s.y*FluidSize) - y0);

  float4 l1 = lerp(tex11, tex21, fx);
  float4 l2 = lerp(tex12, tex22, fx);

  return lerp(l1, l2, fy);
}

// Sets the edges of the texture, clips everything else
float2 SetBounds(float2 v)
{
	float2 pos = v;
	float Offset = (1.0f/(FluidSize));
	float HalfPixel = (.5f/FluidSize);
	if(pos.x <  HalfPixel)
        pos.x += Offset;
    else if(pos.y <  HalfPixel)
        pos.y += Offset;
    else if(pos.x >=  (1.0f-HalfPixel)-Offset)
        pos.x -= Offset;
    else if(pos.y >=  (1.0f-HalfPixel)-Offset)
		pos.y -= Offset;		
	else
		clip(-1);
	return pos;
}

float4 PSVelocityColorize(float2 TexCoords : TEXCOORD0) : COLOR0
{
	return (tex2D(buffer, TexCoords) * -VelocityColor) * FluidSize;
}

// Adds the sources to Density and Velocity
DoubleOutput PSAddSources(float2 TexCoords : TEXCOORD0)
{
	DoubleOutput Output;	
	float4 uv = tex2D(VelocitySources, TexCoords - float2(.5f/FluidSize, .5f/FluidSize));	
	Output.Vel = max(-.8f, min(.8f, tex2D(Velocity, TexCoords - float2(.5f/FluidSize, .5f/FluidSize)) + uv));
	Output.Den = tex2D(Density, TexCoords - float2(.5f/FluidSize, .5f/FluidSize)) + (tex2D(DensitySources, TexCoords - float2(.5f/FluidSize, .5f/FluidSize))/8.0f);	
	Output.Vel.w = 1.0f;
	Output.Den.w = 1.0f;
	return Output;
}

// Sets the boundaries for Density and Velocity
DoubleOutput PSSetBoundsDouble(float2 TexCoords : TEXCOORD0)
{
	DoubleOutput Output;
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);
    Pos = SetBounds(Pos);
	Output.Vel = -tex2D(Velocity, Pos);
	Output.Den = tex2D(Density, Pos);
	return Output;
}

// Sets the boundries for Pressure
float4 PSSetBoundsSingle(float2 TexCoords : TEXCOORD0) : COLOR0
{	
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);
    Pos = SetBounds(Pos);
	return tex2D(buffer, Pos);	
}

// Advects Velocity and Density
DoubleOutput PSAdvection(float2 TexCoords : TEXCOORD0)
{
	DoubleOutput Output;
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);	
    Pos -= tex2D(Velocity, Pos) / FluidSize;	
	Output.Vel = VelocityDiffusion*QuadLerp(Velocity, Pos);
	Output.Den = DensityDiffusion*QuadLerp(Density, Pos);    
    return Output;
}

// Calculates and spits out a divergence texture
float4 PSDivergence(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);
	float Offset = 1.0f/FluidSize;	
	float4 left   = tex2D(buffer, float2(Pos.x - Offset, Pos.y));
	float4 right  = tex2D(buffer, float2(Pos.x + Offset, Pos.y));
	float4 top    = tex2D(buffer, float2(Pos.x, Pos.y - Offset));
	float4 bottom = tex2D(buffer, float2(Pos.x, Pos.y + Offset));
	return .5f * ((right.x - left.x) + (bottom.y - top.y));
}

// Calculates and spits out the pressure texture over a series of iterations
float4 PSJacobi(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);
	float Offset = 1.0f/FluidSize;	
	float4 center = tex2D(Divergence, Pos);
	float4 left   = tex2D(buffer, float2(Pos.x - Offset, Pos.y));
	float4 right  = tex2D(buffer, float2(Pos.x + Offset, Pos.y));
	float4 top    = tex2D(buffer, float2(Pos.x, Pos.y - Offset));
	float4 bottom = tex2D(buffer, float2(Pos.x, Pos.y + Offset));
	return ((left + right + bottom + top) - center) * .25f;
}

// Subtracts the pressure texture from the velocity texture
float4 PSSubtract(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 Pos = TexCoords - float2(.5f/FluidSize, .5f/FluidSize);	
	float Offset = 1.0f/FluidSize;
	float left   = tex2D(Pressure, float2(Pos.x - Offset, Pos.y)).x;
	float right  = tex2D(Pressure, float2(Pos.x + Offset, Pos.y)).x;
	float top    = tex2D(Pressure, float2(Pos.x, Pos.y - Offset)).y;
	float bottom = tex2D(Pressure, float2(Pos.x, Pos.y + Offset)).y;

	float2 grad = float2(right-left, bottom-top) * .5f;
	float4 v = tex2D(buffer, Pos);
	v.xy -= grad;
	return v;
}

FinalVertexOutput VSFinal(FinalVertexInput input)
{
	FinalVertexOutput output;
    output.Position = input.Position;
	output.Position[1] *= -1;
    output.uv = input.uv;

    return output;
}

float4 PSFinal(FinalVertexOutput input) : COLOR0
{
	float4 final = QuadLerp(finalSampler,input.uv);
    float sum = final[0] + final[1] + final[2];
	/*float alpha = final[0];
	final = float4(0.0,1,0.0,alpha);*/
    if(sum < 0.3)
    {
        final[3] = sum*sum;
    }
    if(final[0] < 0.05) final[0] = 0.05;
    if(final[1] < 0.3) final[1] = 0.3;
    if(final[2] < 0.1) final[2] = 0.1;
    //return float4(1,1,0,1);
	return final;
}

// Techniques and passes
Technique VelocityColorize
{
	pass VelocityColorize
	{
		PixelShader = compile ps_2_0 PSVelocityColorize();
	}
}

technique DoAddSources
{
	pass DoAddSources
	{
		PixelShader = compile ps_2_0 PSAddSources();
	}
	pass SetBounds
	{
		PixelShader = compile ps_2_0 PSSetBoundsDouble();
	}
}

technique DoAdvection
{
	pass DoAdvection
	{
		PixelShader = compile ps_2_0 PSAdvection();
	}
	pass SetBounds
	{
		PixelShader = compile ps_2_0 PSSetBoundsDouble();
	}
}

technique DoDivergence
{
	pass DoDivergence
	{
		PixelShader = compile ps_2_0 PSDivergence();
	}
}

Technique DoJacobi
{
	pass DoJacobi
	{
		PixelShader = compile ps_2_0 PSJacobi();
	}
	pass SetBounds
	{
		PixelShader = compile ps_2_0 PSSetBoundsSingle();
	}
}

Technique Subtract
{
	pass Subtract
	{
		PixelShader = compile ps_2_0 PSSubtract();
	}
}

Technique Final
{
	pass Final
	{
        AlphaBlendEnable = TRUE;
        //DestBlend = INVSRCALPHA;
        //SrcBlend = SRCALPHA;
		VertexShader = compile vs_2_0 VSFinal();
		PixelShader = compile ps_2_0 PSFinal();
	}
}
