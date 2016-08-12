sampler s0; // from SpriteBatch

float _lineSize; // width of the line in pixels
float4 _lineColor;


float4 horizontalLinesPS( float2 texCoord:TEXCOORD0, in float2 screenPos:VPOS ) : COLOR0
{
	// we only need the alpha value of the original sprite
	float4 alpha = tex2D( s0, texCoord ).a;
	
	// floor the screenPosition / lineSize. This gives us blocks with height lineSize. We mod that by 2 to take only the even blocks
	float flooredAlternate = floor( screenPos.y / _lineSize ) % 2.0;
	
	// lerp transparent to lineColor. This will always be either transparent or lineColor since flooredAlternate will be 0 or 1.
	float4 finalColor = lerp( float4( 0, 0, 0, 0 ), _lineColor, flooredAlternate );

	return finalColor *= alpha;
}


float4 verticalLinesPS( float2 texCoord:TEXCOORD0, in float2 screenPos:VPOS ) : COLOR0
{
	float4 alpha = tex2D( s0, texCoord ).a;
	float flooredAlternate = floor( screenPos.x / _lineSize ) % 2.0;
	float4 finalColor = lerp( float4( 0, 0, 0, 0 ), _lineColor, flooredAlternate );

	return finalColor *= alpha;
}



technique VerticalLines
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 verticalLinesPS();
	}
}

technique HorizontalLines
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 horizontalLinesPS();
	}
}