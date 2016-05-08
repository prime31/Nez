sampler s0;
float3 _sepiaTone; // defaults to 1.2, 1.0, 0.8


float4 PixelShaderFunction( float4 color:COLOR0, float2 texCoord:TEXCOORD0 ) : COLOR0
{
    float4 tex = tex2D( s0, texCoord );

    // first we need to convert to greyscale
    float grayScale = dot( tex.rgb, float3( 0.3, 0.59, 0.11 ) );

    tex.rgb = grayScale * _sepiaTone;

    return tex;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}