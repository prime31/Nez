// Author: Fabien Benetou
// License: MIT

// https://gl-transitions.com/editor/windowblinds
// https://github.com/gl-transitions/gl-transitions/tree/master/transitions/windowblinds.glsl

// Converted to HLSL/XNA for Nez

sampler s0;

float _progress; // 0

#define mod(x, y) x - (y * floor(x / y))

float4 mainPS(float2 uv : TEXCOORD0) : COLOR0
{
    float t = _progress;

    if (mod(floor(uv.y * 50.0 * _progress), 2.0) == 0.0)
        t *= 2.0 - 0.5;

    return lerp(tex2D(s0, uv), 0.0, lerp(t, _progress, smoothstep(0.8, 1.0, _progress)));
}

technique WindowBlinds
{
    pass P0
    {
        PixelShader = compile ps_2_0 mainPS();
    }
}
