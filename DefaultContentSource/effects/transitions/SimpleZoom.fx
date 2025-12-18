// Author: 0gust1
// License: MIT

// https://gl-transitions.com/editor/SimpleZoom
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/SimpleZoom.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

float _zoomQuickness; // 0.8 (0.2 - 1.0)

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{

		float2 zoom = 0.5 + (uv - 0.5) * (1.0 - smoothstep(0.0, _zoomQuickness, _progress));

		return lerp(tex2D(s0, zoom), 0, smoothstep(_zoomQuickness - 0.2, 1.0, _progress));
}

technique SimpleZoom
{
		pass P0
		{
				PixelShader = compile ps_2_0 mainPS();
		}
}
