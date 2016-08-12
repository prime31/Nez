sampler s0;

texture _secondTexture;
sampler2D _secondTextureSampler = sampler_state
{
	Texture = <_secondTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
    MagFilter = Point;
    MinFilter = Point;
};


#define BlendOverlayf( color, blend ) ( color < 0.5 ? ( 2.0 * color * blend ) : ( 1.0 - 2.0 * ( 1.0 - color ) * ( 1.0 - blend ) ) )

float4 PixelShaderFunction( float2 coords:TEXCOORD0 ) : COLOR0
{
    float4 color = tex2D( s0, coords );
	float4 mask = tex2D( _secondTextureSampler, coords );
	float4 final = float4( 0.0, 0.0, 0.0, 1.0 );

	final.r = BlendOverlayf( color.r, mask.r );
	final.g = BlendOverlayf( color.g, mask.g );
	final.b = BlendOverlayf( color.b, mask.b );

    return final;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}