Texture2D currentTex;
float4 multColor;

sampler sampler = sampler_state
{
    Texture = <currentTex>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 uv		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 uv		: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;
	output.uv = input.uv;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texCol = tex2D(sampler, input.uv);
    return texCol*multColor;
}

technique BasicTechnique
{
    pass BasicPass
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
