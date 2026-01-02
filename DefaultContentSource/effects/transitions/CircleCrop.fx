// Author: fkuteken
// License: MIT
// ported by gre from https://gist.github.com/fkuteken/f63e3009c1143950dee9063c3b83fb88

// https://gl-transitions.com/editor/CircleCrop
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/CircleCrop.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0
float _ratio;    // Screen.Width/Height

float4 _bgcolor; // = vec4(0.0, 0.0, 0.0, 1.0)

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	float dist = length((float2(uv.x, uv.y) - 0.5) * float2(1.0, 1.0 / _ratio));

	// branching is ok here as we statically depend on progress uniform
	// (branching won't change over pixels)
	return lerp(
		_progress < 0.5 ? tex2D(s0, uv) : 0,
		_bgcolor,
		step(pow(2.0 * abs(_progress - 0.5), 3.0), dist)
	);
}

technique CircleCrop
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
