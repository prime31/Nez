// ##### ##### ##### ##### ##### ##### #####
// ##### ##### Common Uniforms   ##### #####
// ##### ##### ##### ##### ##### ##### #####
SamplerState s0; // from SpriteBatch
texture _normalMap;
sampler _normalMapSampler = sampler_state { Texture = <_normalMap>; };
float4x4 _matrixTransform;

static const float3 _ambientColor = float3( 0.1, 0.1, 0.1 );
static const float3 _lightPosition = float3( -340, 360, 100 );
static const float _lightRadius = 700;
static const float3 _lightColor = float3( 1, 1, 1 );


struct VSOutput
{
	float4 position : POSITION;
	float4 color 	: COLOR0;
	float2 texCoord	: TEXCOORD0;
	float4 world	: TEXCOORD3;
};


VSOutput mainVS( float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0 )
{
	VSOutput output;
    output.position = mul( position, _matrixTransform );
	output.color = color;
	output.texCoord = texCoord;
	output.world = output.position;

	return output;
}


float4 mainPS( VSOutput input ) : COLOR
{
    // get normal data from the normalMap
    float4 normalData = tex2D( _normalMapSampler, input.texCoord );

    // tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0;
    //normal.y *= -1.0;
	
	float4 color = tex2D( s0, input.texCoord );
	
    // get out of here if we fail the alpha test
	clip( ( color.a < 0.2 ) ? -1 : 1 );
	
	
	
    // surface-to-light vector
    float3 lightVector = _lightPosition - input.world.xyz;
	float attenuation = saturate( 1.0f - length( lightVector ) / _lightRadius );
	
    // normalize light vector
    lightVector = normalize( lightVector );

    // compute diffuse light
    float NdL = max( 0, dot( normal, lightVector ) );
	
	float3 diffuse = NdL * _lightColor.rgb;
	
	
	// Combine all of these values into one (including the ambient light)
	color.rgb = saturate( color.rgb * diffuse + _ambientColor );
	
	//return color * 0.00001 + float4( color.rgb * diffuse, 1 );
	return color;
}


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Techniques        ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

technique Diffuse
{
	pass P0
	{
		VertexShader = compile vs_2_0 mainVS();
		PixelShader = compile ps_2_0 mainPS();
	}
}