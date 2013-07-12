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
	float4 WPosition : TEXCOORD2;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.WPosition = localToWorld(input.Position);
    float4 viewPosition = worldToView(output.WPosition);
	//float4 ww = localToWorld(input.Position);
	//float4 viewPosition = worldToView(ww);
    output.Position = viewToProjection(viewPosition);
	// * 

    //float4 normal = mul(input.Normal, View);
	//normal = mul(normal, Projection);
	//float4 normal = input.Normal;



	/*
	float4 diffuseDir = mul(f3_DiffuseLightDirection, mtx_World);
	//float3 dir = normalize(DiffuseLightDirection);
    //float lightIntensity = dot(normal, dir) * AmbientIntensity;
	float lightIntensity = dot(normal, diffuseDir) * f_DiffuseIntensity;
    output.Color = saturate(f4_AmbientColor * f_AmbientIntensity + lightIntensity);
	*/
    //output.Normal = normal;
	output.Normal = localToWorld(input.Normal);
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.Normal = normalize(input.Normal);
	float3 V = normalize( f3_EyePosition - (float3) input.WPosition	 );
	float3 R = reflect( f3_DiffuseLightDirection, input.Normal);
	//input.Color = clamp(input.Color, 0.35, 1);

	float4 textureColor = tex2D(samp_Material, input.TextureCoordinate);
    float4 tex =(1,1,1,1);//= /saturate(textureColor * input.Color);
	tex.a = 1;
	LightingAttribs attribs = {1,1,1,1};
	return calcPhong( attribs, f4_DiffuseColor, input.Normal, -f3_DiffuseLightDirection, V, R );
}





technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}