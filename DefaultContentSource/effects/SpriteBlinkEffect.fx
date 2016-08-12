sampler s0;
float4 _blinkColor; // 1,1,1,1


struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float4 mainPixel( VertexShaderOutput input ) : COLOR
{
	float4 color = tex2D( s0, input.TextureCoordinates ) * input.Color;
	color.rgb = lerp( color.rgb, _blinkColor.rgb, _blinkColor.a );
	
	return color;
}


technique SpriteBlink
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPixel();
	}
};