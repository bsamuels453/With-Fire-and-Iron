using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gondola.Common {
    static class RenderHelper {
        public static Matrix CalculateViewMatrix(Vector3 position, Angle3 lookDir){
            Matrix rotation = Matrix.CreateFromYawPitchRoll(lookDir.Yaw, -lookDir.Pitch, 0);
            Vector3 transformedReference = Vector3.Transform(new Vector3(0,0,1), rotation);
            Vector3 cameraDirection = position + transformedReference;
            return Matrix.CreateLookAt(position, cameraDirection, Vector3.Up);
        }
    }
}
