#include <Lighting.c>

float f_AmbientIntensity = 1;
float f_DiffuseIntensity;
float f_DecalScaleMult;

float3 f3_DiffuseLightDirection;
float4 f4_DiffuseColor = float4(1, 1, 1, 1);

texture tex_Material;
texture tex_Normalmap;
texture tex_PortDecalMask;
texture tex_StarboardDecalMask;
texture tex_DecalMaterial;

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

sampler2D samp_DecalMaterial = sampler_state {
    Texture = <tex_DecalMaterial>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_PortDecalMask = sampler_state {
    Texture = <tex_PortDecalMask>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_StarboardDecalMask = sampler_state {
    Texture = <tex_StarboardDecalMask>;
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
	float4 Position : POSITION;
	float4 WorldPosition : DEPTH;
	float2 TexCoord : TEXCOORD1;
	float3 Normal : TEXCOORD2;
	float3 UntransformedNormal : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.WorldPosition = mul(input.Position, mtx_World);
	float4 viewPosition = mul(output.WorldPosition, mtx_View);

	output.Position = mul(viewPosition, mtx_Projection);
	output.Normal = normalize(mul(input.Normal, mtx_World));
	output.TexCoord = input.TextureCoordinate;
	output.UntransformedNormal = normalize(input.Normal);

    return output;
}

////////////////////////////////////////////
/////////////////PIXEL SHADER///////////////
////////////////////////////////////////////
float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	float4 color = tex2D(samp_Material, input.TexCoord);
	float4 decalColor = tex2D(samp_DecalMaterial, input.TexCoord);

	float2 decalCoords;
	decalCoords.x = input.TexCoord.x/f_DecalScaleMult;
	decalCoords.y = input.TexCoord.y/f_DecalScaleMult;

	float4 decalMask;
	if(input.UntransformedNormal.z < 0){
		decalMask = tex2D(samp_PortDecalMask,  decalCoords.xy );	
	}
	else{
		decalMask = tex2D(samp_StarboardDecalMask,  decalCoords.xy );
	}

	//eliminate source texture color in decal'd area
	color = color * (1-decalMask.a);
	//add decal color to source texture color
	color = color +  decalColor * decalMask.a;
	color = saturate(color);


	//calculate lighting vectors - renormalize vectors
	input.Normal = normalize( input.Normal );		
	float3 V = normalize( f3_EyePosition - (float3) input.WorldPosition );
	//DONOT USE -light.dir since the reflection returns a ray from the surface
	float3 R = reflect( f3_DiffuseLightDirection, input.Normal);
	
	//calculate lighting
	LightingAttribs attribs = {0.1,0.5,0.5,30};	
	float4 I = calcPhong( attribs, f4_DiffuseColor, input.Normal, -f3_DiffuseLightDirection, V, R );
    
	//with texturing
	//return I * colorMap.Sample(linearSampler, input.t);
	
	//no texturing pure lighting
	float4 finalColor = saturate(color) * I;
	finalColor.a = 1;
	return finalColor; 
	/*
	float3 normal = tex2D(samp_Normalmap, input.TexCoord) + input.Normal;
	normalize(normal);

	float diffuseQuantity = dot(normalize(f3_DiffuseLightDirection), normal) * f_DiffuseIntensity;

    float3 light = normalize(f3_DiffuseLightDirection);

	float4 diffuseContribution = (color) *(diffuseQuantity);
	float4 ambientContribution = (color) *( f4_AmbientColor * f_AmbientIntensity);
	float4 shadedColor = diffuseContribution + ambientContribution;

	shadedColor.a = 1;
	*/

	//return saturate(color);
}

technique Standard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}