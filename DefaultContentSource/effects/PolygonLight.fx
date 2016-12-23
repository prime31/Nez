static const float3 black = float3( 0.0f, 0.0f, 0.0f );

float2 lightSource; // light position in 2D world space
float3 lightColor;  // color of the light
float  lightRadius; // radius of the light
float4x4 viewProjectionMatrix; // camera view-proj matrix


struct VertexShaderInput
{
	float4 position : POSITION0;
	float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 position : POSITION0;
	float2 worldPos : TEXCOORD0;
};


VertexShaderOutput mainVS( VertexShaderInput input )
{
	VertexShaderOutput output;
	//output.position = float4( input.Position, 1 );
	output.position = mul( input.position, viewProjectionMatrix );

	// vertex position in 2D world space
	output.worldPos = input.texCoord;

	return output;
}


float4 mainPS( VertexShaderOutput input ) : COLOR0
{   
	// compute the relative position of the pixel and the distance towards the light
	float2 position = input.worldPos - lightSource;     
	float dist = sqrt( dot( position, position ) );

	// mix between black and the light color based on the distance and the power of the light
	float3 mix = lerp( lightColor, black, saturate( dist / lightRadius ) );
	return float4( mix, 1.0f );
}


technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 mainVS();
		PixelShader = compile ps_2_0 mainPS();
	}
}
