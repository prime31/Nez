#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 viewProjectionMatrix;


struct VertexShaderInput
{
    float4 Position : POSITION;
    float4 Color    : COLOR;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};



// Vertex shader.
VertexShaderOutput MainVert( VertexShaderInput vin )
{
    VertexShaderOutput vout;
    
	// we only get the view-projection matrix. Because of batching, we transform our verts on the CPU with their model matrix to world coords
	vout.Position = mul( vin.Position, viewProjectionMatrix );
	vout.Color = vin.Color;
    
    return vout;
}


// Fragment shader
float4 MainFrag( VertexShaderOutput vin ) : COLOR
{
	return vin.Color;
}


technique Geometry
{
	pass P0
	{
		VertexShader = compile vs_2_0 MainVert();
		PixelShader = compile PS_SHADERMODEL MainFrag();
	}
};