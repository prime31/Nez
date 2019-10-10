float2 _lightSource; // light position in 2D world space
float3 _lightColor;  // color of the light
float  _lightRadius; // radius of the light
float4x4 _viewProjectionMatrix; // camera view-proj matrix


struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 WorldPos : TEXCOORD0;
};


VertexShaderOutput Vert(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(input.Position, _viewProjectionMatrix);
	output.WorldPos = input.TexCoord;

	return output;
}


float4 Pixel(VertexShaderOutput input, in float2 screenPos:VPOS) : COLOR0
{   
	// compute the relative position of the pixel and the distance towards the light
	float2 position = _lightSource - input.WorldPos;
	float distance = length(_lightSource - screenPos.xy);
	
	float attenuation = saturate(1.0f - distance/_lightRadius);
	
	return float4(attenuation, attenuation, attenuation, attenuation) * float4(_lightColor, 1);
}


technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 Vert();
		PixelShader = compile ps_3_0 Pixel();
	}
}
