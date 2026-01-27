// Author: Adrian Purser
// License: MIT

// https://gl-transitions.com/editor/Bounce
// https://github.com/gl-transitions/gl-transitions/blob/master/transitions/Bounce.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float4 _shadowColor; // float4(0, 0, 0, 0.6)
float _shadowHeight; // 0.075
float _bounces;      // 3.0

#define PI 3.14159265358

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	float stime = sin(_progress * PI / 2.0);
	float phase = _progress * PI * _bounces;
	float y = (abs(cos(phase))) * (1.0 - stime);
	float d = 1.0 - uv.y - y;

	return lerp(
		lerp(0.0, _shadowColor,
			 step(d, _shadowHeight) * (1.0 - lerp(
				 ((d / _shadowHeight) * _shadowColor.a) + (1.0 - _shadowColor.a),
				 1.0,
				smoothstep(0.8, 1.0, _progress) // fade-out the shadow at the end
			))
		),
		tex2D(s0, float2(uv.x, uv.y + y)),
		step(d, 0.0)
	);
}

technique Bounce
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
