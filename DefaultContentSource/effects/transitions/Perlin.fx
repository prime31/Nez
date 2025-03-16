// Author: Rich Harris
// License: MIT

// https://gl-transitions.com/editor/perlin
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/perlin.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float _scale;      // = 4.0
float _smoothness; // = 0.01

float _seed; // = 12.9898

#define mod(x, y) x - (y * floor(x / y))

// http://byteblacksmith.com/improvements-to-the-canonical-one-liner-glsl-rand-for-opengl-es-2-0/
float random(float2 co)
{
		float a = _seed;
		float b = 78.233;
		float c = 43758.5453;
		float dt = dot(co.xy, float2(a, b));
		float sn = mod(dt, 3.14);
		return frac(sin(sn) * c);
}

// 2D Noise based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float noise(float2 st)
{
		float2 i = floor(st);
		float2 f = frac(st);

		// Four corners in 2D of a tile
		float a = random(i);
		float b = random(i + float2(1.0, 0.0));
		float c = random(i + float2(0.0, 1.0));
		float d = random(i + float2(1.0, 1.0));

		// Smooth Interpolation

		// Cubic Hermine Curve.  Same as SmoothStep()
		float2 u = f * f * (3.0 - 2.0 * f);
		// u = smoothstep(0.,1.,f);

		// Mix 4 corners percentages
		return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
		float n = noise(uv * _scale);

		float p = lerp(-_smoothness, 1.0 + _smoothness, _progress);
		float lower = p - _smoothness;
		float higher = p + _smoothness;

		float q = smoothstep(lower, higher, n);

		return lerp(tex2D(s0, uv), 0.0, 1.0 - q);
}

technique Perlin
{
		pass P0
		{
				PixelShader = compile ps_2_0 mainPS();
		}
}
