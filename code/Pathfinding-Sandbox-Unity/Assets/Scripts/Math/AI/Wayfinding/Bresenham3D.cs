/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using System;
using System.Collections.Generic;

using UnityEngine;

using SMath = System.Math;

namespace PFS.Math.AI.Wayfinding {
    /// <summary>
    /// Encapsulates the Bresenham algorithm, employed to reduce points in a list of 3D coordinates.
    /// <remarks>Based in part on a python implementation in assignment code from the Udacity Flying Car Nanodegree I took in 2018. <see>https://graduation.udacity.com/confirm/Q7CT96Q4</see></remarks>
    /// </summary>
    public class Bresenham3D : System.Object {
        /// <summary>
        /// Processes Bresenham's algorithm.
        /// </summary>
        /// <param name="points">A List<Vector3> of 3D coordinates.</param>
        public void Process(ref List<Vector3> points, CartesianDataGrid2D<int> weightedGrid) {
            List<Vector3> bresPath = new List<Vector3>();
            List<Vector3> prunedPath = new List<Vector3>();
            List<Vector3> pathSlice = new List<Vector3>();
            prunedPath.Add(points[0]);

            int i = 1;

            while (i < points.Count - 2) {
                pathSlice = points.GetRange(i + 1, points.Count - (i + 1));

                foreach (Vector3 p1 in pathSlice) {
                    bresPath = GetBresenhamPath(points[i], p1);

                    int j = 0;

                    while (j < bresPath.Count - 1) {
                        Vector3 p2 = bresPath[j];
                        Vector3 p2Previous = ((j - 1) > 0) ? bresPath[j - 1] : Vector3.zero;

                        bool isObstacle = (weightedGrid.Data[(int)p2.x, (int)p2.z] == Int16.MaxValue);
                        bool isObstaclePrevious = (p2Previous != Vector3.zero) ? (weightedGrid.Data[(int)p2Previous.x, (int)p2Previous.z] == Int16.MaxValue) : false;

                        if (isObstacle) {
                            if (p2Previous != Vector3.zero && !prunedPath.Contains(p2Previous) && !isObstaclePrevious) prunedPath.Add(p2Previous);
                            
                            j++;
                            i += j;

                            break;
                        }

                        j++;
                    }
                }

                i++;
            }

            prunedPath.Add(points[points.Count - 1]);
            points = prunedPath;
        }

        /// <summary>
        /// Creates a Bresenham path from two points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>A Bresenham path.</returns>
        private List<Vector3> GetBresenhamPath(Vector3 point1, Vector3 point2, float deltaThreshold=float.MaxValue) {
            List<Vector3> bresPath = new List<Vector3>();
            bresPath.Add(point1);

            var dx = SMath.Abs(point2.x - point1.x);
            var dy = SMath.Abs(point2.y - point1.y);
            var dz = SMath.Abs(point2.z - point1.z);
            float p1;
            float p2;
            int xs, ys, zs = 0;

            if (dx > deltaThreshold || dy > deltaThreshold || dz > deltaThreshold) {
                return bresPath;
            }

            //Debug.Log("" + dx + "," + dy + "," + dz);

            xs = (point2.x > point1.x) ? 1 : -1;
            ys = (point2.y > point1.y) ? 1 : -1;
            zs = (point2.z > point1.z) ? 1 : -1;

            if (dx >= dy && dx >= dz) { // Driving axis is X-axis
                p1 = 2 * dy - dx;
                p2 = 2 * dz - dx;

                while (point1.x != point2.x) {
                    point1.x += xs;

                    if (p1 >= 0) {
                        point1.y += ys;
                        p1 -= 2 * dx;
                    }

                    if (p2 >= 0) {
                        point1.z += zs;
                        p2 -= 2 * dx;
                    }

                    p1 += 2 * dy;
                    p2 += 2 * dz;

                    bresPath.Add(new Vector3(point1.x, point1.y, point1.z));
                }
            } else if (dy >= dx && dy >= dz) { // Driving axis is Y-axis
                p1 = 2 * dx - dy;
                p2 = 2 * dz - dy;

                while (point1.y != point2.y) {
                    point1.y += ys;

                    if (p1 >= 0) {
                        point1.x += xs;
                        p1 -= 2 * dy;
                    }

                    if (p2 >= 0) {
                        point1.z += zs;
                        p2 -= 2 * dy;
                    }

                    p1 += 2 * dx;
                    p2 += 2 * dz;

                    bresPath.Add(new Vector3(point1.x, point1.y, point1.z));
                }
            } else { // Driving axis is Z-axis
                p1 = 2 * dy - dz;
                p2 = 2 * dx - dz;

                while (point1.z != point2.z) {
                    point1.z += zs;

                    if (p1 >= 0) {
                        point1.y += ys;
                        p1 -= 2 * dz;
                    }
                    
                    if (p2 >= 0) {
                        point1.x += xs;
                        p2 -= 2 * dz;
                    }

                    p1 += 2 * dy;
                    p2 += 2 * dx;

                    bresPath.Add(new Vector3(point1.x, point1.y, point1.z));
                }
            }

            return bresPath;
        }
    }
}
