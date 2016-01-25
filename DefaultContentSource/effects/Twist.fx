sampler s0;

float radius; // 0.5
float angle; // 5.0
float2 offset; // 0.5, 0.5


float4 PixelShaderFunction( float2 coords:TEXCOORD0, in float2 screenPos:VPOS ) : COLOR0
{
    float2 coord = coords - offset;
    float dist = length( coord );

    if( dist < radius )
    {
        float ratio = (radius - dist) / radius;
        float angleMod = ratio * ratio * angle;
        float s = sin( angleMod );
        float c = cos( angleMod );
        coord = float2( coord.x * c - coord.y * s, coord.x * s + coord.y * c );
    }

    return tex2D( s0, coord + offset );
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}