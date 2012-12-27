inline int2 GetVertAssignment(int depth, int chunkWidth, int workerId, __constant int *vertAssignments);
inline int GetVertIndex(int x, int y, int chunkWidth);
//inline float3 cast_char3tfloat3(uchar3 var);
inline float uchar3Dot(uchar3 v1, uchar3 v2);
inline float uchar3Mag(uchar3 vec);

typedef enum{
	v0 = 0,
	v1 = 1, 
	v2 = 2,
	v3 = 3,
	v4 = 4
} VERTEXENUM;

__kernel void QuadTree(
	int chunkWidth,
	int maxDepth,
	__constant int *vertAssignments,
	__global uchar3 *normals,
	__global bool *activeVerts){
	
	int workerId = get_global_id(0);
	
	for(int curDepth=0; curDepth<maxDepth; curDepth++){
		int2 vert = GetVertAssignment(
			curDepth,
			chunkWidth,
			workerId,
			vertAssignments
		);
		//check to see if this worker is necessary for this depth
		if(vert.x == -1){
			return;
		}
		// +              v1 
		// ^               |  
		// |               |
		// Z axis   v4----v0----v2
		// |               |
		// v               |
		//                v3
		//           <- X axis -> +
		//notice that even though int2 indexes the second value as y,
		//it still cooresponds with the value on the z axis
		
		int vertIdx[5];
		uchar3 uvert[5];
		float angles[4];
		
		vertIdx[v0] = GetVertIndex(vert.x, vert.y, chunkWidth);
		vertIdx[v1] = GetVertIndex(vert.x, vert.y+1, chunkWidth);
		vertIdx[v2] = GetVertIndex(vert.x+1, vert.y, chunkWidth);
		vertIdx[v3] = GetVertIndex(vert.x, vert.y-1, chunkWidth);
		vertIdx[v4] = GetVertIndex(vert.x-1, vert.y, chunkWidth);
		
		for(int i=0; i<5; i++){
			uvert[i] = normals[vertIdx[i]];
		}
		
		for(int i=1; i<5; i++){
			angles[i-1] = acos( 
				uchar3Dot(uvert[0], uvert[i]) / 
				(uchar3Mag(uvert[0]) * uchar3Mag(uvert[i]))
			);
		}
		
		/*
		for(int i=0; i<5; i++){
			fvert[i] = cast_char3tfloat3(uvert[i]);
		}
		
		for(int i=0; i<5; i++){
			vertMag[i] = distance(float3(0,0,0), fvert[i]);
		}
		
		
		for(int i=1; i<5; i++){
			angles[i-1] = acos( dot(fvert[0], fvert[i]) / (vertMag[0] * vertMag[i]));
		}*/
	}	
}

inline int2 GetVertAssignment(int depth, int chunkWidth, int workerId, __constant int *vertAssignments){
	int offset = depth * get_global_size(0); + workerId;
	int vertIndex = vertAssignments[offset];
	if( vertIndex == -1){
		return (int2)(-1, -1);
	}
	int vertX = vertIndex / chunkWidth;
	int vertZ = vertIndex - (vertX*chunkWidth);
	return (int2)(vertX, vertZ);	
}

//todo: macro this
inline int GetVertIndex(int x, int y, int chunkWidth){
	return x*chunkWidth+y;
}
/*
inline float3 cast_char3tfloat3(uchar3 var){
	return (float3)( var.x, var.y, var.z);
}
*/

inline float uchar3Mag(uchar3 vec){
	return sqrt(
		pown( (float)vec.x, 2) +
		pown( (float)vec.y, 2) +
		pown( (float)vec.z, 2)
	);
}

inline float uchar3Dot(uchar3 v1, uchar3 v2){
	return 
		((float)v1.x)*((float)v2.x) + 
		((float)v1.y)*((float)v2.y) +
		((float)v1.z)*((float)v2.z)
	;
}