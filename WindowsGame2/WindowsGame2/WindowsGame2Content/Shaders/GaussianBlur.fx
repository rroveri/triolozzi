
float sampleWeights[15];
float2 sampleOffsets[15];

sampler inputTexture : register(s0);


struct PS_INPUT
{
	float4 Position	: POSITION;
	float2 TexCoords : TEXCOORD0;
};

float4 GaussianBlur_PS (PS_INPUT Input) : COLOR0
{
	float4 color = 0.0f;
	
	for(int i = 0; i < 15; i++ )
	{
		color += tex2D(inputTexture, Input.TexCoords + sampleOffsets[i]) * sampleWeights[i];
	}
	
	return color;
}

technique Blur
{
	pass P0
	{
		PixelShader = compile ps_2_0 GaussianBlur_PS();
	}
}