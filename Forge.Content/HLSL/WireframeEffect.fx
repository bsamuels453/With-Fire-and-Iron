float4x4 World;
float4x4 View;
float4x4 Projection;
float3 Color = (1,1,1);
float Alpha = 1;

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
	float4x4 WorldInverseTranspose = transpose(World);
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
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
	pixelColor.r=Color.r;
	pixelColor.g=Color.g;
	pixelColor.b=Color.b;
	pixelColor.a = Alpha;
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