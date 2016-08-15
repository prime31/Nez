// Pixel shader extracts the brighter areas of an image.. This is the first step in applying a bloom postprocess.

sampler s0; // from SpriteBatch

float _bloomThreshold;


float4 PixelShaderFunction( float2 texCoord : TEXCOORD0 ) : COLOR0
{
    // Look up the original image color.
    float4 c = tex2D( s0, texCoord );

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate( ( c - _bloomThreshold ) / ( 1 - _bloomThreshold ) );
}


technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
