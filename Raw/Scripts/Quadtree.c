bool IsVertexRelevant(float3 *verts);
bool AreCornersEqual(__global bool *activeVerts,int width,int centerX,int centerZ,int radius,bool desiredVal);
bool AreSidesEqual(__global bool *activeNodes,int chunkWidth,int centerX,int centerZ,int radius,bool desiredVal);
void CrossCull(int cellWidth,int chunkBlockWidth,int curDepth,__global float3 *normals,__global bool *activeNodes);

typedef enum{
    v0 = 0,
    v1 = 1, 
    v2 = 2,
    v3 = 3,
    v4 = 4
} VERTEXENUM;
//VERTEXENUM For edge tests:
// +              v0 
// ^               |  
// |               |
// Z axis   v3----v4----v1
// |               |
// v               |
//                v2
//           <- X axis -> +

//VERTEXENUM For cross tests:
// +        v0----------v1
// ^               |  
// |               |
// Z axis         v4
// |               |
// v               |
//          v3----------v2
//           <- X axis -> +
//notice that even though int2 indexes the second value as y,(??)
//it still cooresponds with the value on the z axis

//threadpool tasks assuming pool width is 4
//     x
//   0 1 2 3 
//  0\ \ \ \horizontal workers
//  1\ \ \ \
//  2\ \ \ \
//z 3\ \ \ \
//  4/ / / /vertical workers
//  5/ / / /
//  6/ / / /
//  7/ / / /

//clockwise everything
__kernel void QuadTree(
    int chunkWidth,//measured in blocks
    int maxDepth,
    __global float3 *normals,
    __global bool *activeVerts){
           
        if( get_global_id(1) <= get_global_size(1)/2){//xx
            //launch horizontal task
        }
        else{
            //launch vertical task   
        }
    }


inline void HorizontalWorker(
    int chunkBlockWidth,
    int maxDepth,
    __global float3 *normals,
    __global bool *activeNodes){
        //generate relevant information
        int chunkVertWidth = chunkBlockWidth+1;
        int x_id = get_global_id(0);
        int z_id = get_global_id(1);
        
        int curDepth=0;
        int curCellWidth = curDepth*2+2;
        int startPoint = curCellWidth/2;
        int step = curCellWidth;
        
        int pointX = x_id*step+curCellWidth;
        int pointZ = z_id*step+startPoint+curCellWidth;
        
        //check if horizontal removal is even valid
        //make sure each corner is inactive(false)
        if( !AreCornersEqual(
            activeNodes,
            chunkVertWidth,
            pointX,
            pointZ,
            curDepth,
            false
            )){
                //break or something
        }
        
        //now see if we can disable this node
        float3 verts[5];
        verts[v0] = normals[chunkVertWidth*pointX + pointZ+step/2];
        verts[v1] = normals[chunkVertWidth*(pointX+step/2) + pointZ];
        verts[v2] = normals[chunkVertWidth*pointX + pointZ-step/2];
        verts[v3] = normals[chunkVertWidth*(pointX-step/2) + pointZ];
        verts[v4] = normals[chunkVertWidth*pointX + pointZ];
        
        if(!IsVertexRelevant(verts)){
            activeNodes[chunkVertWidth*pointX + pointZ] = false;        
        }
        
        //wait until all orthogonal culling is completed
        barrier(CLK_GLOBAL_MEM_FENCE);
        //figure out if this thread is going to do cross culling
        //if not, waits at the next fence like a good little worker
        CrossCull(
            curCellWidth,
            chunkBlockWidth,
            curDepth,
            normals,
            activeNodes
            );
        barrier(CLK_GLOBAL_MEM_FENCE);
        
        
        //int xThreads = chunkWidth/2-1;
        //int zThreads = chunkWidth/2;
    }
    
void CrossCull(
    int cellWidth,
    int chunkBlockWidth,
    int curDepth,
    __global float3 *normals,
    __global bool *activeNodes){
    
        int x_id = get_global_id(0);
        int z_id = get_global_id(1);
        
        int x_max = get_global_size(0);
        int z_max = get_global_size(1);

        //this enumerates the 2d array of worker ids into a 1d array of super_ids
        int super_id = x_max*x_id+z_id;
        int numCells = chunkBlockWidth/cellWidth;
        
        //to figure out whether or not this worker should do the crosscull, 
        //we see if its super_id would fit in a 1d array enumerated from [numCells, numCells]
        if(super_id/numCells >= numCells){
            //this worker doesnt do the crosscull
            return;
        }
        
		//these coorespond to which cell this worker is going to try to cull
        int x_cell = super_id/numCells;
        int z_cell = super_id - x_cell*numCells;
		int x_vert = x_cell * cellWidth + 1;
		int z_vert = z_cell * cellWidth + 1;
        
		//make sure we're able to cull the cross
		//first check outer corners. They must be enabled.
        if( !AreCornersEqual(
			activeNodes,
            chunkBlockWidth+1,
            x_cell,
            z_cell,
            pown((float)2,curDepth),
            true
            )){
                return;
        }
		//check the outer "sides"; they should be disabled for the cross
		//to be removed properly
		if( !AreSidesEqual(
           activeNodes,
            chunkBlockWidth+1,
            x_cell,
            z_cell,
            pown((float)2,curDepth),
            false
            )){
                return;
        }
		
		//now check inset corners, we want them to be disabled
		for(int depth = curDepth-1; depth>=0; depth--){
			if( !AreCornersEqual(
				activeNodes,
				chunkBlockWidth+1,
				x_cell,
				z_cell,
				pown((float)2,depth),
				false
				)){
					return;
			}
		}
		
		//okay, at this point we know that a cross cull would be valid
		//check to see if it's necessary now
		float3 verts[5];
		int chunkVertWidth = chunkBlockWidth+1;
        verts[v0] = normals[chunkVertWidth*(x_vert-cellWidth/2) + z_vert+cellWidth/2];
        verts[v1] = normals[chunkVertWidth*(x_vert+cellWidth/2) + z_vert+cellWidth/2];
        verts[v2] = normals[chunkVertWidth*(x_vert-cellWidth/2) + z_vert-cellWidth/2];
        verts[v3] = normals[chunkVertWidth*(x_vert+cellWidth/2) + z_vert-cellWidth/2];
        verts[v4] = normals[chunkVertWidth*x_vert + z_vert];

		if(!IsVertexRelevant(verts)){
			activeNodes[x_vert*chunkVertWidth+z_vert] = false;		
		}
		return;
    }

bool AreCornersEqual(
    __global bool *activeNodes,
    int chunkWidth,
    int centerX,
    int centerZ,
    int radius,
    bool desiredVal){
        
        bool ret=true;
		//xx all these gotos are probably going to cause issues with
		//irreducible control flow, try testing to see if the compiler's
		//optimizations with flag perform better than goto
        for(int x=centerX-radius; x<=centerX+radius; x+=radius*2){
            for(int z=centerZ-radius; z<=centerZ+radius; z+=radius*2){
                if( activeNodes[x*chunkWidth+z] != desiredVal ){
                    ret = false;
                    goto brkLoop;
                }
            }
        }
        brkLoop:
        return ret;
    }
    
bool AreSidesEqual(
    __global bool *activeNodes,
    int chunkWidth,
    int centerX,
    int centerZ,
    int radius,
    bool desiredVal){
        
        bool ret=true;
        //it would have probably been a better idea to just hardcode these loops
        for(int x=centerX-radius; x<=centerX+radius; x+=radius*2){
            if( activeNodes[x*chunkWidth+centerZ] != desiredVal ){
                ret = false;
                goto brkLoop;
            }
        }
        for(int z=centerZ-radius; z<=centerZ+radius; z+=radius*2){
            if( activeNodes[centerX*chunkWidth+z] != desiredVal ){
                ret = false;
                goto brkLoop;
            }
        }
        brkLoop:
        return ret;
    }

inline float Magnitude(float3 vec){
    return distance( (0,0,0), vec);
}

bool IsVertexRelevant(float3 *verts){
    float angles[4];
    
    for(int i=0; i<4; i++){
        angles[i] = acos( 
            dot(verts[4], verts[i]) / 
            (Magnitude(verts[4]) * Magnitude(verts[i]))
        );
    }
    
    bool disableCentNode = true;
    const float minAngle = 10 * 0.174533f;
    for(int i=0; i<4; i++){
        if(angles[i] > minAngle){
            disableCentNode = false;
            break;
        }
    }
    return !disableCentNode;
}



        //vertIdx[v0] = GetVertIndex(center.x-radius, center.y+radius, chunkWidth);
        //vertIdx[v1] = GetVertIndex(center.x+radius, center.y+radius, chunkWidth);
        //vertIdx[v2] = GetVertIndex(center.x+radius, center.y-radius, chunkWidth);
        //vertIdx[v3] = GetVertIndex(center.x-radius, center.y-radius, chunkWidth);
        //vertIdx[v4] = GetVertIndex(center.x, center.y, chunkWidth);

        //int vertIdx[5];
          //      vertIdx[v0] = GetVertIndex(center.x, center.y+radius, chunkWidth);
       // vertIdx[v1] = GetVertIndex(center.x+radius, center.y, chunkWidth);
       // vertIdx[v2] = GetVertIndex(center.x, center.y-radius, chunkWidth);
      //  vertIdx[v3] = GetVertIndex(center.x-radius, center.y, chunkWidth);
     //   vertIdx[v4] = GetVertIndex(center.x, center.y, chunkWidth);
