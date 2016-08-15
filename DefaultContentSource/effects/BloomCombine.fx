// Pixel shader combines the bloom image with the original scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler s0; // from SpriteBatch

texture _baseMap;
sampler _baseMapSampler = sampler_state { Texture = <_baseMap>; };

float _bloomIntensity;
float _baseIntensity;

float _bloomSaturation;
float _baseSaturation;


// helper for modifying the saturation of a color.
float4 adjustSaturation( float4 color, float saturation )
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the human eye is more sensitive to green light, and less to blue.
    float grey = dot( color.rgb, float3( 0.3, 0.59, 0.11 ) );

    return lerp( grey, color, saturation );
}


float4 PixelShaderFunction( float2 texCoord : TEXCOORD0 ) : COLOR0
{
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D( s0, texCoord );
    float4 base = tex2D( _baseMapSampler, texCoord );
    
    // Adjust color saturation and intensity.
    bloom = adjustSaturation( bloom, _bloomSaturation ) * _bloomIntensity;
    base = adjustSaturation( base, _baseSaturation ) * _baseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom, to prevent things looking excessively burned-out.
    base *= ( 1 - saturate( bloom ) );
    
    // Combine the two images.
    return base + bloom;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
