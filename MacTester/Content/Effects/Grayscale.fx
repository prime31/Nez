sampler s0;

// does this work also?
//sampler TextureSampler : register( s0 );


float4 PixelShaderFunction( float4 color:COLOR0, float2 texCoord:TEXCOORD0 ) : COLOR0
{
    float4 tex = tex2D( s0, texCoord );

    // Convert it to greyscale. The constants 0.3, 0.59, and 0.11 are because
    // the human eye is more sensitive to green light, and less to blue.
    float greyscale = dot( tex.rgb, float3( 0.3, 0.59, 0.11 ) );

    // The input color alpha controls saturation level.
    tex.rgb = lerp( greyscale, tex.rgb, color.a * 4 );

    return color;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}