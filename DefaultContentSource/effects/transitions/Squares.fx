sampler s0;

float _progress; // 0
float4 _color; // Color.Black
float2 _size; // 30, 30 / aspectRatio
float _smoothness; // 0.5



float rand( float2 co )
{
	float x = sin( dot( co.xy, float2( 12.9898, 78.233 ) ) ) * 43758.5453;
	return x - floor( x );
}


float4 mainPS( float2 texCoord:TEXCOORD0 ) : COLOR0
{
	float r = rand( floor( _size.xy * texCoord ) );
	float m = smoothstep( 0.0, -_smoothness, r - ( _progress * ( 1.0 + _smoothness ) ) );

	return lerp( tex2D( s0, texCoord ), _color, m );
}



technique Squares
{
	pass P0
	{
		PixelShader = compile ps_3_0 mainPS();
	}
};