float4x4 mtx_World;
float4x4 mtx_View;
float4x4 mtx_Projection;
float4x4 mtx_WorldInverseTranspose;

float f_AmbientIntensity = 1;
float f_DiffuseIntensity;
float f_DecalScaleMult;

float3 f3_DiffuseLightDirection;
float4 f4_DiffuseColor = float4(1, 1, 1, 1);
float4 f4_AmbientColor;

texture tex_Material;
texture tex_Normalmap;
texture tex_DecalWrap;

sampler2D samp_Material = sampler_state {
    Texture = <tex_Material>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};
sampler2D samp_Normalmap = sampler_state {
    Texture = <tex_Normalmap>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_Decal = sampler_state {
    Texture = <tex_DecalWrap>;
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
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4x4 WorldInverseTranspose = transpose(mtx_World);
    float4 worldPosition = mul(input.Position, mtx_World);
	float4 viewPosition = mul(worldPosition, mtx_View);

	output.Position = mul(viewPosition, mtx_Projection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(samp_Material, input.TextureCoordinate);

	float2 decalCoords;
	decalCoords.x = input.TextureCoordinate.x/f_DecalScaleMult;
	decalCoords.y = input.TextureCoordinate.y/f_DecalScaleMult;

	float4 decal = tex2D(samp_Decal,  decalCoords.xy );

	//eliminate source texture color in decal'd area
	color = color * (1-decal.a);
	//add decal color to source texture color
	color = color + decal;
	color = saturate(color);

	float3 normal = tex2D(samp_Normalmap, input.TextureCoordinate) + input.Normal;
	normalize(normal);

	float diffuseQuantity = dot(normalize(f3_DiffuseLightDirection), normal) * f_DiffuseIntensity;

    float3 light = normalize(f3_DiffuseLightDirection);

	float4 diffuseContribution = (color) *(diffuseQuantity);
	float4 ambientContribution = (color) *( f4_AmbientColor * f_AmbientIntensity);
	float4 shadedColor = diffuseContribution + ambientContribution;

	shadedColor.a = 1;
	return saturate(shadedColor);
}

technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}