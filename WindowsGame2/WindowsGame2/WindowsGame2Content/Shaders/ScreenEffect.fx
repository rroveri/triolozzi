
Texture2D postitHappy, postitSad, lap, numbers;

float4 nLaps;

sampler postitHappySampler = sampler_state
{
    Texture = <postitHappy>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler postitSadSampler = sampler_state
{
    Texture = <postitSad>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler lapSampler = sampler_state
{
    Texture = <lap>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler numberSampler = sampler_state
{
    Texture = <numbers>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

struct PostitVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct BarVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct LapVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct NLapVertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
	float2 uv		: TEXCOORD0;
};

struct PostitVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

struct BarVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

struct LapVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

struct NLapVertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

PostitVertexShaderOutput VertexShaderFunctionPostit(PostitVertexShaderInput input)
{
    PostitVertexShaderOutput output;
    output.Position = input.Position;
    output.Position[1] *= -1;
	output.Color = input.Color;
    output.uv = input.uv;

    return output;
}

LapVertexShaderOutput VertexShaderFunctionLap(LapVertexShaderInput input)
{
    LapVertexShaderOutput output;
    output.Position = input.Position;
    output.Position[1] *= -1;
	output.Color = input.Color;
    output.uv = input.uv;

    return output;
}

NLapVertexShaderOutput VertexShaderFunctionNLap(NLapVertexShaderInput input)
{
    NLapVertexShaderOutput output;
    output.Position = input.Position;
    output.Position[1] *= -1;
	output.Color = input.Color;
    output.uv = input.uv;

    return output;
}

BarVertexShaderOutput VertexShaderFunctionBar(BarVertexShaderInput input)
{
    BarVertexShaderOutput output;
    output.Position = input.Position;
    output.Position[1] *= -1;
	output.Color = input.Color;
    output.uv = input.uv;

    return output;
}

float4 PixelShaderFunctionPostit(PostitVertexShaderOutput input) : COLOR0
{
    float4 texCol;
    if(input.Color[0] == 0)
    {
        texCol = tex2D(postitSadSampler, input.uv);
    }
    else
    {
        texCol = tex2D(postitHappySampler, input.uv);
    }
	return texCol;
}

float4 PixelShaderFunctionLap(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(lapSampler, input.uv);
	return texCol;
}

float4 PixelShaderFunctionNLap(NLapVertexShaderOutput input) : COLOR0
{
    float2 uv = float2(input.uv[0], input.uv[1]);
    float4 texCol = tex2D(numberSampler, uv);
    return texCol;
}

float4 PixelShaderFunctionBar(BarVertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique ScreenTechinque
{
    pass PostitPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPostit();
    }

    pass BarPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionBar();
        PixelShader = compile ps_3_0 PixelShaderFunctionBar();
    }

    pass LapPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionLap();
        PixelShader = compile ps_3_0 PixelShaderFunctionLap();
    }

    pass NLapPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionNLap();
        PixelShader = compile ps_3_0 PixelShaderFunctionNLap();
    }
}
