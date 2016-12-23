sampler s0;

texture _distortionTexture;
sampler2D _distortionTextureSampler = sampler_state
{
    Texture = <_distortionTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};


float _time; // Time used to scroll the distortion map
float _distortionFactor; // default 0.005. Factor used to control severity of the effect
float _riseFactor; // default 0.15. Factor used to control how fast air rises


float4 mainPS( float2 coords:TEXCOORD0 ) : COLOR0
{
    float2 distortionUV = coords;
    distortionUV.y -= _time * -_riseFactor;
    
    // Compute the distortion by reading the distortion map
    float2 distortionMapValue = tex2D( _distortionTextureSampler, distortionUV ).xy;

	// bring it into the -1 to 1 range
    float2 distortionPositionOffset = distortionMapValue;
    distortionMapValue = ( ( distortionMapValue * 2.0 ) - 1.0 );
    
    // The _distortionFactor scales the offset and thus controls the severity
    distortionMapValue *= _distortionFactor;

    // The latter 2 channels of the texture are unused... be creative
    //float2 distortionUnused = distortionMapValue.zw;

    // Since we all know that hot air rises and cools, the effect loses its severity the higher up we get
    // We use the y texture coordinate of the original texture to tell us how "high up" we are and damp accordingly
    distortionMapValue *= ( coords.y ); // 1.0 - coords.y for actual OpenGL due to coords x at the bottom
    
	return tex2D( s0, distortionMapValue + coords );
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 mainPS();
    }
}