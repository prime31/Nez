
Texture2D SpriteTexture;
float4 blinkColor; // 1,1,1,1


sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float4 MainPS( VertexShaderOutput input ) : COLOR
{
	float4 color = tex2D( SpriteTextureSampler, input.TextureCoordinates ) * input.Color;
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