#include <Lighting.c>

float3 f3_Color = (1,1,1);
float f_Alpha = 1;

////////////////////////////////////////////
/////////////////VERTEX SHADER//////////////
////////////////////////////////////////////
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4x4 mtx_WorldInverseTranspose = transpose(mtx_World);
    float4 worldPosition = mul(input.Position, mtx_World);
    float4 viewPosition = mul(worldPosition, mtx_View);
    output.Position = mul(viewPosition, mtx_Projection);
    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	/////////////
	//COLORING///
	/////////////
	float4 pixelColor;
	pixelColor.r = f3_Color.r;
	pixelColor.g = f3_Color.g;
	pixelColor.b = f3_Color.b;
	pixelColor.a = f_Alpha;
	return pixelColor;
}

technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}