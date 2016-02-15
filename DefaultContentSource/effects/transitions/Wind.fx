sampler s0;

float _progress; // 0
float _size; // 0.3 (0.1 - 0.6)
float _windSegments; // 100 (1 - 1000)



float rand( float2 co )
{
	float x = sin( dot( co.xy, float2( 12.9898, 78.233 ) ) ) * 43758.5453;
	return x - floor( x );
}


float4 mainPS( float2 texCoord:TEXCOORD0 ) : COLOR0
{
	float r = rand( floor( float2( 0.0, texCoord.y * _windSegments ) ) );
	float m = smoothstep( 0.0, -_size, texCoord.x * ( 1.0 - _size ) + _size * r - ( _progress * ( 1.0 + _size ) ) );

	return lerp( tex2D( s0, texCoord ), float4( 0.0, 0.0, 0.0, 0.0 ), m );
}



technique Wind
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPS();
	}
};