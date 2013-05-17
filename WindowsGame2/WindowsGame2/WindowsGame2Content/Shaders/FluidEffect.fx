struct VertexShaderInput
{
    float4 Position	: POSITION0;
    float2 TexCoord	: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float  rand		: TEXCOORD1;
    float2 TexCoord	: TEXCOORD0;
};

float4 Color;

texture Texture;

Texture2D random;

sampler randomSampler = sampler_state
{
    Texture = <random>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

sampler2D texSampler = sampler_state
{
	Texture = <Texture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.rand = input.Position[2];
    output.Position = float4(input.Position[0],input.Position[1],1,1);
    output.TexCoord = input.TexCoord;

    return output;
}

float textureSize = 64.0f;
float texelSize =  {1.0f / 64.0f};

float4 tex2Dbilinear( sampler2D tex, float4 uv )
{
        float4 height00 = tex2Dlod(tex, uv);
        float4 height10 = tex2Dlod(tex, uv + float4(texelSize, 0, 0, 0)); 
        float4 height01 = tex2Dlod(tex, uv + float4(0, texelSize, 0, 0)); 
        float4 height11 = tex2Dlod(tex, uv + float4(texelSize , texelSize, 0, 0));

        float2 f = frac( uv.xy * textureSize );

        float4 tA = lerp( height00, height10, f.x );
        float4 tB = lerp( height01, height11, f.x );

        return lerp( tA, tB, f.y );
}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//return tex2Dbilinear(texSampler, float4(input.TexCoord.xy,0,0));
    float4 final = tex2D(texSampler,input.TexCoord);
	float sum = final[0] + final[1] + final[2];
	final[3] = 1;
	if(sum < 0.3)
	{
		final[3] = 0;
	}
    else if(sum <= 0.4 && sum >= 0.3)
    {
        final[3] = (sum - 0.3) / 0.1;
    }
	if(length(input.TexCoord - float2(0.5,0.5)) >= 0.4)
	{
		final[3] = final[3] - max(0,(length(input.TexCoord - float2(0.5,0.5)) - 0.4) / 0.1);
    }
	if(final[0] < 0.05) final[0] = 0.05;
    if(final[1] < 0.3) final[1] = 0.3;
    if(final[2] < 0.1) final[2] = 0.1;
    //return float4(1,1,0,1);

	float2 access = float2(input.rand / 100,input.rand / 100);
	float rand = tex2D(randomSampler, access)[0];

    //if(length(input.TexCoord - float2(0.5,0.5)) > (0.7 + 0.2 * rand))

	return final;
}

technique Technique1
{
    pass Pass1
    {
		AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
