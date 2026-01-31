// Author: Paweł Płóciennik
// License: MIT

// https://gl-transitions.com/editor/WaterDrop
// https://github.com/gl-transitions/gl-transitions/blob/master/transitions/WaterDrop.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float _amplitude; // 30.0
float _speed;     // 30.0;

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	float2 dir = uv - 0.5;
	float dist = length(dir);

	if (dist > _progress)
	{
		return lerp(tex2D(s0, uv), 0, _progress);
	}
	else
	{
		float2 offset = dir * sin(dist * _amplitude - _progress * _speed);
		return lerp(tex2D(s0, uv + offset), 0, _progress);
	}
}

technique WaterDrop
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
