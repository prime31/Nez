sampler s0;
float4x4 MatrixTransform;


struct VertexShaderOutput
{
	float4 position : POSITION;
	float4 color : COLOR0;
	float2 texCoord : TEXCOORD0;
};


VertexShaderOutput spriteVert( float4 position: POSITION0, float4 color: COLOR0, float2 texCoord: TEXCOORD0 )
{
	VertexShaderOutput output;
    output.position = mul( position, MatrixTransform );
	output.color = color;
	output.texCoord = texCoord;
	
	return output;
}


float4 spritePixel( VertexShaderOutput input ) : COLOR
{
	float4 color = tex2D( s0, input.texCoord ) * input.color;
	color.rgb *= input.color.a;
	
	return color;
}


technique SpriteDrawing
{
	pass P0
	{
		VertexShader = compile vs_2_0 spriteVert();
		PixelShader = compile ps_2_0 spritePixel();
	}
};