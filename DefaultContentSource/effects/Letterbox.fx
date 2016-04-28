sampler s0;

float4 _color; // 0,0,0,1
float _letterboxSize; // 0


float4 mainPS( float2 texCoord:TEXCOORD0, in float2 screenPos:VPOS ) : COLOR0
{
	float4 color = tex2D( s0, texCoord );

	// get the position from the bottom of the screen in pixels. we can use the screenPos along with the texCoord to calculate this since we are full screen
	// in a post processor.
	float positionFromBottom = screenPos.y / texCoord.y - screenPos.y;

	// we want to show the letterbox whenever we are at the top or bottom of the screen within _letterboxSize
	if( min( screenPos.y, positionFromBottom ) < _letterboxSize )
		color = _color;

	return color;
}



technique Vignette
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPS();
	}
}