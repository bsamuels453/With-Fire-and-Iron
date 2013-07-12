#include <Lighting.c>

float f_AmbientIntensity = 1;
float f_DiffuseIntensity;

float3 f3_DiffuseLightDirection;
float4 f4_DiffuseColor = float4(1, 1, 1, 1);

texture tex_Material;

sampler2D samp_Material = sampler_state {
    Texture = <tex_Material>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

////////////////////////////////////////////
/////////////////VERTEX SHADER//////////////
////////////////////////////////////////////
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    //float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	//float4x4 WorldInverseTranspose = transpose(World);
    float4 worldPosition = mul(input.Position, mtx_World);
	
	//valid alternative
	//float4x4 test = mul (View, Projection);
	//output.Position = mul(worldPosition, test);
	
    float4 viewPosition = mul(worldPosition, mtx_View);
    output.Position = mul(viewPosition, mtx_Projection);

	float4 normal = input.Normal;
    //float4 normal = mul(input.Normal, View);
	//normal = mul(normal, Projection);
	//float4 normal = input.Normal;
	normal = normalize(normal);

	float4 diffuseDir = mul(f3_DiffuseLightDirection, mtx_World);
	//float3 dir = normalize(DiffuseLightDirection);
    //float lightIntensity = dot(normal, dir) * AmbientIntensity;
	float lightIntensity = dot(normal, diffuseDir) * f_DiffuseIntensity;
    output.Color = saturate(f4_AmbientColor * f_AmbientIntensity + lightIntensity);

    //output.Normal = normal;
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.Color = clamp(input.Color, 0.35, 1);

	float4 textureColor = tex2D(samp_Material, input.TextureCoordinate);
    float4 tex = saturate(textureColor * input.Color);
	tex.a = 1;
	return tex;
}

technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}