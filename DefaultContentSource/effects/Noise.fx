sampler s0;

float noise; // 1.0

float rand( float2 co )
{
    return frac( sin( dot( co.xy, float2( 12.9898, 78.233 ) ) ) * 43758.5453 );
}


float4 PixelShaderFunction( float2 coords:TEXCOORD0, in float2 screenPos:VPOS ) : COLOR0
{
    float4 color = tex2D( s0, coords );

    float diff = ( rand( coords ) - 0.5 ) * noise;

    color.r += diff;
    color.g += diff;
    color.b += diff;

    return color;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}