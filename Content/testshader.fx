#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float fAmplitude, fFrequency, fPeriods, fDistortStr, fWaveFrequency, fWaveAmplitude, fWavePeriods, fPixelWidth, fPixelHeight;

struct VertexShaderInput
{
	float4 pos : POSITION0;
	float4 color : COLOR0;
};

struct VertexShaderOutput
{
	float4 pos : SV_POSITION;
	float4 color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.pos = mul(input.pos, WorldViewProjection);
	output.color = input.color;

	return output;
}

//float4 MainPS(VertexShaderOutput input) : COLOR
//{
//
//	float4 color = tex2D(bkd, float2(input.pos.x, input.pos.y));
//	float4 distortColor = tex2D(distort, float2(input.pos.x, input.pos.y));
//	float2 offset = float2(fFrequency * fPixelWidth, fFrequency * fPixelHeight);
//	offset.x = distortColor.r * fDistortStr;
//	offset.y = distortColor.g * fDistortStr;
//
//	color = tex2D(bkd, float2(In.x + (sin((In.x + fFrequency) * fPeriods) * fAmplitude) + offset.x,In.y + (sin((In.x + fFrequency) * fPeriods) * fAmplitude) + offset.y));
//
//	color += fColor;
//
//	if (waveEnabled) {
//	color.a = smoothstep(0.01,0.03, In.y + (sin((In.x + fWaveFrequency * fPixelHeight) * fWavePeriods) * fWaveAmplitude) + offset);
//
//	return input.color;
//}



sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
	Texture = <Texture>;          //apply a texture to the sampler
	MipFilter = Point;         //sampler states
	MinFilter = Point;
	MagFilter = Point;
	Filter = Point;
	AddressU = mirror;
	AddressV = mirror;
};



struct vs2ps
{
	float4 Pos : POSITION;
	float4 TexCd : TEXCOORD0;
	float3 PosW : TEXCOORD1;
};

float Frequency = 25;
float Phase = 2;
float Amplitude = 3.5;
float4 PS(vs2ps In) : COLOR
{
	float2 cord = In.TexCd;
	cord.x += sin(cord.y * Frequency + Phase) * Amplitude;
	float4 col = tex2D(Samp, cord);
	return col;
}

technique BasicColorDrawing
{
	pass P0
	{
		//VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL PS();
	}
};