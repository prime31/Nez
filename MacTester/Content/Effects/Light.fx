static const float3 black = float3( 0.0f, 0.0f, 0.0f );

float2 lightSource; // Light position in 2D world space
float3 lightColor;  // Color of the light
float  lightRadius; // Radius of the light
float4x4 viewProjectionMatrix; // camera view-proj matrix


struct VertexShaderInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
  float4 Position : POSITION0;
  float2 WorldPos : TEXCOORD0;
};


VertexShaderOutput VertexShaderFunction( VertexShaderInput input )
{
  VertexShaderOutput output;
  //output.Position = float4( input.Position, 1 );
  output.Position = mul( input.Position, viewProjectionMatrix );

  // Vertex position in 2D world space
  output.WorldPos = input.TexCoord;
  
  return output;
}


float4 PixelShaderFunction( VertexShaderOutput input ) : COLOR0
{   
  // Compute the relative position of the pixel and the distance towards the light
  float2 position = input.WorldPos - lightSource;     
  float dist = sqrt( dot( position, position ) );

  // Mix between black and the light color based on the distance and the power of the light
  float3 mix = lerp( lightColor, black, saturate( dist / lightRadius ) );
  return float4( mix, 1.0f );
}


technique Technique1
{
  pass Pass1
  {
    VertexShader = compile vs_2_0 VertexShaderFunction();
    PixelShader = compile ps_2_0 PixelShaderFunction();
  }
}
