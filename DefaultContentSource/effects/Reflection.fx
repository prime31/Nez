sampler s0;
sampler _renderTexture;
sampler _normalMap;

float4x4 _matrixTransform;
float _reflectionIntensity; // 0.4
float _normalMagnitude; // 0.05


struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
    float2 texCoord		: TEXCOORD0;
    float4 world		: TEXCOORD3;
};


VSOutput vertexShader(	float4 position	: POSITION0,
						float4 color	: COLOR0,
						float2 texCoord	: TEXCOORD0 )
{
	VSOutput output;
    output.position = mul( position, _matrixTransform );
	output.color = color;
	output.texCoord = texCoord;
	output.world = output.position;

	return output;
}


float4 pixelShader( VSOutput input ) : COLOR0
{
    float2 screenPos = input.world.xy / input.world.w;
    float2 screenSpaceTexCoord = 0.5f * float2( screenPos.x, -screenPos.y ) + 0.5f;

	// fetch and transform our normal data before we sample the renderTarget for our reflection
	float4 normalData = tex2D( _normalMap, input.texCoord );
	
    // tranform normal back into [-1,1] range
    float2 normal = 2.0f * normalData.xy - 1.0;

    float4 color = tex2D( s0, input.texCoord ) * input.color;
	float4 reflection = tex2D( _renderTexture, screenSpaceTexCoord + normal * _normalMagnitude );
	
	color.rgb = color.rgb * ( 1.0 - reflection.a * _reflectionIntensity ) + reflection.rgb * reflection.a * color.a * _reflectionIntensity;
	return color;
    return lerp( color, reflection, _reflectionIntensity ) * color.a;
}


technique Technique1
{
    pass Pass1
    {
    	VertexShader = compile vs_2_0 vertexShader();
        PixelShader = compile ps_2_0 pixelShader();
    }
}