float4x4 World;
float4x4 View;
float4x4 Projection;
float TextureScalingFactor;
float4 AmbientColor;
float AmbientIntensity;
float4x4 WorldInverseTranspose;
float3 DiffuseLightDirection = float3(1, 1, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity;

////constant textures
texture GrassTexture;
texture DirtTexture;
texture RockTexture;
texture IceTexture;
texture TreeTexture;
texture TreeBumpTexture;
texture RockBumpTexture;

////dynamic textures
texture NormalMapTexture;
texture BinormalMapTexture;
texture TangentMapTexture;

texture TerrainCanvasTexture;
texture InorganicCanvasTexture;
texture FoliageCanvasTexture;
texture TreeCanvasTexture;


sampler2D GrassSamp = sampler_state {
    Texture = (GrassTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D DirtSamp = sampler_state {
    Texture = (DirtTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D RockSamp = sampler_state {
    Texture = (RockTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D IceSamp = sampler_state {
    Texture = (IceTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D TreeSamp = sampler_state {
    Texture = (TreeTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D TreeBumpSamp = sampler_state {
    Texture = (TreeBumpTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D RockBumpSamp = sampler_state {
    Texture = (RockBumpTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
	MipFilter = Linear;
};

sampler2D NormalSamp = sampler_state {
    Texture = (NormalMapTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D BinormalSamp = sampler_state {
    Texture = (BinormalMapTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D TangentSamp = sampler_state {
    Texture = (TangentMapTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D TerrainSamp = sampler_state {
    Texture = (TerrainCanvasTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D InorganicSamp = sampler_state {
    Texture = (InorganicCanvasTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D FoliageSamp = sampler_state {
    Texture = (FoliageCanvasTexture);
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
	MipFilter = Linear;
};

sampler2D TreeCanvSamp = sampler_state {
    Texture = (TreeCanvasTexture);
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
	float4x4 WorldInverseTranspose = transpose(World);
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
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
    float4 terrainCanvas = tex2D(TerrainSamp, input.TextureCoordinate);

	float4 inorganicCanvas = tex2D(InorganicSamp, input.TextureCoordinate);
	float4 foliageCanvas   = tex2D(FoliageSamp,   input.TextureCoordinate);
	float4 treeCanvas      = tex2D(TreeCanvSamp,  input.TextureCoordinate);

	float4 dirtColor  = tex2D(DirtSamp,  input.TextureCoordinate*TextureScalingFactor);
	float4 rockColor  = tex2D(RockSamp,  input.TextureCoordinate*TextureScalingFactor);
	float4 iceColor   = tex2D(IceSamp,   input.TextureCoordinate*TextureScalingFactor);
	float4 grassColor = tex2D(GrassSamp, input.TextureCoordinate*TextureScalingFactor);
	float4 treeTColor = tex2D(TreeSamp,  input.TextureCoordinate*TextureScalingFactor);

	float4 inorganicColor = rockColor   * inorganicCanvas.r + iceColor * inorganicCanvas.g + 0 * inorganicCanvas.b;
	float4 foliageColor   = grassColor  * foliageCanvas.r   + 0 * foliageCanvas.g          + 0 * foliageCanvas.b;
	float4 treeColor      = treeTColor  * treeCanvas.r      + 0 * treeCanvas.g             + 0 *treeCanvas.b;

	// * terrainCanvas.r + foliageColor * terrainCanvas.g + treeColor * terrainCanvas.b;


	/////////////
	///SHADING///
	/////////////
	float3 normal = tex2D(NormalSamp, input.TextureCoordinate);
	float3 treeBump = tex2D(TreeBumpSamp, input.TextureCoordinate * 4) - (0.5, 0.5, 0.5);
	float3 rockBump = tex2D(RockBumpSamp, input.TextureCoordinate * 4) - (0.5, 0.5, 0.5);
	
	float rockContrib, iceContrib;
	
	float slope = normal.y;
	
	const float transitionStart=0.53;
	const float range=0.03;
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
	
	float3 tangent = tex2D(TangentSamp, input.TextureCoordinate);
	float3 binormal = tex2D(BinormalSamp, input.TextureCoordinate);

	float3 treeNormal = (treeBump.x * tangent + treeBump.y * binormal) * terrainCanvas.b;
	float3 rockNormal = (rockBump.x * tangent + rockBump.y * binormal) * terrainCanvas.r * inorganicCanvas.r;
	normal += treeNormal;
	normal += (rockNormal);
	normalize(normal);

    float diffuseQuantity = dot(normalize(DiffuseLightDirection), normal) * DiffuseIntensity;
    //float3 light = normalize(DiffuseLightDirection);


	float4 diffuseContribution = (pixelColor) *(diffuseQuantity);
	float4 ambientContribution = (pixelColor) *( AmbientColor * AmbientIntensity);
	float4 shadedColor = diffuseContribution; + ambientContribution;

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