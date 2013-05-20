
Texture2D postitHappy, postitSad, lap, numbers;
Texture2D postitHappy_NW, postitHappy_NE, postitHappy_SW, postitHappy_SE;
Texture2D postitSad_NW, postitSad_NE, postitSad_SW, postitSad_SE;
Texture2D pigiama_NW, pigiama_NE, pigiama_SW, pigiama_SE;
Texture2D awake_NW, awake_NE, awake_SW, awake_SE;
Texture2D pencil;

float4 nLaps;

sampler pencilSampler = sampler_state
{
    Texture = <pencil>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler pigiamaNWSampler = sampler_state
{
    Texture = <pigiama_NW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler pigiamaNESampler = sampler_state
{
    Texture = <pigiama_NE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler pigiamaSWSampler = sampler_state
{
    Texture = <pigiama_SW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler pigiamaSESampler = sampler_state
{
    Texture = <pigiama_SE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler awakeNWSampler = sampler_state
{
    Texture = <awake_NW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler awakeNESampler = sampler_state
{
    Texture = <awake_NE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler awakeSWSampler = sampler_state
{
    Texture = <awake_SW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler awakeSESampler = sampler_state
{
    Texture = <awake_SE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler postitHappyNWSampler = sampler_state
{
    Texture = <postitHappy_NW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitHappyNESampler = sampler_state
{
    Texture = <postitHappy_NE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitHappySWSampler = sampler_state
{
    Texture = <postitHappy_SW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitHappySESampler = sampler_state
{
    Texture = <postitHappy_SE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler postitSadNWSampler = sampler_state
{
    Texture = <postitSad_NW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitSadNESampler = sampler_state
{
    Texture = <postitSad_NE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitSadSWSampler = sampler_state
{
    Texture = <postitSad_SW>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};
sampler postitSadSESampler = sampler_state
{
    Texture = <postitSad_SE>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler postitHappySampler = sampler_state
{
    Texture = <postitHappy>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler postitSadSampler = sampler_state
{
    Texture = <postitSad>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler lapSampler = sampler_state
{
    Texture = <lap>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler numberSampler = sampler_state
{
    Texture = <numbers>;
	MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
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

float4 PixelShaderFunctionPostitNW(PostitVertexShaderOutput input) : COLOR0
{
    float4 texCol;
    if(input.Color[0] == 0)
    {
        texCol = tex2D(postitSadNWSampler, input.uv);
    }
    else
    {
        texCol = tex2D(postitHappyNWSampler, input.uv);
    }
	return texCol;
}
float4 PixelShaderFunctionPostitNE(PostitVertexShaderOutput input) : COLOR0
{
    float4 texCol;
    if(input.Color[0] == 0)
    {
        texCol = tex2D(postitSadNESampler, input.uv);
    }
    else
    {
        texCol = tex2D(postitHappyNESampler, input.uv);
    }
	return texCol;
}

float4 PixelShaderFunctionPostitSW(PostitVertexShaderOutput input) : COLOR0
{
    float4 texCol;
    if(input.Color[0] == 0)
    {
        texCol = tex2D(postitSadSWSampler, input.uv);
    }
    else
    {
        texCol = tex2D(postitHappySWSampler, input.uv);
    }
	return texCol;
}

float4 PixelShaderFunctionPostitSE(PostitVertexShaderOutput input) : COLOR0
{
    float4 texCol;
    if(input.Color[0] == 0)
    {
        texCol = tex2D(postitSadSESampler, input.uv);
    }
    else
    {
        texCol = tex2D(postitHappySESampler, input.uv);
    }
	return texCol;
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

float4 PixelShaderFunctionPigiamaNW(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(pigiamaNWSampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionPigiamaNE(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(pigiamaNESampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionPigiamaSW(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(pigiamaSWSampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionPigiamaSE(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(pigiamaSESampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionAwakeNW(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(awakeNWSampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionAwakeNE(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(awakeNESampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionAwakeSW(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(awakeSWSampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionAwakeSE(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(awakeSESampler, input.uv);
	texCol *= input.Color;
	return texCol;
}

float4 PixelShaderFunctionPencil(LapVertexShaderOutput input) : COLOR0
{
    float4 texCol = tex2D(pencilSampler, input.uv);
    if(texCol[0] == texCol[1] && texCol[1] == texCol[2])
	    texCol *= input.Color;
	return texCol;
}


technique ScreenTechinque
{

	pass PencilPass
	{
		VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPencil();
	}
	pass PostitPassNW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPostitNW();
    }
	pass PostitPassNE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPostitNE();
    }
	pass PostitPassSW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPostitSW();
    }
	pass PostitPassSE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPostitSE();
    }

	pass PigiamaPassNW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPigiamaNW();
    }
	pass PigiamaPassNE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPigiamaNE();
    }
	pass PigiamaPassSW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPigiamaSW();
    }
	pass PigiamaPassSE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionPigiamaSE();
    }

	pass AwakePassNW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionAwakeNW();
    }
	pass AwakePassNE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionAwakeNE();
    }
	pass AwakePassSW
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionAwakeSW();
    }
	pass AwakePassSE
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionPostit();
        PixelShader = compile ps_3_0 PixelShaderFunctionAwakeSE();
    }
	
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
