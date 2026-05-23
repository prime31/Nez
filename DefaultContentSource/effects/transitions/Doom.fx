// Author: Zeh Fernando
// License: MIT

// https://gl-transitions.com/editor/DoomScreenTransition
// https://github.com/gl-transitions/gl-transitions/blob/master/transitions/DoomScreenTransition.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

#define mod(x, y) x - (y * floor(x / y))

// Transition parameters --------

// Number of total bars/columns
float _bars; // = 30

// Multiplier for speed ratio. 0 = no variation when going down, higher = some elements go much faster
float _amplitude; // = 2

// Further variations in speed. 0 = no noise, 1 = super noisy (ignore frequency)
float _noise; // = 0.1

// Speed variation horizontally. the bigger the value, the shorter the waves
float _frequency; // = 0.5

// How much the bars seem to "run" from the middle of the screen first (sticking to the sides). 0 = no drip, 1 = curved
// drip
float _dripScale; // = 0.5

// The code proper --------

float rand(int num)
{
	return frac(mod(float(num) * 67123.313, 12.0) * sin(float(num) * 10.3) * cos(float(num)));
}

float wave(int num)
{
	float fn = float(num) * _frequency * 0.1 * float(_bars);
	return cos(fn * 0.5) * cos(fn * 0.13) * sin((fn + 10.0) * 0.3) / 2.0 + 0.5;
}

float drip(int num)
{
	return sin(float(num) / float(_bars - 1) * 3.141592) * _dripScale;
}

float pos(int num)
{
	return (_noise == 0.0 ? wave(num) : lerp(wave(num), rand(num), _noise)) 
		+ (_dripScale == 0.0 ? 0.0 : drip(num));
}

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
	int bar = int(uv.x * float(_bars));
	float scale = 1.0 + pos(bar) * _amplitude;
	float phase = _progress * scale;
	float posY = (1.0 - uv.y) / 1.0;
	float2 p;
	float4 c;
	if (phase + posY < 1.0)
	{
		p = float2(uv.x, uv.y - lerp(0.0, 1.0, phase)) / 1.0;
		c = tex2D(s0, p);
	}
	else
	{
		p = uv.xy / 1.0;
		c = 0.0;
	}

	// Finally, apply the color
	return c;
}

technique Doom
{
	pass P0
	{
		PixelShader = compile ps_2_0 mainPS();
	}
}
