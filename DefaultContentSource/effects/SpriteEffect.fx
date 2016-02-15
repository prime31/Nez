
Texture2D SpriteTexture;


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
	return tex2D( SpriteTextureSampler, input.TextureCoordinates );
}


technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_3_0 MainPS();
	}
};