// Author: bobylito
// License: MIT

// https://gl-transitions.com/editor/PolkaDotsCurtain
// https://github.com/gl-transitions/gl-transitions/blob/master/transitions/PolkaDotsCurtain.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

static float _sqrt2 = 1.414213562373;

float _dots;    // 20.0;
float2 _center; // float2(0, 0);

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	if (distance(frac(uv * _dots), 0.5) >= (_progress / distance(uv, _center)))
	{
		return tex2D(s0, uv);
	}
	
	return 0;
}

technique PolkaDotsCurtain
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
