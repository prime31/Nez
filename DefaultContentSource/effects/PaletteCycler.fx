sampler s0; // from SpriteBatch

texture _paletteTexture;
sampler1D _paletteTextureSampler = sampler_state
{
    Texture = <_paletteTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

float _time; // time in seconds
float _cycleSpeed; // defaults to 0


float4 mainPixel( float2 coords:TEXCOORD0 ) : COLOR0
{
	// first grab the main texture pixel
	float4 baseTex = tex2D( s0, coords );
	
	// use one of the components of the grayscale color to calculate an index into the paletteTexture
	float index = baseTex.r + _time * _cycleSpeed;

	// return the mapped color from the paletteTexture
	return tex1D( _paletteTextureSampler, index );
}


technique PaletteCycler
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPixel();
	}
};