//todo optimize this, maybe even for vendor-specific if have the time

//since max constant parameters to kernel is ~6, gotta cram them into an array
typedef enum{
	Lacunarity = 0,
	Gain = 1,
	Offset = 2,
	Octaves = 3,
	HScale = 4,
	VScale = 5,
	MetersPerBlock = 6
} PARAMETERS;

inline float GenRidge( float height, float offset );
inline float GenNoise(int x, int z);
inline float CosInterp(float a, float b, float x);
inline float InterpNoise(float x, float z);
inline float GetHeight(int x, int z, float lacunarity, float gain, float offset, int octaves, float hScale, float vScale, int chunkOfstX, int chunkOfstZ);

__kernel void GenTerrain(
	__constant float *parameters,
	__constant int *chunkOffsetX,
	__constant int *chunkOffsetZ,
	__global float4 *geometry,
	__global float4 *normals){  
	
	////////////////////
	//GENERATE TERRAIN//
	////////////////////
	int blockX = get_global_id(0);
	int blockZ = get_global_id(1);
	
	int chunkWidth = get_global_size(0);
	float realPosX = (blockX  + *chunkOffsetX) * parameters[MetersPerBlock];
	float realPosZ = (blockZ  + *chunkOffsetZ) * parameters[MetersPerBlock];
	
	float height = GetHeight(
		blockX, 
		blockZ, 
		parameters[Lacunarity], 
		parameters[Gain], 
		parameters[Offset], 
		parameters[Octaves], 
		parameters[HScale], 
		parameters[VScale], 
		*chunkOffsetX,
		*chunkOffsetZ
	);
	
	geometry[blockX*chunkWidth+blockZ] = (float4)(realPosX,height,realPosZ,-1);
	
	////////////////////
	//GENERATE NORMALS//
	////////////////////
	barrier(CLK_GLOBAL_MEM_FENCE);
}

inline float GenRidge( float height, float offset ){
	height = fabs(height);
	height = offset - height;
	height = pown(height, 2);
	return height;
}

inline float GenNoise(int x, int z){
	int n = x + z*57;//xxxx fix this FFS
	n = (n << 13) ^ n;
	return (1.0f - ((n*(n*n*15731 + 789221) + 1376312589)& 0x7fffffff)/1073741824.0f);
}

inline float CosInterp(float a, float b, float x){
	float ft = x*3.1415927f;
	float f = ((1 - cos(ft))*0.5f);
	return a*(1 - f) + b*f;
}

inline float InterpNoise(float x, float z){
    int intX = x;
    int intZ = z;
	
	float fracX = x - intX;
    float fracZ = z - intZ;

    float v1 = GenNoise(intX, intZ);
    float v2 = GenNoise(intX + 1, intZ);
    float v3 = GenNoise(intX, intZ + 1);
    float v4 = GenNoise(intX + 1, intZ + 1);

    float i1 = CosInterp(v1, v2, fracX);
    float i2 = CosInterp(v3, v4, fracX);

    return CosInterp(i1, i2, fracZ);
}

inline float GetHeight(int x, int z, float lacunarity, float gain, float offset, int octaves, float hScale, float vScale, int chunkOfstX, int chunkOfstZ){
	float posX = (x + chunkOfstX) * hScale;
	float posZ = (z + chunkOfstZ) * hScale;
		
	float sum = 0;
	float amplitude = 0.5f;
	float frequency = 1.0f;
	float prev = 1.0f;
		
	for(int curOctave=0; curOctave < octaves; curOctave++){
		float n = GenRidge(
					InterpNoise(
						posX * frequency,
						posZ * frequency
					),
					offset
					);
		sum += n * amplitude * prev;
		prev = n;
		frequency *= lacunarity;
		amplitude *= gain;
	}
	return sum * vScale;
}