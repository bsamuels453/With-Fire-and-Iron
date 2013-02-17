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
#define VERTS(x,y) (activeVerts[(x)*treeWidth+(y)])
#define INDICIES(x,y) (indicies[(x)*treeWidth+(y)])

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

//numXThreads = cells*2

__kernel void VertexWinder(
	__constant char* activeVerts,
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
	while(true){
		int2 newPos = dirs[0]*step+pos;
		if(VERTS(newPos.x,newPos.y)==1){
			break;
		}
		else{
			if(canExtend){
				if(step == 1){
					GetExtensionDirections(dirs, windingType);
				}
				step *= 2;
			}
			else{
				return;
			}
		}
	}	
	
	int2 curPos = pos; 
	indexes[0] = curPos.x*(vertWidth)+curPos.y;
	for(int i=1; i<3; i++){
		curPos = dirs[i-1]+curPos;
		indexes[i] = curPos.x*(vertWidth)+curPos.y;
	}
	 
	if(x_id==0 && y_id==0){
		//INDICIES(0,0) = windingType;
		//INDICIES(0,1) = dirs[1].x;
		//INDICIES(0,2) = dirs[2].x;
	}
	//INDICIES(x_id,y_id) = (int3)(indexes[0], indexes[1], indexes[2]);
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
	int2 dirs[3];
	switch(workerType){
		case TOPLEFT_DOWN:
			dirs[0] = (int2)(0,-2);
			dirs[1] = (int2)(1,1);
			dirs[2] = (int2)(-1,1);
			break;
		case BOTTOMLEFT_RIGHT:
			dirs[0] = (int2)(2,0);
			dirs[1] = (int2)(-1,1);
			dirs[2] = (int2)(-1,-1);
			break;
		case BOTTOMRIGHT_UP:
			dirs[0] = (int2)(0,2);
			dirs[1] = (int2)(1,-1);
			dirs[2] = (int2)(1,-1);
			break;
		case TOPRIGHT_LEFT:
			dirs[0] = (int2)(-2,0);
			dirs[1] = (int2)(1,-1);
			dirs[2] = (int2)(1,-1);
			break;
	}
	directions = dirs;
	}