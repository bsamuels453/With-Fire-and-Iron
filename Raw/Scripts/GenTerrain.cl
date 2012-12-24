//todo optimize this, maybe even for vendor-specific if have the time

//since max const parameters to kernel is 8, gotta cram some in an array
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
inline float GetHeight(int x, int z, __constant float *parameters, int chunkOfstX, int chunkOfstZ);

__kernel void GenTerrain(
	__constant float *parameters,
	__constant int *chunkOffsetX, //chunk offsets are the this chunk's offset from center measured in blocks
	__constant int *chunkOffsetZ,
	__global float4 *geometry,
	__write_only image2d_t normals,
	__write_only image2d_t binormals,
	__write_only image2d_t tangents){  
	
	////////////////////
	//GENERATE TERRAIN//
	////////////////////
	float metersPerBlock = parameters[MetersPerBlock];
	int blockX = get_global_id(0);
	int blockZ = get_global_id(1);
	
	int chunkWidth = get_global_size(0);
	float realPosX = (blockX  + *chunkOffsetX) * metersPerBlock;
	float realPosZ = (blockZ  + *chunkOffsetZ) * metersPerBlock;
	
	float height = GetHeight(
		blockX, 
		blockZ, 
		parameters,
		*chunkOffsetX,
		*chunkOffsetZ
	);
	
	geometry[blockX*chunkWidth+blockZ] = (float4)(realPosX,height,realPosZ,-1);
	
	////////////////////
	//GENERATE NORMALS//
	////////////////////
	barrier(CLK_GLOBAL_MEM_FENCE);
	
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
	//notice that 1 is being placed in the w component. this is essentially
	//a flag to let the rest of the program know that the float4 hasn't been
	//assigned any value, since in this application any assigned float4 will
	//have a w value of zero
	float4 v1 = (float4)(0,0,0,1);
	float4 v2 = (float4)(0,0,0,1);
	float4 v3 = (float4)(0,0,0,1);
	float4 v4 = (float4)(0,0,0,1);
	
	if(blockX == 0){
		v4 = (float4)(
		-1, 
		GetHeight(blockX-1, blockZ, parameters,*chunkOffsetX,*chunkOffsetZ),
		0,
		0
		);
	}
	if(blockX == chunkWidth-1){
		v2 = (float4)(
		1, 
		GetHeight(blockX+1, blockZ, parameters,*chunkOffsetX,*chunkOffsetZ),
		0,
		0
		);
	}
	if(blockZ == 0){
		v3 = (float4)(
		0, 
		GetHeight(blockX, blockZ-1, parameters,*chunkOffsetX,*chunkOffsetZ),
		-1,
		0
		);
	}
	if(blockZ == chunkWidth-1){
		v1 = (float4)(
		0, 
		GetHeight(blockX, blockZ+1, parameters,*chunkOffsetX,*chunkOffsetZ),
		1,
		0
		);
	}
	
	//now we fill v1,v2,v3,v4 with vertexes from the
	//geometry array if they havent been filled already
	if(v1.w == 0){
		v1 = geometry[blockX*chunkWidth+blockZ+1];
	}
	if(v2.w == 0){
		v2 = geometry[(blockX+1)*chunkWidth+blockZ];
	}
	if(v3.w == 0){
		v3 = geometry[blockX*chunkWidth+blockZ-1];
	}
	if(v4.w == 0){
		v4 = geometry[(blockX-1)*chunkWidth+blockZ];
	}
	
	//to make this simpler, we assume the center vertex is zero
	float4 centHeight = (float4)(0,height,0,0);
	v1 = v1 - centHeight;
	v2 = v2 - centHeight;
	v3 = v3 - centHeight;
	v4 = v4 - centHeight;
	
	float4 crossSum = (float4)(0,0,0,0);
	crossSum += cross(v1, v2);
	crossSum += cross(v2, v3);
	crossSum += cross(v3, v4);
	crossSum += cross(v4, v1);
	
	write_imagef(normals, (int2)(blockX, blockZ), crossSum);
	write_imagef(binormals, (int2)(blockX, blockZ), v1);
	write_imagef(tangents, (int2)(blockX, blockZ), v2);
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