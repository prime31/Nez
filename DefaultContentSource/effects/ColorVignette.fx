sampler s0;

float _power; // 1.0
float _radius; // 1.25
float4 _color;

float4 mainPS( float2 texCoord:TEXCOORD0 ) : COLOR0
{
	float4 color = tex2D( s0, texCoord );
	float2 dist = ( texCoord - 0.5f ) * _radius;
	dist.x = 1 - dot( dist, dist ) * _power;
	color.rgb *= saturate( dist.x + _color );

	return color;
}



technique ColorVignette
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPS();
	}
};
