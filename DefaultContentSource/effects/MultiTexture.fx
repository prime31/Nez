sampler s0;

texture _secondTexture;
sampler2D _secondTextureSampler = sampler_state
{
	Texture = <_secondTexture>;
    AddressU = Clamp;
    AddressV = Clamp;
    MagFilter = Point;
    MinFilter = Point;
};


float4 PixelShaderFunction( float2 coords:TEXCOORD0 ) : COLOR0
{
    float4 color = tex2D( s0, coords );
	float4 color2 = tex2D( _secondTextureSampler, coords );
	
    return color * color2;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}