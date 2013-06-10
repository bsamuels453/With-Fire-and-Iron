using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Pathfinding {
    static class Bmp {

        public static void Dot(Bitmap bmp, int x, int y, Color color) {
            const int width = 5;
            for (int xCounter = x; xCounter < width+x; xCounter++) {
                for (int yCounter = y; yCounter < width+y; yCounter++) {
                    bmp.SetPixel(xCounter, yCounter, color);
                }
            }
        }
    }
}
