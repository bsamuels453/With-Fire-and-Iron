inline int2 GetVertAssignment(int chunkWidth, int workerId, __constant int *vertAssignments);
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
// +              v0 
// ^               |  
// |               |
// Z axis   v3----v4----v1
// |               |
// v               |
//                v2
//           <- X axis -> +

__kernel void QuadTree(
	int chunkWidth,
	int maxDepth,
	__constant int *vertAssignments,
	__global uchar3 *normals,
	__global bool *activeVerts){
	
	int workerId = get_global_id(0);
	for(int depth=0; depth<maxDepth; depth++){
		int2 vert = GetVertAssignment(
			chunkWidth,
			workerId,
			vertAssignments
		);
		if(vert.x == -1){
			return;
		}

		//notice that even though int2 indexes the second value as y,
		//it still cooresponds with the value on the z axis
		
		int vertIdx[5];
		uchar3 uvert[5];
		float angles[4];
		
		vertIdx[v0] = GetVertIndex(vert.x, vert.y+1, chunkWidth);
		vertIdx[v1] = GetVertIndex(vert.x+1, vert.y, chunkWidth);
		vertIdx[v2] = GetVertIndex(vert.x, vert.y-1, chunkWidth);
		vertIdx[v3] = GetVertIndex(vert.x-1, vert.y, chunkWidth);
		vertIdx[v4] = GetVertIndex(vert.x, vert.y, chunkWidth);
		
		for(int i=0; i<5; i++){
			uvert[i] = normals[vertIdx[i]];
		}
		
		for(int i=0; i<4; i++){
			angles[i] = acos( 
				uchar3Dot(uvert[4], uvert[i]) / 
				(uchar3Mag(uvert[4]) * uchar3Mag(uvert[i]))
			);
		}
		
		bool disableCentVert = true;
		const float minAngle = 10 * 0.174533f;
		for(int i=0; i<4; i++){
			if(angles[i] > minAngle){
				disableCentVert = false;
				break;
			}
		}
		
		if(disableCentVert){
			activeVerts[vertIdx[4]] = false;		
		}
		
		barrier(CLK_GLOBAL_MEM_FENCE);
		
		//Now to disable edge vertexes. 
		if(disableCentVert){
			int stride = (int)pown(2.0f, depth+1);
			int halfstride = stride/2;
				
			//check north neighbor
			if(activeVerts[GetVertIndex(vert.x, vert.y+stride, chunkWidth)] == false){
				activeVerts[GetVertIndex(vert.x, vert.y+halfstride, chunkWidth)] = false;
			}
			//check south neighbor
			if(activeVerts[GetVertIndex(vert.x, vert.y-stride, chunkWidth)] == false){
				activeVerts[GetVertIndex(vert.x, vert.y-halfstride, chunkWidth)] = false;
			}
			//check east neighbor
			if(activeVerts[GetVertIndex(vert.x+stride, vert.y, chunkWidth)] == false){
				activeVerts[GetVertIndex(vert.x+halfstride, vert.y, chunkWidth)] = false;
			}
			//check west neighbor
			if(activeVerts[GetVertIndex(vert.x-stride, vert.y, chunkWidth)] == false){
				activeVerts[GetVertIndex(vert.x-halfstride, vert.y, chunkWidth)] = false;
			}
		}
		
		barrier(CLK_GLOBAL_MEM_FENCE);
	}
}

inline int2 GetVertAssignment(int chunkWidth, int workerId, __constant int *vertAssignments){
	int vertIndex = vertAssignments[workerId];
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