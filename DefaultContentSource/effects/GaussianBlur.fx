// Pixel shader applies a one dimensional gaussian blur filter. This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

sampler s0; // from SpriteBatch

#define SAMPLE_COUNT 15

float2 _sampleOffsets[SAMPLE_COUNT];
float _sampleWeights[SAMPLE_COUNT];


float4 PixelShaderFunction( float2 texCoord : TEXCOORD0 ) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for( int i = 0; i < SAMPLE_COUNT; i++ )
        c += tex2D( s0, texCoord + _sampleOffsets[i] ) * _sampleWeights[i];
    
    return c;
}


technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
