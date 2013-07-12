#include <Lighting.c>

float4 f4_TintColor;

////////////////////////////////////////////
/////////////////VERTEX SHADER//////////////
////////////////////////////////////////////
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4x4 mtx_WorldInverseTranspose = transpose(mtx_World);
    float4 worldPosition = mul(input.Position, mtx_World);
    float4 viewPosition = mul(worldPosition, mtx_View);
    output.Position = mul(viewPosition, mtx_Projection);

    float4 normal = mul(input.Normal, mtx_WorldInverseTranspose);
	normal = normalize(normal);

    output.Normal = normal;

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = f4_TintColor;
	textureColor.a = 1;
	return textureColor;
}

technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}