#include <Lighting.c>

float3 f3_DiffuseLightDirection = float3(1, 1, 0);
float4 f4_DiffuseColor = float4(1, 1, 1, 1);

float f_TextureScalingFactor;
float f_DiffuseIntensity;
float f_AmbientIntensity;

////constant textures
texture tex_GrassTexture;
texture tex_DirtTexture;
texture tex_RockTexture;
texture tex_IceTexture;
texture tex_TreeTexture;
texture tex_TreeNormalTexture;
texture tex_RockNormalTexture;
texture tex_SnowNormalTexture;

////dynamic textures
texture tex_Normalmap;
texture tex_Binormalmap;
texture tex_Tangentmap;

texture tex_TerrainCanvasTexture;
texture tex_InorganicCanvasTexture;
texture tex_FoliageCanvasTexture;
texture tex_TreeCanvasTexture;


sampler2D samp_GrassMaterial = sampler_state {
    Texture = <tex_GrassTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_DirtMaterial = sampler_state {
    Texture = <tex_DirtTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_RockMaterial = sampler_state {
    Texture = <tex_RockTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_IceMaterial = sampler_state {
    Texture = <tex_IceTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_TreeMaterial = sampler_state {
    Texture = <tex_TreeTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_TreeNormalmap = sampler_state {
    Texture = <tex_TreeNormalTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_RockNormalmap = sampler_state {
    Texture = <tex_RockNormalTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_SnowNormalmap = sampler_state {
    Texture = <tex_SnowNormalTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D samp_TerrainNormalmap = sampler_state {
    Texture = <tex_Normalmap>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_TerrainBinormalmap = sampler_state {
    Texture = <tex_Binormalmap>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_TerrainTangentmap = sampler_state {
    Texture = <tex_Tangentmap>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_TerrainCanvas = sampler_state {
    Texture = <tex_TerrainCanvasTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_InorganicCanvas = sampler_state {
    Texture = <tex_InorganicCanvasTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_FoliageCanvas = sampler_state {
    Texture = <tex_FoliageCanvasTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D samp_TreeCanvas = sampler_state {
    Texture = <tex_TreeCanvasTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

////////////////////////////////////////////
/////////////////VERTEX SHADER//////////////
////////////////////////////////////////////
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4x4 mtx_WorldInverseTranspose = transpose(mtx_World);
    float4 mtx_WorldPosition = mul(input.Position, mtx_World);
    float4 mtx_ViewPosition = mul(mtx_WorldPosition, mtx_View);
    output.Position = mul(mtx_ViewPosition, mtx_Projection);
    output.TextureCoordinate = input.TextureCoordinate;

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
    float4 terrainCanvas = tex2D(samp_TerrainCanvas, input.TextureCoordinate);

	float4 inorganicCanvas = tex2D(samp_InorganicCanvas, input.TextureCoordinate);
	float4 foliageCanvas   = tex2D(samp_FoliageCanvas,   input.TextureCoordinate);
	float4 treeCanvas      = tex2D(samp_TreeCanvas,  input.TextureCoordinate);

	float4 dirtColor  = tex2D(samp_DirtMaterial,  input.TextureCoordinate*f_TextureScalingFactor);
	float4 rockColor  = tex2D(samp_RockMaterial,  input.TextureCoordinate*f_TextureScalingFactor);
	float4 iceColor   = tex2D(samp_IceMaterial,   input.TextureCoordinate*f_TextureScalingFactor);
	float4 grassColor = tex2D(samp_GrassMaterial, input.TextureCoordinate*f_TextureScalingFactor);
	float4 treeTColor = tex2D(samp_TreeMaterial,  input.TextureCoordinate*f_TextureScalingFactor);

	float4 inorganicColor = rockColor   * inorganicCanvas.r + iceColor * inorganicCanvas.g + 0 * inorganicCanvas.b;
	float4 foliageColor   = grassColor  * foliageCanvas.r   + 0 * foliageCanvas.g          + 0 * foliageCanvas.b;
	float4 treeColor      = treeTColor  * treeCanvas.r      + 0 * treeCanvas.g             + 0 *treeCanvas.b;

	// * terrainCanvas.r + foliageColor * terrainCanvas.g + treeColor * terrainCanvas.b;


	/////////////
	///SHADING///
	/////////////
	float3 normal = tex2D(samp_TerrainNormalmap, input.TextureCoordinate);
	float3 treeBump = tex2D(samp_TreeNormalmap, input.TextureCoordinate * 4) - (0.5, 0.5, 0.5);
	float3 rockBump = tex2D(samp_RockNormalmap, input.TextureCoordinate * 4) - (0.5, 0.5, 0.5);
	float3 snowBump = tex2D(samp_SnowNormalmap, input.TextureCoordinate * 4) - (0.5, 0.5, 0.5);
	
	float rockContrib, iceContrib;
	
	float slope = normal.y;
	
	const float transitionStart=0.27;
	const float range=0.007;
	float multiplier = 1/range;
	float transitionEnd = transitionStart+range;
	
	if(slope<transitionStart){
		iceContrib=0;
		rockContrib=1;
	}
	else{
		if(slope>transitionEnd){
			iceContrib=1;
			rockContrib=0;
		}
		else{
			iceContrib = (slope-transitionStart)*multiplier;
			rockContrib = 1-iceContrib;
		}
	}
	
	float4 pixelColor = rockColor*rockContrib+iceColor*iceContrib;
	
	float3 tangent = tex2D(samp_TerrainTangentmap, input.TextureCoordinate);
	float3 binormal = tex2D(samp_TerrainBinormalmap, input.TextureCoordinate);

	//float3 treeNormal = (treeBump.x * tangent + treeBump.y * binormal) * terrainCanvas.b;
	float3 rockNormal = (rockBump.x * tangent + rockBump.y * binormal) * rockContrib/8;
	float3 snowNormal = (snowBump.x * tangent + snowBump.y * binormal) * iceContrib/2;
	//normal += treeNormal;
	normal += (rockNormal);
	normalize(normal);

    float diffuseQuantity = dot(normalize(f3_DiffuseLightDirection), normal) * f_DiffuseIntensity;
	float ambientQuantity = f_AmbientIntensity * f4_AmbientColor;

	float4 diffuseContribution = (pixelColor) *(diffuseQuantity);
	float4 ambientContribution = (pixelColor) *(ambientQuantity);
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