//since max const parameters to kernel is 8, gotta cram some in an array
typedef enum{
	Lacunarity = 0,
	Gain = 1,
	Offset = 2,
	Octaves = 3,
	HScale = 4,
	VScale = 5,
	MetersPerBlock = 6,
	BlocksPerChunk = 7
} PARAMETERS;

inline float GenRidge( float height, float offset );
inline float GenNoise(int x, int z);
inline float CosInterp(float a, float b, float x);
inline float InterpNoise(float x, float z);
inline float GetHeight(int x, int z, __constant float *parameters, int chunkOfstX, int chunkOfstZ);

__kernel void GenTerrain(
	__constant float *parameters,
	int chunkOffsetX, //chunk offsets are the this chunk's offset from center measured in chunks
	int chunkOffsetZ,
	__global float3 *geometry,
	__global float2 *uvCoords){  
	////////////////////
	//GENERATE TERRAIN//
	////////////////////
	float metersPerBlock = parameters[MetersPerBlock];
	float blocksPerChunk = parameters[BlocksPerChunk];
	
	int blockX = get_global_id(0);
	int blockZ = get_global_id(1);
	
	int chunkWidth = get_global_size(0);
	float realPosX = blockX * metersPerBlock + chunkOffsetX;
	float realPosZ = blockZ * metersPerBlock + chunkOffsetZ;
	
	float height = GetHeight(
		blockX, 
		blockZ, 
		parameters,
		chunkOffsetX/metersPerBlock,
		chunkOffsetZ/metersPerBlock
	);
	
	int index = blockX*chunkWidth+blockZ;
	geometry[index] = (float3)(realPosX, height, realPosZ);
	uvCoords[index] = (float2)((float)blockX/(chunkWidth-1), (float)blockZ/(chunkWidth-1));
}

//todo: even though the normal buffers are uchar/ushort, the kernel treats them like signed variant so there's a lot of accuracy lost. fix it.
__kernel void GenNormals(
	__constant float *parameters,
	int chunkOffsetX, //chunk offsets are the this chunk's offset from center measured in blocks
	int chunkOffsetZ,
	__global float3 *geometry,
	__global ushort3 *normals,
	__global uchar3 *binormals,
	__global uchar3 *tangents){
	////////////////////
	//GENERATE NORMALS//
	////////////////////
	//first we have to generate extra vertex(s) if the worker is generating the 
	//normals for a vertex at the edge of the chunk because its neighboring
	//vertexes may be in the geometry array
    // +             v1 
    // ^              |  
    // |              |
    // Z axis   v4----O----v2
    // |              |
    // v              |
    //               v3
    //           <- X axis -> +
	int blockX = get_global_id(0);
	int blockZ = get_global_id(1);
	int chunkWidth = get_global_size(0);
	int index = blockX+chunkWidth*blockZ;
	float height = 	geometry[index].y;
	float metersPerBlock = parameters[MetersPerBlock];

	float3 v1 = (float3)(0,0,0);
	float3 v2 = (float3)(0,0,0);
	float3 v3 = (float3)(0,0,0);
	float3 v4 = (float3)(0,0,0);
	
	bool v1Init=false;
	bool v2Init=false;
	bool v3Init=false;
	bool v4Init=false;
	
	if(blockX == 0){
		v4 = (float3)(
		-1, 
		GetHeight(blockX-1, blockZ, parameters,chunkOffsetX/metersPerBlock,chunkOffsetZ/metersPerBlock),
		0
		);
		v4Init = true;
	}

	if(blockX == chunkWidth-1){
		v2 = (float3)(
		1, 
		GetHeight(blockX+1, blockZ, parameters,chunkOffsetX/metersPerBlock,chunkOffsetZ/metersPerBlock),
		0
		);
		v2Init = true;
	}
	if(blockZ == 0){
		v3 = (float3)(
		0, 
		GetHeight(blockX, blockZ-1, parameters,chunkOffsetX/metersPerBlock,chunkOffsetZ/metersPerBlock),
		-1
		);
		v3Init = true;
	}
	if(blockZ == chunkWidth-1){
		v1 = (float3)(
		0, 
		GetHeight(blockX, blockZ+1, parameters,chunkOffsetX/metersPerBlock,chunkOffsetZ/metersPerBlock),
		1
		);
		v1Init = true;
	}
	
	//now we fill v1,v2,v3,v4 with vertexes from the
	//geometry array if they havent been filled already
	if(!v1Init){
		v1 = (float3)(
			0,
			geometry[(blockX)*chunkWidth+blockZ+1].y,
			1
		);
	}
	if(!v2Init){
		v2 = (float3)(
			1,
			geometry[(blockX+1)*chunkWidth+blockZ].y,
			0
		);
	}
	if(!v3Init){
		v3 = (float3)(
			0,
			geometry[blockX*chunkWidth+blockZ-1].y,
			-1
		);
	}
	if(!v4Init){
		v4 = (float3)(
			-1,
			geometry[(blockX-1)*chunkWidth+blockZ].y,
			0
		);
	}
	
	//to make this simpler, we assume the center vertex is zero
	v1.y = v1.y - height;
	v2.y = v2.y - height;
	v3.y = v3.y - height;
	v4.y = v4.y - height;

	float3 crossSum = (float3)(0,0,0);
	crossSum += cross(v1, v2);
	crossSum += cross(v2, v3);
	crossSum += cross(v3, v4);
	crossSum += cross(v4, v1);
	
	crossSum = normalize(crossSum);
	v1 = normalize(v1);
	v2 = normalize(v2);

	normals[index] = (ushort3)(
		(ushort)(crossSum.x*16383.0+16384.0),//*16383+16384),
		(ushort)(crossSum.y*16383.0+16384.0),//*16383+16384),
		(ushort)(crossSum.z*16383.0+16384.0)//*16383+16384)
	);
	
	binormals[index] = (uchar3)(
		(uchar)(v1.x*127+128),
		(uchar)(v1.y*127+128),
		(uchar)(v1.z*127+128)
	);
	
	tangents[index] = (uchar3)(
		(uchar)(v2.x*127+128),
		(uchar)(v2.y*127+128),
		(uchar)(v2.z*127+128)
	);
}


inline float GenRidge( float height, float offset ){
	height = fabs(height);
	height = offset - height;
	height = pown(height, 2);
	return height;
}

inline float GenNoise(int x, int z){
	int hash = x + z*57;
	hash = (hash << 13) ^ hash; //is this really an appropriate hashing method?
	return (1.0f - ((hash*(hash*hash*15731 + 789221) + 1376312589)& 0x7fffffff)/1073741824.0f);
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

inline float GetHeight(int x, int z, __constant float *parameters, int chunkOfstX, int chunkOfstZ){
	float lacunarity = parameters[Lacunarity];
	float gain = parameters[Gain];
	float offset = parameters[Offset];
	float octaves = parameters[Octaves];
	float hScale = parameters[HScale];
	float vScale = parameters[VScale];

	float posX = (x + chunkOfstX)* hScale;
	float posZ = (z + chunkOfstZ)* hScale;
		
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