// Author: gre
// License: MIT

// https://gl-transitions.com/editor/windowslice
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/windowslice.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float _count;      // 10 (1 - 100)
float _smoothness; // 0.5 (0.0 - 1.0)

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	float pr = smoothstep(-_smoothness, 0.0, uv.x - _progress * (1.0 + _smoothness));
	float s = step(pr, frac(_count * uv.x));
	return lerp(tex2D(s0, uv), 0, s);
}

technique WindowSlice
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
