//this file contains scrap code that may be useful in the future

/*
var ship = Matrix.CreateWorld(new Vector3(0,0, 0), new Vector3(-1, 0, 0), Vector3.Up);
var projectile = Matrix.CreateWorld(new Vector3(-2, 0, -2), Vector3.Forward, Vector3.Up);

var invShip = Matrix.Invert(ship);

var moved = MultMatrix(invShip, new Vector3(-2, 10, -2));

Vector3 v1, translation;
Quaternion rot;
            
Func<Matrix, Matrix, Vector3> worldSpaceToModel = (objMtx, refMtx) => {
    var translatedPos = objMtx - refMtx;
    var transposedPos = Matrix.Transpose(refMtx);
    var localPos = translatedPos * transposedPos;
    return localPos.Translation;
};
*
/*
ship.Decompose(out v1, out rot, out translation);
var matrixTrans = Matrix.CreateTranslation(translation);

rot.W *= -1;//reverse rotation direction
var revRotMatrix = Matrix.CreateFromQuaternion(rot);

var finalProj = projectile * revRotMatrix;
matrixTrans = matrixTrans * revRotMatrix;
finalProj = finalProj - matrixTrans;

var originalPos = finalProj * ship + ship;
var test = Matrix.CreateTranslation(2, 0, -1);
var originalPos2 = test * ship + ship;
 */