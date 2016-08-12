// ##### ##### ##### ##### ##### ##### #####
// ##### ##### Debugging/config  ##### #####
// ##### ##### ##### ##### ##### ##### #####

// if z is ignored for attenuation it will be calculated with z = 0 so the falloff is 2D linear
// if z is not ignored for attenuation spot light attenuation needs special care and very high intensity
#define IGNORE_Z_FOR_ATTENUATION
//#define DEBUG_ATTENUATION



// ##### ##### ##### ##### ##### ##### #####
// ##### ##### Common Uniforms   ##### #####
// ##### ##### ##### ##### ##### ##### #####

float4x4 _objectToWorld;
float4x4 _worldToView;
float4x4 _projection; // viewToCamera?
float4x4 _screenToWorld; // this is used to compute the world-position

// color of the light 
float3 _color;

// this is the position of the light
float3 _lightPosition;

// how far does this light reach
float _lightRadius;

// control the brightness of the light
float _lightIntensity;

// normal map
texture _normalMap;
sampler _normalMapSampler = sampler_state { Texture = <_normalMap>; };


// ##### ##### ##### ##### ##### ##### #####
// ##### Clear Gbuffer uniforms        #####
// ##### ##### ##### ##### ##### ##### #####
float3 _clearColor;


// ##### ##### ##### ##### ##### ##### #####
// ##### Spot light uniforms     ##### #####
// ##### ##### ##### ##### ##### ##### #####
float _coneAngle;
float2 _lightDirection;


// ##### ##### ##### ##### ##### ##### #####
// ##### Directional light uniforms    #####
// ##### ##### ##### ##### ##### ##### #####
// direction of the light. shared with area light.
float3 _dirAreaLightDirection;

// specular intentity
float _specularIntensity;

// specular power
float _specularPower;


// ##### ##### ##### ##### ##### ##### #####
// ##### Final combine uniforms        #####
// ##### ##### ##### ##### ##### ##### #####
texture _colorMap;
sampler _colorMapSampler = sampler_state { Texture = <_colorMap>; };

texture _lightMap;
sampler _lightMapSampler = sampler_state { Texture = <_lightMap>; };

float3 _ambientColor;


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Shared functions  ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

inline float calcSpecular( float3 lightVector, float3 normal )
{
    // reflection vector
    float3 reflectionVector = normalize( reflect( -lightVector, normal ) );

    // camera-to-surface vector
    float3 halfVec = float3( 0, 0, 1 );

    // compute specular light. R.V^n
    return _specularIntensity * pow( saturate( dot( reflectionVector, halfVec ) ), _specularPower );
}


// calculates nDotL and the final diffuse contribution. returns via out attenuation and the normalized
// lightVector (lightPosition - screenPixel)
inline float3 calcDiffuseContribution( float4 world, float4 screenPosition, out float attenuation, out float3 lightVector )
{
    float2 screenPos = world.xy / world.w;
    float2 screenSpaceTexCoord = 0.5f * float2( screenPos.x, -screenPos.y ) + 0.5f;

    // obtain screen position
    screenPosition.xy /= screenPosition.w;

    // get normal data from the normalMap
    float4 normalData = tex2D( _normalMapSampler, screenSpaceTexCoord );

    // tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0;
    normal.y *= -1.0;

    // screen-space position
    float4 position = float4( screenPos, 0.0, 1.0 ); // optional future change: add depth texture and stick the value here

    // transform to world space
    position = mul( position, _screenToWorld );

    // surface-to-light vector
    lightVector = _lightPosition - position.xyz;

#ifdef IGNORE_Z_FOR_ATTENUATION
    float3 lightVectorNoZ = float3( _lightPosition.xy, 0 ) - float3( position.xy, 0 );

    // compute attenuation based on distance - linear attenuation
    attenuation = saturate( 1.0f - length( lightVectorNoZ ) / _lightRadius );
#else
	attenuation = saturate( 1.0f - length( lightVector ) / _lightRadius );
#endif

    // normalize light vector
    lightVector = normalize( lightVector );

    // compute diffuse light
    float NdL = max( 0, dot( normal, lightVector ) );

#ifdef IGNORE_Z_FOR_ATTENUATION
    lightVector = normalize( lightVectorNoZ );
#endif

    return NdL * _color.rgb;
}


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Clear GBuffer     ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

struct PixelClearGBufferOut
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
};


float4 clearGBufferVert( float4 position:POSITION0 ) : POSITION0
{
    return position;
}


PixelClearGBufferOut clearGBufferPixel( float4 position:POSITION0 )
{
    PixelClearGBufferOut output;

    // black color
    output.color = float4( _clearColor, 1.0 );

    // when transforming 0.5f into [-1,1], we will get 0.0f
    output.normal.rgb = float3( 0.5, 0.5, 1 ); // plain old diffuse
    // no self illumination by default
    output.normal.a = 0;

    return output;
}


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Point Light 		 ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

struct VertexPointSpotOut
{
    float4 position 		: POSITION0;
    float4 screenPosition 	: TEXCOORD1;
	float4 world			: TEXCOORD3;
};


VertexPointSpotOut nonDirectionalLightVert( float4 position:POSITION0 )
{
    VertexPointSpotOut output;

    // process geometry
    float4 worldPosition = mul( position, _objectToWorld );
    float4 viewPosition = mul( worldPosition, _worldToView );
    output.position = mul( viewPosition, _projection );

    output.screenPosition = position;
    output.world = output.position;

    return output;
}


float4 pointLightPixel( VertexPointSpotOut input ) : COLOR0
{
	float attenuation;
	float3 lightVector;
    float3 diffuseLight = calcDiffuseContribution( input.world, input.screenPosition, attenuation, lightVector );

    // take into account attenuation and lightIntensity.
    float4 result = attenuation * _lightIntensity * float4( diffuseLight.rgb, 1 );

	#ifdef DEBUG_ATTENUATION
    // debug attenuation falloff
    result = result * 0.001 + float4( attenuation, attenuation, attenuation, 1 );
    #endif

    return result;
}



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Spot Light        ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

float4 spotLightPixel( VertexPointSpotOut input ) : COLOR0
{
	// standard diffuse
	float attenuation;
	float3 lightVector;
    float3 diffuseLight = calcDiffuseContribution( input.world, input.screenPosition, attenuation, lightVector );


    // spotlight cone calculations
    float phi = cos( radians( _coneAngle * 0.5 ) );

    // the angle away from the light's direction
    float rho = -dot( lightVector.xy, normalize( _lightDirection.xy ) );
    float spotAttenuation = max( 0, ( ( rho - phi ) / ( 1.0 - phi ) ) );
    attenuation *= spotAttenuation;

    // take into account attenuation and lightIntensity.
    float4 result = attenuation * _lightIntensity * float4( diffuseLight.rgb, 1 );

    #ifdef DEBUG_ATTENUATION
    // debug attenuation falloff
    return result * 0.001 + float4( attenuation, attenuation, attenuation, 1 );
    #endif

    return result;
}



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Area light        ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

float4 areaLightPixel( VertexPointSpotOut input ) : COLOR0
{
    float2 screenPos = input.world.xy / input.world.w;
    float2 screenSpaceTexCoord = 0.5f * float2( screenPos.x, -screenPos.y ) + 0.5f;

    // obtain screen position
    input.screenPosition.xy /= input.screenPosition.w;

    // get normal data from the normalMap
    float4 normalData = tex2D( _normalMapSampler, screenSpaceTexCoord );

    // tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0;
    normal.y *= -1.0;

    // we dont need to worry about converting to world space since area lights dont care
    // surface-to-light vector
    float3 lightVector = -normalize( _dirAreaLightDirection );

    // compute diffuse light
    float NdL = max( 0, dot( normal, lightVector ) );
    float3 diffuseLight = NdL * _color.rgb;

    // take into account attenuation and lightIntensity.
    float4 result = _lightIntensity * float4( diffuseLight.rgb, 1 );
    return result;

    // debug attenuation falloff
    //return result * 0.001 + float4( 1,0,1, 1.0 );
}



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Directional Light       ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

struct VertexPosTexCoordsOutput
{
    float4 position : POSITION0;
    float2 texCoord : TEXCOORD0;
};


VertexPosTexCoordsOutput directionalLightVert( float4 position:POSITION0, float2 texCoord:TEXCOORD0 )
{
    VertexPosTexCoordsOutput output;

    output.position = position;
    output.texCoord = texCoord;

    return output;
}


float4 directionalLightPixel( VertexPosTexCoordsOutput input ) : COLOR0
{
    // get normal data from the normalMap
    float4 normalData = tex2D( _normalMapSampler, input.texCoord );

    // tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    normal.y *= -1.0;

    // we dont need to worry about converting to world space since directional lights dont care
    // surface-to-light vector
    float3 lightVector = -normalize( _dirAreaLightDirection );

    // compute diffuse light
    float NdL = max( 0, dot( normal, lightVector ) );
    float3 diffuseLight = NdL * _color.rgb;

    // reflection vector
    float3 reflectionVector = normalize( reflect( -lightVector, normal ) );

    // camera-to-surface vector
    float3 halfVec = float3( 0, 0, 1 );

    // compute specular light. R.V^n
    float specularLight = _specularIntensity * pow( saturate( dot( reflectionVector, halfVec ) ), _specularPower );

    // output the two lights
    return float4( diffuseLight.rgb + specularLight, 1 );
}



// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Final combine     ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

VertexPosTexCoordsOutput finalCombineVert( float4 position:POSITION0, float2 texCoord:TEXCOORD0 )
{
    VertexPosTexCoordsOutput output;

    output.position = position;
    output.texCoord = texCoord;

    return output;
}

float4 finalCombinePixel( VertexPosTexCoordsOutput input ) : COLOR0
{
    float3 diffuseColor = tex2D( _colorMapSampler, input.texCoord ).rgb;
    float selfIllumination = tex2D( _normalMapSampler, input.texCoord ).a;
    float4 light = tex2D( _lightMapSampler, input.texCoord );

    // when we are self illuminated we disregard the lighting accumulation
    float3 diffuseLight = lerp( light.rgb, 1, selfIllumination );

    // compute ambient light, tailing it off to 0 based on the selfIllumination value
    float3 ambient = diffuseColor * _ambientColor;
    float3 ambientLight = lerp( ambient, 0, selfIllumination );

    float4 result = float4( diffuseColor * diffuseLight + ambient, 1 );
    return result;
}


// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####
// ##### ##### Techniques        ##### ##### ##### ##### ##### ##### #####
// ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### ##### #####

#define TECHNIQUE( name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_2_0 vsname(); PixelShader = compile ps_2_0 psname(); } }


TECHNIQUE( ClearGBuffer, clearGBufferVert, clearGBufferPixel )
TECHNIQUE( DeferredPointLight, nonDirectionalLightVert, pointLightPixel )
TECHNIQUE( DeferredSpotLight, nonDirectionalLightVert, spotLightPixel )
TECHNIQUE( DeferredAreaLight, nonDirectionalLightVert, areaLightPixel )
TECHNIQUE( DeferredDirectionalLight, directionalLightVert, directionalLightPixel )
TECHNIQUE( FinalCombine, finalCombineVert, finalCombinePixel )