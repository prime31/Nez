sampler s0;
//sampler _renderTexture;
sampler _normalMap;

SamplerState _renderTexture
{
    AddressU = Clamp;
    AddressV = Clamp;
    MagFilter = Point;
    MinFilter = Point;
};

float4x4 _matrixTransform;
float _reflectionIntensity; // 0.4
float _normalMagnitude; // 0.05

// water only
float _time;
float _screenSpaceVerticalOffset;



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Mirror Reflection       ##### ##### ##### ##### ##### ####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
    float2 texCoord		: TEXCOORD0;
    float4 world		: TEXCOORD3;
};


VSOutput mirrorVertex( float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0 )
{
	VSOutput output;
    output.position = mul( position, _matrixTransform );
	output.color = color;
	output.texCoord = texCoord;
	output.world = output.position;

	return output;
}


float4 mirrorPixel( VSOutput input ) : COLOR0
{
    float2 screenPos = input.world.xy / input.world.w;
    float2 screenSpaceTexCoord = 0.5f * float2( screenPos.x, -screenPos.y ) + 0.5f;

	// fetch and transform our normal data before we sample the renderTarget for our reflection
	float4 normalData = tex2D( _normalMap, input.texCoord );
	
    // transform normal back into [-1,1] range
    float2 normal = 2.0f * normalData.xy - 1.0;

    float4 color = tex2D( s0, input.texCoord ) * input.color;
	float4 reflection = tex2D( _renderTexture, screenSpaceTexCoord + normal * _normalMagnitude );
	
	color.rgb = color.rgb * ( 1.0 - reflection.a * _reflectionIntensity ) + reflection.rgb * reflection.a * color.a * _reflectionIntensity;
	return color;
}


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Water Reflection        ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

struct WaterVertexOut
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
    float2 texCoord		: TEXCOORD0;
    float4 world		: TEXCOORD3;
	float4 uvgrab		: TEXCOORD1;
};

WaterVertexOut waterVertex( float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0 )
{
	WaterVertexOut output;
    output.position = mul( position, _matrixTransform );
	output.color = color;
	output.texCoord = texCoord;
	output.world = output.position;
	
	// composite coordinates are [-1,1] so we add 1 (pos.w) and multiply by 0.5 to get in the 0-1 range for uvs
	output.uvgrab.xy = ( output.position.xy + output.position.w ) * 0.5;
	output.uvgrab.zw = output.position.zw;
	output.uvgrab.y += 0.58;
	
	float4 top = mul( _matrixTransform, float4( 0, 0.5, 0, 1 ) );
	top.xy /= top.w;
	//output.uvgrab.y = 1 - ( output.uvgrab.y + top.y );
	
	return output;
}


float4 waterPixel( WaterVertexOut input ) : COLOR0
{
    //float2 screenPos = input.world.xy / input.world.w;
    //float2 screenSpaceTexCoord = 0.5f * float2( screenPos.x, -screenPos.y ) + 0.5f;

	//half2 perspectiveCorrection = half2( 2.0f * ( 0.5 - input.texCoord.x ) * input.texCoord.y, 0.0f );
	//half2 perspectiveCorrection = half2( 2.0f * ( 0.5 - input.uvgrab.x ) * input.uvgrab.y, 0.0f );
	//perspectiveCorrection.x *= 0;
	
	float factor = ( 0.5 - input.uvgrab.x ) * ( input.uvgrab.y - 0.65 );
	//float factor = ( 0.5 - input.uvgrab.x ) * ( 1 - input.texCoord.y );
	half2 perspectiveCorrection = half2( factor, 0 );
	
	
	// start with perspectiveCorrection uv and sample our normal twice
	float2 normTexCoord = input.uvgrab.xy; // show we be doing this? - perspectiveCorrection
	normTexCoord.x = frac( normTexCoord.x + _time / 16.0 );
	float4 normal1 = tex2D( _normalMap, normTexCoord );
	normal1.xy -= 0.5; // get red and green channels in -0.5 to 0.5 range
	
	float2 normTexCoord2 = input.uvgrab.xy * 3.0;
	normTexCoord2.x = frac( normTexCoord2.x + _time / 50.0 );
	float4 normal2 = tex2D( _normalMap, normTexCoord2 );
	normal2.xy -= 0.5; // get red and green channels in -0.5 to 0.5 range
	
	// use the _normalMagnitude to get a uv offset
	float2 normalOffset = ( normal1.xy + normal2.xy ) * _normalMagnitude;
	//normalOffset = 0.0001 * normalOffset;


    float4 color = tex2D( s0, input.texCoord ) * input.color;
	float2 reflectionTexCoord = input.uvgrab.xy + normalOffset - perspectiveCorrection;
	float4 reflection = tex2D( _renderTexture, reflectionTexCoord );
	
	color.rgb = ( 1.0 - reflection.a * _reflectionIntensity ) + reflection.rgb * reflection.a * color.a * _reflectionIntensity;
	
	// if our waves get higher than our original uv.y than add a white cap
	float _sparkleIntensity = 0.015;
	if( ( reflectionTexCoord.y - input.uvgrab.y ) > _sparkleIntensity )
	{
		color.rgb = float3( 1, 1, 1 );
	}
	
	//color.rgb = color.rgb * 0.000000001 + float3( input.texCoord.y, input.texCoord.y, input.texCoord.y );
	return color;
}



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Techniques        ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

technique MirrorTechnique
{
    pass Pass1
    {
    	VertexShader = compile vs_2_0 mirrorVertex();
        PixelShader = compile ps_2_0 mirrorPixel();
    }
}

technique WaterReflectionTechnique
{
    pass Pass1
    {
    	VertexShader = compile vs_2_0 waterVertex();
        PixelShader = compile ps_2_0 waterPixel();
    }
}