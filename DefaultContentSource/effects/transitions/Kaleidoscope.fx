// Author: nwoeanhinnogaehr
// License: MIT

// https://gl-transitions.com/editor/kaleidoscope
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/kaleidoscope.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float _speed; // 1.0;
float _angle; // 1.0;
float _power; // 1.5;

static float2 _center = float2(0.5, 0.5);

#define PI 3.14159265359
#define TWOPI PI * 2.0
#define mod(x, y) x - (y * floor(x / y))

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{

	float2 p = uv.xy / 1.0;
	float2 q = p;
	float t = pow(_progress, _power) * _speed;
	p = p - 0.5;
	for (int i = 0; i < 7; i++)
	{
		p = float2(sin(t) * p.x + cos(t) * p.y, sin(t) * p.y - cos(t) * p.x);
		t += _angle;
		p = abs(mod(p, 2.0) - 1.0);
	}
	p = abs(mod(p, 1.0));

	return lerp(
		lerp(tex2D(s0, q), 0, _progress),
		lerp(tex2D(s0, p), 0, _progress),
		1.0 - 2.0 * abs(_progress - 0.5)
	);
}

technique Kaleidoscope
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
