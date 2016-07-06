sampler s0;

texture _transitionTex;
sampler _transitionTexSampler = sampler_state { Texture = <_transitionTex>; };


float _progress;
float4 _color; // Color.Black
float _opacity; // 0 - 1



float4 mainPixel( float2 texCoord:TEXCOORD0 ) : COLOR0
{
	float4 transit = tex2D( _transitionTexSampler, texCoord );
	float4 color = tex2D( s0, texCoord );

	// when our transition texture.b is less than progress we just return the color blended based on the opacity
	if( transit.b < _progress )
		return lerp( color, _color, _opacity );

	// transition texture.b is > progress so return the texture
	return color;
}


float4 wipeWithDistortPixel( float2 texCoord:TEXCOORD0 ) : COLOR0
{
	float4 transit = tex2D( _transitionTexSampler, texCoord );
	
	// get an offset from the rg channels of the transitionTex. we need it in the -1 - 1 range
	float2 direction = normalize( ( transit.rg - 0.5 ) * 2 );
	float4 color = tex2D( s0, texCoord + _progress * direction );

	if( transit.b < _progress )
		return lerp( color, _color, _opacity );

	return color;
}



technique TextureWipe
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPixel();
	}
};

technique TextureWipeWithDistort
{
	pass P0
	{
		PixelShader = compile ps_3_0 wipeWithDistortPixel();
	}
};