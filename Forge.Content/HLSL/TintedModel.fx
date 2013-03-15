float4x4 World;
float4x4 View;
float4x4 Projection;
float4 TintColor;
float4x4 WorldInverseTranspose;

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
	float4x4 WorldInverseTranspose = transpose(World);
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    float4 normal = mul(input.Normal, WorldInverseTranspose);
	normal = normalize(normal);

    output.Normal = normal;

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 textureColor = TintColor;
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