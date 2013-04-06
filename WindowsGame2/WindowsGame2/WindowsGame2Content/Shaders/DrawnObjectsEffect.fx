float4x4 View;
float4x4 Projection;

float2 redCarPos,blueCarPos,greenCarPos,pinkCarPos;

Texture2D trailSketch;
Texture2D objectSketch;
Texture2D random;

float objetAlpha = 0.2;

sampler trailSketchSampler = sampler_state
{
    Texture = <trailSketch>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler objectSketchSampler = sampler_state
{
    Texture = <objectSketch>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler randomSampler = sampler_state
{
    Texture = <random>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

struct TrailVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct ObjectVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct ObjectVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
	float2 xy       : TEXCOORD1;
};

struct TrailVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

ObjectVertexShaderOutput VertexShaderFunctionObject(ObjectVertexShaderInput input)
{
    ObjectVertexShaderOutput output;

    //float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(input.Position, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
	output.xy = float2(input.Position[0],input.Position[1]);
    output.uv = input.uv;

    return output;
}

TrailVertexShaderOutput VertexShaderFunctionTrail(TrailVertexShaderInput input)
{
    TrailVertexShaderOutput output;

    //float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(input.Position, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
    output.uv = input.uv;

    return output;
}

float4 PixelShaderFunctionObject(ObjectVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(objectSketchSampler, input.uv / 4);
    float alpha = texCol[0] + objetAlpha;
    texCol *= input.Color;
	texCol += input.Color * objetAlpha;
    float rand = tex2D(randomSampler, input.xy)[0];

    // Moses Effect
	float2 carPos;
	if(input.Color[0] == 1)
	{
		carPos = redCarPos;
	}
	else if(input.Color[1] == 1)
	{
		carPos = greenCarPos;
	}
	else if(input.Color[2] == 1)
	{
		carPos = blueCarPos;
	}
	else
	{
		carPos = pinkCarPos;
	}
    if(length(carPos - input.xy) < (0.2 + 0.4 * rand))
        return float4(0,0,0,0);
    
    return float4(texCol[0],texCol[1],texCol[2],alpha);

    //float4 rand = tex2D(randomSampler, input.xy)[0];
    //float alpha = rand * cos(input.xy[0] + input.xy[1]) + sin(input.xy[0] - input.xy[1]);
    //float4 texCol = input.Color;
    //return float4(texCol[0],texCol[1],texCol[2],alpha);
}

float4 PixelShaderFunctionTrail(TrailVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(trailSketchSampler, input.uv);
    float alpha = texCol[0];
    texCol *= input.Color;
    return float4(texCol[0],texCol[1],texCol[2],alpha);
}

technique DoodleTechinque
{
    pass ObjectPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionObject();
        PixelShader = compile ps_3_0 PixelShaderFunctionObject();
    }

    pass TrailPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionTrail();
        PixelShader = compile ps_3_0 PixelShaderFunctionTrail();
    }
}
