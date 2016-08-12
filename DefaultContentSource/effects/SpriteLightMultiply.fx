sampler s0;

texture _lightTexture;
sampler _lightTextureSampler = sampler_state { Texture = <_lightTexture>; };

float _multiplicativeFactor; // 1


float4 PixelShaderFunction( float2 coords:TEXCOORD0 ) : COLOR0
{
    float4 base = tex2D( s0, coords );
	float4 lights = tex2D( _lightTextureSampler, coords );

    return lights * base * _multiplicativeFactor;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}