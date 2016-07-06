sampler s0;

texture _normalMap;
sampler _normalMapSampler = sampler_state { Texture = <_normalMap>; };

texture _renderTexture;
sampler2D _renderTextureSampler = sampler_state
{
    Texture = <_renderTexture>;
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
float _sparkleIntensity; // 0.015;
float3 _sparkleColor; // float3( 1, 1, 1 );
float _perspectiveCorrectionIntensity;
float _secondDisplacementScale;
float _firstDisplacementSpeed; // 16
float _secondDisplacementSpeed; // 50


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
	float4 normalData = tex2D( _normalMapSampler, input.texCoord );
	
    // transform normal back into [-1,1] range
    float2 normal = 2.0f * normalData.xy - 1.0;

    float4 color = tex2D( s0, input.texCoord ) * input.color;
	float4 reflection = tex2D( _renderTextureSampler, screenSpaceTexCoord + normal * _normalMagnitude );
	
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
	
	// we have to offset our uvgrab by the normalized position in screen space of our water plane. The Effect handles normalizing it to the
	// [-1,1] range that we need.
	output.uvgrab.y += _screenSpaceVerticalOffset;
	
	return output;
}


float4 waterPixel( WaterVertexOut input ) : COLOR0
{
	// calculate perspective correction. we want it to be 0 in the center of the screen and greater/lesser as we get to the left/right. We subtract
	// the uv from 0.5 to get a value that is in the -0.5 - 0.5 range to do this. The further towards the bottom of the screen that we are, the
	// greater the perspective correction.
	float factor = ( 0.5 - input.uvgrab.x ) * input.texCoord.y;
	half2 perspectiveCorrection = half2( factor * _perspectiveCorrectionIntensity, 0 );
	
	// start with the main uv and sample our normal twice
	float2 normTexCoord = input.texCoord.xy; // show we be doing this? input.uvgrab.xy - perspectiveCorrection
	normTexCoord.x = frac( normTexCoord.x + _time * _firstDisplacementSpeed );
	float4 normal1 = tex2D( _normalMapSampler, normTexCoord );
	normal1.xy -= 0.5; // get red and green channels in -0.5 to 0.5 range
	
	float2 normTexCoord2 = input.texCoord.xy * _secondDisplacementScale;
	normTexCoord2.x = frac( normTexCoord2.x + _time * _secondDisplacementSpeed );
	float4 normal2 = tex2D( _normalMapSampler, normTexCoord2 );
	normal2.xy -= 0.5; // get red and green channels in -0.5 to 0.5 range
	
	// use the _normalMagnitude to get a uv offset
	float2 normalOffset = ( normal1.xy + normal2.xy ) * _normalMagnitude;


    float4 color = tex2D( s0, input.texCoord );
	float2 reflectionTexCoord = input.uvgrab.xy + normalOffset + perspectiveCorrection;
	float4 reflection = tex2D( _renderTextureSampler, reflectionTexCoord );
	
	color.rgb = ( 1.0 - reflection.a * _reflectionIntensity ) + reflection.rgb * reflection.a * color.a * _reflectionIntensity;
	
	// if our waves get higher than our original uv.y than add a white cap
	if( reflectionTexCoord.y - input.uvgrab.y > _sparkleIntensity )
		color.rgb = _sparkleColor;

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