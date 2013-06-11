float4x4 World;
float4x4 View;
float4x4 Projection;
float4 AmbientColor;
float AmbientIntensity = 1;
float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection;
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity;

texture Texture;
sampler2D textureSampler = sampler_state {
    Texture = <Texture>;
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
    float4 worldPosition = mul(input.Position, World);
	
	//valid alternative
	//float4x4 test = mul (View, Projection);
	//output.Position = mul(worldPosition, test);
	
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float4 normal = input.Normal;
    //float4 normal = mul(input.Normal, View);
	//normal = mul(normal, Projection);
	//float4 normal = input.Normal;
	normal = normalize(normal);

	float4 diffuseDir = mul(DiffuseLightDirection, World);
	//float3 dir = normalize(DiffuseLightDirection);
    //float lightIntensity = dot(normal, dir) * AmbientIntensity;
	float lightIntensity = dot(normal, diffuseDir) * DiffuseIntensity;
    output.Color = saturate(AmbientColor * AmbientIntensity + lightIntensity);

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

	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
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