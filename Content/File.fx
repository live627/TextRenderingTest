#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV		: TEXCOORD0;
	float4 Color    : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV		: TEXCOORD0;
	float4 Color    : COLOR0;
};

texture Texture;

sampler tex = sampler_state
{
    Texture = (Texture);

};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, Projection);
	output.UV = input.UV;
	output.Color = input.Color;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return tex2D(tex,input.UV).a * input.Color;
}

float4 ShadowPS(VertexShaderOutput input) : COLOR0
{
	float mask = tex2D(tex,input.UV).a;
	float4 clr = input.Color;
	
	if (mask < 0.7)
	{
		float2 offset = float2(1.0f / 1024.0f, -1.0f / 1024.0f);
		mask = tex2D(tex, input.UV + offset).a;
		if (mask < 0.7)
			discard;
		else
			clr *= 0.5f;
	}
	
	// do some anti-aliasing
	clr.a *= smoothstep(0.125, 1.00, mask);
	
	return clr;
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();		
	}
};

technique Technique2
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();		
		PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
