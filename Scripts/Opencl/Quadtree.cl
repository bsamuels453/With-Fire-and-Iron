//todo:a damn header file
typedef enum{
	HORIZONTAL,
	VERTICAL
} WORKERTYPE;

bool IsVertexRelevant(short3 *verts);
char AreCornersEqual(
    __global char *activeVerts,
    int width,
    int centerX,
    int centerZ,
    int radius,
    char desiredVal,
	__global int* dummy
);
char AreSidesEqual(
    __global char *activeNodes,
    int chunkWidth,
    int centerX,
    int centerZ,
    int radius,
    char desiredVal
);

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
    
__kernel void QuadTree(
	int depth,
    int chunkBlockWidth,
    __global short3 *normals,
    __global char *activeNodes,
	__global int* dummy
	){    
		WORKERTYPE type;
		if( get_global_id(1) < get_global_size(1)/2){
			type = HORIZONTAL;
		}
		else{
			type = VERTICAL;
		}
		
        //generate relevant information
        int chunkVertWidth = chunkBlockWidth+1;
		int x_id, z_id, x_pos, z_pos;
		if(type == HORIZONTAL){
			x_id = get_global_id(0);
			z_id = get_global_id(1);
			x_pos = x_id;
			z_pos = z_id;
		}
		else{
			z_id = get_global_id(0);
			x_id = get_global_id(1)-get_global_size(1)/2;
			x_pos = z_id;
			z_pos = x_id;
		}

        //int curCellWidth = depth*2+2;
		int curCellWidth = pown(2.0,depth)*2;
        int startPoint = curCellWidth/2;
        int step = curCellWidth;
            
		int pointX, pointZ;
		if(type == HORIZONTAL){
			pointX = x_id*step+curCellWidth;
			pointZ = z_id*step+startPoint;
		}
		else{
			pointX = x_id*step+startPoint;
			pointZ = z_id*step+curCellWidth;
		}
           //check if horizontal removal is even valid
           //make sure each corner is inactive(false)
        bool canSetNode = true;
        if(depth != 0){
            canSetNode = AreCornersEqual(
                activeNodes,
                chunkVertWidth,
                pointX,
                pointZ,
                pown(2.0,depth-1),
                false,
			   	 dummy
                );
        }
        if( canSetNode){
            //now see if we can disable this node
            short3 verts[5];
            verts[v0] = normals[pointX + (pointZ*chunkVertWidth)+step/2];
            verts[v1] = normals[(pointX+step/2) + pointZ*chunkVertWidth];
            verts[v2] = normals[pointX + (pointZ*chunkVertWidth)-step/2];
            verts[v3] = normals[(pointX-step/2) + pointZ*chunkVertWidth];
            verts[v4] = normals[pointX + pointZ*chunkVertWidth];

            if(!IsVertexRelevant(verts)){
                activeNodes[chunkVertWidth*pointX + pointZ] = 0;        
            }
        }

    }

__kernel void CrossCull(
    int depth,
	int chunkBlockWidth,
    __global short3* normals,
    __global char* activeNodes,
	__global int* dummy
	){
		int x_id = get_global_id(0);
		int z_id = get_global_id(1);
		int x_max = get_global_size(0);
		int z_max = get_global_size(1);
        int cellWidth = pown(2.0,depth)*2;
        //these coorespond to which cell this worker is going to try to cull
        int x_cell = x_id;
        int z_cell = z_id;
        int x_vert = x_cell * cellWidth+cellWidth/2;//+pown(2.0,depth)
        int z_vert = z_cell * cellWidth+cellWidth/2;//+pown(2.0,depth)

        //make sure we're able to cull the cross
        //first check outer corners. They must be enabled.
        if( !AreCornersEqual(
            activeNodes,
            chunkBlockWidth+1,
            x_vert,
            z_vert,
            cellWidth/2,
            true,
			dummy
            )){
                return;
        }
        //check the outer "sides"; they should be disabled for the cross
        //to be removed properly
        if( !AreSidesEqual(
           activeNodes,
            chunkBlockWidth+1,
            x_vert,
            z_vert,
            cellWidth/2,
            false
            )){
                return;
        }
        
        //now check inset corners, we want them to be disabled
        for(int subDepth = depth-1; subDepth>=0; subDepth--){
            if( !AreCornersEqual(
                activeNodes,
                chunkBlockWidth+1,
                x_vert,
                z_vert,
                pown(2.0,subDepth),
                false,
				dummy
                )){
                    return;
            }
        }
        
        //okay, at this point we know that a cross cull would be valid
        //check to see if it's necessary now
        short3 verts[5];
        int chunkVertWidth = chunkBlockWidth+1;
        verts[v0] = normals[x_vert + (z_vert*chunkVertWidth)+cellWidth/2];
        verts[v1] = normals[(x_vert+cellWidth/2) + z_vert*chunkVertWidth];
        verts[v2] = normals[x_vert + (z_vert*chunkVertWidth)-cellWidth/2];
        verts[v3] = normals[(x_vert-cellWidth/2) + z_vert*chunkVertWidth];
        verts[v4] = normals[x_vert + z_vert*chunkVertWidth];

        if(!IsVertexRelevant(verts)){
            activeNodes[x_vert*chunkVertWidth+z_vert] = 0;        
        }
        return;
    }

char AreCornersEqual(
    __global char *activeNodes,
    int chunkVertWidth,
    int centerX,
    int centerZ,
    int radius,
    char desiredVal,
	__global int* dummy
	){ 
        char ret=1;
        //xx these gotos are probably going to cause issues with
        //irreducible control flow, try testing to see if the compiler's
        //optimizations with flag perform better than goto
        for(int x=centerX-radius; x<=centerX+radius; x+=radius*2){
			for(int z=centerZ-radius; z<=centerZ+radius; z+=radius*2){
                if( activeNodes[x*chunkVertWidth+z] != desiredVal ){
				dummy[0]=1;
                    ret = 0;
                    goto brkLoop;
                }
            }
        }
        brkLoop:
        return ret;
    }

char AreSidesEqual(
    __global char *activeNodes,
    int chunkWidth,
    int centerX,
    int centerZ,
    int radius,
    char desiredVal){
        char ret=1;
        //it would have probably been a better idea to just hardcode these loops
        for(int x=centerX-radius; x<=centerX+radius; x+=radius*2){
            if( activeNodes[x*chunkWidth+centerZ] != desiredVal ){
                ret = 0;
                goto brkLoop;
            }
        }
        for(int z=centerZ-radius; z<=centerZ+radius; z+=radius*2){
            if( activeNodes[centerX*chunkWidth+z] != desiredVal ){
                ret = 0;
                goto brkLoop;
            }
        }
        brkLoop:
        return ret;
    }

float Magnitude(float3 vec){
	return sqrt(vec.x*vec.x+vec.y*vec.y+vec.z+vec.z);
}
    
bool IsVertexRelevant(short3 *verts){
    float angles[4];
	float3 fVerts[5];
	for(int i=0; i<5; i++){
		fVerts[i] = normalize((float3)((float)(verts[i].x), (float)(verts[i].y), (float)(verts[i].z)));
	}
    
    for(int i=0; i<4; i++){
        angles[i] = acos( 
            dot(fVerts[4], fVerts[i])
        )*57.29;
		angles[i] = fabs(angles[i]);
    }

    bool disableCentNode = true;
    float minAngle = 14;
    for(int i=0; i<4; i++){
        if(angles[i] > minAngle){
            disableCentNode = false;
            break;
        }
    }
    return !disableCentNode;
}  