//todo: a damn header file
bool IsVertexRelevant(float3 *verts);
bool AreCornersEqual(
    __global bool *activeVerts,
    int width,
    int centerX,
    int centerZ,
    int radius,
    bool desiredVal
);
bool AreSidesEqual(
    __global bool *activeNodes,
    int chunkWidth,
    int centerX,
    int centerZ,
    int radius,
    bool desiredVal
);
void CrossCull(
    int x_id,
    int z_id,
    int x_max,
    int z_max,
    int cellWidth,
    int chunkBlockWidth,
    int curDepth,
    __constant float3 *normals,
    __global bool *activeVerts
);
void HorizontalWorker(
    int chunkBlockWidth,
    int maxDepth,
    __constant float3 *normals,
    __global bool *activeVerts
);
void VerticalWorker(
    int chunkBlockWidth,
    int maxDepth,
    __constant float3 *normals,
    __global bool *activeVerts
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
    int chunkWidth,
    int maxDepth,
    __constant float3* normals, 
    __global bool* activeVerts){
    
    if( get_global_id(1) < get_global_size(1)/2){
         HorizontalWorker(chunkWidth, maxDepth, normals, activeVerts);
    }
    else{
         VerticalWorker(chunkWidth, maxDepth, normals, activeVerts);
    }
    return;    
}

void CrossCull(
    int x_id,
    int z_id,
    int x_max,
    int z_max,
    int cellWidth,
    int chunkBlockWidth,
    int curDepth,
    __constant float3* normals,
    __global bool* activeNodes){

        //this enumerates the 2d array of worker ids into a 1d array of super_ids
        int super_id = x_id+z_id*x_max;
        int numCells = chunkBlockWidth/cellWidth;
        
        //to figure out whether or not this worker should do the crosscull, 
        //we see if its super_id would fit in a 1d array enumerated from [numCells, numCells]
        if(super_id/numCells >= numCells){
            //this worker doesnt participate in crossculling
            return;
        }
        
        //these coorespond to which cell this worker is going to try to cull
        int x_cell = super_id/numCells;
        int z_cell = super_id - x_cell*numCells;
        int x_vert = x_cell * cellWidth+pown(2.0,curDepth);
        int z_vert = z_cell * cellWidth+pown(2.0,curDepth);

        //make sure we're able to cull the cross
        //first check outer corners. They must be enabled.
        if( !AreCornersEqual(
            activeNodes,
            chunkBlockWidth+1,
            x_vert,
            z_vert,
            pown(2.0,curDepth),
            true
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
            pown(2.0,curDepth),
            false
            )){
                return;
        }
        
        //now check inset corners, we want them to be disabled
        for(int depth = curDepth-1; depth>=0; depth--){
            if( !AreCornersEqual(
                activeNodes,
                chunkBlockWidth+1,
                x_vert,
                z_vert,
                pown(2.0,depth),
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
    int chunkVertWidth,
    int centerX,
    int centerZ,
    int radius,
    bool desiredVal){ 
        bool ret=true;
        //xx these gotos are probably going to cause issues with
        //irreducible control flow, try testing to see if the compiler's
        //optimizations with flag perform better than goto
        for(int x=centerX-radius; x<=centerX+radius; x+=radius*2){
        for(int z=centerZ-radius; z<=centerZ+radius; z+=radius*2){
                if( activeNodes[x*chunkVertWidth+z] != desiredVal ){
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
    
bool IsVertexRelevant(float3 *verts){
    /*
    float angles[4];
    
    for(int i=0; i<4; i++){
        angles[i] = acos( 
            dot(verts[4], verts[i]) / 
            (Magnitude(verts[4]) * Magnitude(verts[i]))
        );
    }
    */
    bool disableCentNode = true;/*
    const float minAngle = 10 * 0.174533f;
    for(int i=0; i<4; i++){
        if(angles[i] > minAngle){
            disableCentNode = false;
            break;
        }
    }*/
    return !disableCentNode;
}

//todo: combine horizontal and vertical worker methods because they have so much alike
void HorizontalWorker(
    int chunkBlockWidth,
    int maxDepth,
    __constant float3 *normals,
    __global bool *activeNodes){    
    
        //generate relevant information
        int chunkVertWidth = chunkBlockWidth+1;
        int x_id = get_global_id(0);
        int z_id = get_global_id(1);
        
        for(int curDepth=1; curDepth <= maxDepth; curDepth++){
            int curCellWidth = curDepth*2+2;
            int startPoint = curCellWidth/2;
            int step = curCellWidth;
            
            int pointX = x_id*step+curCellWidth;
            int pointZ = z_id*step+startPoint;

            //check if horizontal removal is even valid
            //make sure each corner is inactive(false)
            bool canSetNode = true;
            if(curDepth != 0){
                canSetNode = AreCornersEqual(
                    activeNodes,
                    chunkVertWidth,
                    pointX,
                    pointZ,
                    curDepth,//xxx
                    false
                    );
            }
            if( canSetNode){
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
            }
            //wait until all orthogonal culling is completed
            barrier(CLK_GLOBAL_MEM_FENCE);
            //figure out if this thread is going to do cross culling
            //if not, waits at the next fence like a good little worker
            CrossCull(
                x_id,
                z_id,
                chunkBlockWidth/(curCellWidth),
                chunkBlockWidth/(curCellWidth),
                curCellWidth,
                chunkBlockWidth,
                curDepth,
                normals,
                activeNodes
                );
            barrier(CLK_GLOBAL_MEM_FENCE);
        
            //see if this worker needs to be culled
            int numXWorkers = chunkBlockWidth/(curCellWidth*2)-1;
            int numZWorkers = chunkBlockWidth/(curCellWidth*2);

            if(x_id >= numXWorkers)
                break;
            if(z_id >= numZWorkers)
                break;
        }
    }
    
void VerticalWorker(
    int chunkBlockWidth,
    int maxDepth,
    __constant float3 *normals,
    __global bool *activeNodes){
       //generate relevant information
        int chunkVertWidth = chunkBlockWidth+1;
        int z_id = get_global_id(0);////
        int x_id = get_global_id(1)-get_global_size(1)/2;////
        for(int curDepth=1; curDepth <= maxDepth; curDepth++){
            int curCellWidth = curDepth*2+2;
            int startPoint = curCellWidth/2;
            int step = curCellWidth;
            
            int pointX = x_id*step+startPoint;////
            int pointZ = z_id*step+curCellWidth;////

            //check if vertical removal is even valid
            //make sure each corner is inactive(false)
            bool canSetNode = true;
            if(curDepth != 0){
                canSetNode = AreCornersEqual(
                activeNodes,
                chunkVertWidth,
                pointX,
                pointZ,
                curDepth,
                false
                );
            }

            if( canSetNode){
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
            }
            //wait until all orthogonal culling is completed
            barrier(CLK_GLOBAL_MEM_FENCE);
            //figure out if this thread is going to do cross culling
            //if not, waits at the next fence like a good little worker
            CrossCull(
                z_id,
                x_id,
                chunkBlockWidth/(curCellWidth),
                chunkBlockWidth/(curCellWidth),
                curCellWidth,
                chunkBlockWidth,
                curDepth,
                normals,
                activeNodes
                );
            barrier(CLK_GLOBAL_MEM_FENCE);

            //see if this worker needs to be culled
            int numZWorkers = chunkBlockWidth/(curCellWidth*2)-1;
            int numXWorkers = chunkBlockWidth/(curCellWidth*2);

            if(x_id >= numXWorkers){
                break;
            }
            if(z_id >= numZWorkers){
                break;
            }
        }
    }