//Every vertex is assigned two workers. One worker winds a triangle by moving vertically, and the other winds the triangle by moving horizontally.

//take note that this horizontal/vertical worker abstraction is NOT related to the horiz/vertical worker abstraction used in QuadTree.cl
//threadpool tasks assuming quadtree width is 4
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
#define VERTS(x,y) (activeVerts[(x)*vertWidth+(y)])
#define INDICIES(x,y) (indicies[(x)*vertWidth+(y)])

//enum name syntax:
//{pos}_{dir}
//TOPLEFT_RIGHT: position is in topright of quad, is winding triangle to the right
//BOTTOMRIGHT_UP: position is in bottomright of quad, is winding triangle upwards.
typedef enum{
    TOPLEFT_RIGHT,
	TOPLEFT_DOWN,
	BOTTOMLEFT_UP,
	BOTTOMLEFT_RIGHT,
	BOTTOMRIGHT_LEFT,
	BOTTOMRIGHT_UP,
	TOPRIGHT_DOWN,
	TOPRIGHT_LEFT
} WINDINGTYPE;

void GetDirections(int2* directions, WINDINGTYPE workerType);
void GetExtensionDirections(int2* directions, WINDINGTYPE workerType);
void GetRenderDirections(int2* directions, WINDINGTYPE workerType);
void GetExtensionRenderDirections(int2* directions, WINDINGTYPE workerType);

__kernel void VertexWinder(
	__global char* activeVerts,
	__global int3* indicies){
	
	int treeWidth = get_global_size(0);
	int vertWidth =  treeWidth+1;
	int x_id = get_global_id(0);
	int y_id = get_global_id(1);
	int indiceIdx = x_id+y_id*treeWidth;

	bool horizontalWorker = true;
	if(y_id >= treeWidth){
		horizontalWorker = false;
		y_id -= treeWidth;
	}

	//generate the worker origin point and the direction in which the worker winds the triangle
	int x_pos, y_pos;
	WINDINGTYPE windingType;
	
	bool isTop,isLeft;
	if(x_id%2==0){
		x_pos = x_id;
		isLeft = true;
	}
	else{
		x_pos = x_id+1;
		isLeft = false;
	}
	if(y_id%2==0){
		y_pos = y_id;
		isTop = true;
	}
	else{
		y_pos = y_id+1;
		isTop = false;
	}
	
	if(VERTS(x_pos, y_pos)==false){
		return;
	}
	
	//this mess of code figures out where and how this worker will  perform the winding
	bool canExtend=false;
	if(isTop)
		if(isLeft)
			if(horizontalWorker){
				windingType = TOPLEFT_RIGHT;
			}
			else{
				windingType = TOPLEFT_DOWN;
				canExtend = true;
			}
		else
			if(horizontalWorker){
				windingType = TOPRIGHT_LEFT;
				canExtend = true;
			}
			else{
				windingType = TOPRIGHT_DOWN;
			}
	else
		if(isLeft)
			if(horizontalWorker){
				windingType = BOTTOMLEFT_RIGHT;
				canExtend = true;
			}
			else{
				windingType = BOTTOMLEFT_UP;
			}
		else
			if(horizontalWorker){
				windingType = BOTTOMRIGHT_LEFT;
			}
			else{
				windingType = BOTTOMRIGHT_UP;
				canExtend = true;
			}
	int2 pos = (int2)(x_pos, y_pos);
	int indexes[3];
	int2 dirs[3];
	GetDirections(dirs, windingType);
	int step=1;
	bool hasExtended = false;
	int dirToCheck=0;
	int2 checkPos = pos;
	while(true){
		if(hasExtended)
			GetExtensionDirections(dirs, windingType);
		else
			GetDirections(dirs, windingType);
		
		for(int i=0; i<3; i++){
			dirs[i] = dirs[i] * step;
		}
		int2 newPos = dirs[dirToCheck]+checkPos;
		if(VERTS(newPos.x,newPos.y)==1){
			if(dirToCheck == 2){
				break;
			}
			
			dirToCheck++;
			checkPos = newPos;
		}
		else{
			if(canExtend){
				if(!hasExtended){
					hasExtended = true;
					dirToCheck=0;
					checkPos = pos;
					if(x_pos%(step*2) != 0 || y_pos%(step*2) != 0){
						return;
					}
				}
				else{
					step *= 2;
					hasExtended = false;
					dirToCheck=0;
					checkPos = pos;
				}
			}
			else{
				return;
			}
		}
	}	
	
	if(hasExtended){
		GetExtensionRenderDirections(dirs, windingType);
	}
	else{
		GetRenderDirections(dirs, windingType);
	}
	for(int i=0; i<3; i++){
		dirs[i] = dirs[i] * step;
	}
	
	
	int2 curPos = pos; 
	indexes[0] = curPos.x*(vertWidth)+curPos.y;
	//int2 idxs[3];
	//idxs[0] = curPos;
	for(int i=1; i<3; i++){
		curPos = dirs[i-1]+curPos;
		//idxs[i] = curPos;
		indexes[i] = curPos.x*(vertWidth)+curPos.y;
	}
	indicies[indiceIdx]= (int3)(indexes[0], indexes[1], indexes[2]);
	}

	//this hardcoding is necessary until I can figure out a way to do it programatically
void GetDirections(int2* directions, WINDINGTYPE workerType){
	switch(workerType){
		case TOPLEFT_RIGHT:
			directions[0] = (int2)(1,0);
			directions[1] = (int2)(0,1);
			directions[2] = (int2)(-1,-1);
			break;
		case TOPLEFT_DOWN:
			directions[0] = (int2)(0,1);
			directions[1] = (int2)(1,0);
			directions[2] = (int2)(-1,-1);
			break;
		case BOTTOMLEFT_UP:
			directions[0] = (int2)(0,-1);
			directions[1] = (int2)(1,0);
			directions[2] = (int2)(-1,1);
			break;
		case BOTTOMLEFT_RIGHT:
			directions[0] = (int2)(1,0);
			directions[1] = (int2)(0,-1);
			directions[2] = (int2)(-1,1);
			break;
		case BOTTOMRIGHT_LEFT:
			directions[0] = (int2)(-1,0);
			directions[1] = (int2)(0,-1);
			directions[2] = (int2)(1,1);
			break;
		case BOTTOMRIGHT_UP:
			directions[0] = (int2)(0,-1);
			directions[1] = (int2)(-1,0);
			directions[2] = (int2)(1,1);
			break;
		case TOPRIGHT_DOWN:
			directions[0] = (int2)(0,1);
			directions[1] = (int2)(-1,0);
			directions[2] = (int2)(1,-1);
			break;
		case TOPRIGHT_LEFT:
			directions[0] = (int2)(-1,0);
			directions[1] = (int2)(0,1);
			directions[2] = (int2)(1,-1);
			break;
	}
	}

void GetExtensionDirections(int2* directions, WINDINGTYPE workerType){
	switch(workerType){
		case TOPLEFT_DOWN:
			directions[0] = (int2)(0,2);
			directions[1] = (int2)(1,-1);
			directions[2] = (int2)(-1,-1);
			break;
		case BOTTOMLEFT_RIGHT:
			directions[0] = (int2)(2,0);
			directions[1] = (int2)(-1,-1);
			directions[2] = (int2)(-1,1);
			break;
		case BOTTOMRIGHT_UP:
			directions[0] = (int2)(0,-2);
			directions[1] = (int2)(-1,1);
			directions[2] = (int2)(1,-1);
			break;
		case TOPRIGHT_LEFT:
			directions[0] = (int2)(-2,0);
			directions[1] = (int2)(1,1);
			directions[2] = (int2)(1,-1);
			break;
	}
	}
	
//HARDCODING FOR THE HARDCODING GOD
//We need these extra methods for getting how a triangle will be actually wound when rendered.
//The above winding directions are only the way they are for the reduction-function to work. 
//These methods are defined such that the triangle indexes will be defined in a clockwise manner.

void GetRenderDirections(int2* directions, WINDINGTYPE workerType){
	switch(workerType){
		case TOPLEFT_RIGHT:
			directions[0] = (int2)(1,0);
			directions[1] = (int2)(0,1);
			directions[2] = (int2)(-1,-1);
			break;
		case TOPLEFT_DOWN:
			directions[0] = (int2)(1,1);
			directions[1] = (int2)(-1,0);
			directions[2] = (int2)(0,-1);
			break;
		case BOTTOMLEFT_UP:
			directions[0] = (int2)(0,-1);
			directions[1] = (int2)(1,0);
			directions[2] = (int2)(-1,1);
			break;
		case BOTTOMLEFT_RIGHT:
			directions[0] = (int2)(1,-1);
			directions[1] = (int2)(0,1);
			directions[2] = (int2)(-1,0);
			break;
		case BOTTOMRIGHT_LEFT:
			directions[0] = (int2)(-1,0);
			directions[1] = (int2)(0,-1);
			directions[2] = (int2)(1,1);
			break;
		case BOTTOMRIGHT_UP:
			directions[0] = (int2)(-1,-1);
			directions[1] = (int2)(1,0);
			directions[2] = (int2)(0,1);
			break;
		case TOPRIGHT_DOWN:
			directions[0] = (int2)(0,1);
			directions[1] = (int2)(-1,0);
			directions[2] = (int2)(1,-1);
			break;
		case TOPRIGHT_LEFT:
			directions[0] = (int2)(-1,1);
			directions[1] = (int2)(0,-1);
			directions[2] = (int2)(1,0);
			break;
	}
	}

void GetExtensionRenderDirections(int2* directions, WINDINGTYPE workerType){
	switch(workerType){
		case TOPLEFT_DOWN:
			directions[0] = (int2)(1,1);
			directions[1] = (int2)(-1,1);
			directions[2] = (int2)(0,-2);
			break;
		case BOTTOMLEFT_RIGHT:
			directions[0] = (int2)(1,-1);
			directions[1] = (int2)(1,1);
			directions[2] = (int2)(-2,0);
			break;
		case BOTTOMRIGHT_UP:
			directions[0] = (int2)(-1,-1);
			directions[1] = (int2)(1,-1);
			directions[2] = (int2)(0,2);
			break;
		case TOPRIGHT_LEFT:
			directions[0] = (int2)(-1,1);
			directions[1] = (int2)(-1,-1);
			directions[2] = (int2)(2,0);
			break;
	}
	}