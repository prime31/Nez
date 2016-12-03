sampler s0;

float4 mainPS(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);
	
	if (!any(color)) return color;
	
	color.rgb = 1 - color.rgb;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 mainPS();
    }
}