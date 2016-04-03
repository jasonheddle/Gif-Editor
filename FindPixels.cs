//Copyright (C) 2016 Jason Heddle

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//   http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GIF_Editor
{
    class FindPixels
    {
        /// <summary>
        /// Finds all the pixels that are within a circle of a given diameter, and center
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <returns>All the pixels within a circle</returns>
        public static int[][] Circle(int diameter, int centerX, int centerY)
        {
            int radius = diameter / 2;
            List<int> pixelsX = new List<int>();
            List<int> pixelsY = new List<int>();

            for (int x = centerX - radius; x < centerX + radius; x++)
            {
                for (int y = centerY - radius; y < centerY + radius; y++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= radius * radius)
                    {
                        int distance = y - (centerY - radius);
                        for (int i = 0; i < diameter - distance * 2; i++)
                        {
                            pixelsX.Add(x);
                            pixelsY.Add(y + i);
                        }
                        break;
                    }
                }
            }

            int[][] pixels = { pixelsX.ToArray(), pixelsY.ToArray() };
            return pixels;
        }

        /// <summary>
        /// Finds all pixels that form a line
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns>All points within the two points given</returns>
        public static List<Point> Line(Point p0, Point p1)
        {
            int x0 = (int)p0.X;
            int y0 = (int)p0.Y;
            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            var points = new List<Point>();

            while (true)
            {
                points.Add(new Point(x0, y0));
                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }

            return points;
        }
    }
}
