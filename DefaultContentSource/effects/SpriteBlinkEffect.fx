sampler s0;
float4 blinkColor; // 1,1,1,1


struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float4 MainPS( VertexShaderOutput input ) : COLOR
{
	float4 color = tex2D( s0, input.TextureCoordinates ) * input.Color;
	color.rgb = lerp( color.rgb, blinkColor.rgb, blinkColor.a );
	
	return color;
}


technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_3_0 MainPS();
	}
};