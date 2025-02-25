// Author: gre
// License: MIT

// https://gl-transitions.com/editor/directionalwipe
// https://github.com/gl-transitions/gl-transitions/blob/master/transitions/directionalwipe.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float2 _direction; // float2(1.0, 1.0) Normalized
float _smoothness; // 0.5 (0 - 1.0)

static float2 _center = float2(0.5, 0.5);

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{

	float2 v = normalize(_direction);
	v /= abs(v.x) + abs(v.y);
	float d = v.x * _center.x + v.y * _center.y;
	float m =
		(1.0 - step(_progress, 0.0)) *
		(1.0 - smoothstep(-_smoothness, 0.0, v.x * uv.x + v.y * uv.y - (d - 0.5 + _progress * (1.0 + _smoothness))));
	return lerp(tex2D(s0, uv), 0, m);
}

technique DirectionalWipe
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
