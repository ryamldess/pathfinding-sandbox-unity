/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Tools.Debug;

using System.Collections.Generic;

using UnityEngine;

using SMath = System.Math;

namespace PFS.Math.AI.Wayfinding {
    /// <summary>
    /// Encapsulates a colinearity algorithm, employed to reduce points in a list of 3D coordinates.
    /// <remarks>Based in part on a python implementation in assignment code from the Udacity Flying Car Nanodegree I took in 2018. <see>https://graduation.udacity.com/confirm/Q7CT96Q4</see></remarks>
    /// </summary>
    public class Collinearity3D : System.Object {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Collinearity3D() {}

        /// <summary>
        /// Processes the colinearity algorithm.
        /// </summary>
        /// <param name="points">A List<Vector3> of 3D coordinates.</param>
        public void Process(ref List<Vector3> points) {
            List<Vector3> prunedPath = points;
            int i = 0;

            while (i < prunedPath.Count - 2) {
                Vector3 p1 = prunedPath[i];
                Vector3 p2 = prunedPath[i + 1];
                Vector3 p3 = prunedPath[i + 2];

                /* If the 3 points are in a line remove the 2nd point.
                   The 3rd point now becomes and 2nd point and the check is 
                   re-done with a new third point on the next iteration. */
                if (CollinearityCheck(p1, p2, p3, 1)) { // Setting epsilon pretty low
                    /* We can mutate prunedPath freely because the length
                       of the list is checked on every iteration.*/
                    prunedPath.Remove(prunedPath[i + 1]);
                } else {
                    i++;
                }
            }

            points = prunedPath;
        }

        /// <summary>
        /// Checks for collinearity between 3 points.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <param name="p3">Point 3.</param>
        /// <param name="epsilon">A threshold for comparison.</param>
        /// <returns>Whether the points are collinear.</returns>
        public bool CollinearityCheck(Vector3 p1, Vector3 p2, Vector3 p3, double epsilon=1e-6) {
            bool collinear = false;
            
            // Add points as rows in a matrix
            Matrix3x3 matrix = new Matrix3x3(p1, p2, p3);

            // Calculate the determinant of the matrix. 
            float determinant = matrix.Determinant();

            //Set collinear to true if the determinant is less than epsilon
            if (determinant < epsilon) collinear = true;

            return collinear;
        }
    }

    /// <summary>
    /// Encapsulates methods for a 3x3 Matrix so we can derive the determinant of such, as Unity only has Matrix2x2 and Matrix4x4.
    /// </summary>
    internal class Matrix3x3 {
        private List<Vector3> _rows = default(List<Vector3>); 

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Matrix3x3() {}

        /// <summary>
        /// Constructs a Matrix3x3 from 3 Vector3 instances.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="r3"></param>
        public Matrix3x3(Vector3 r1, Vector3 r2, Vector3 r3) {
            _rows = new List<Vector3>(new Vector3[] { r1, r2, r3 });
        }

        /// <summary>
        /// Gets the determinant of this Matrix3x3.
        /// </summary>
        /// <returns>The determinant of this Matrix3x3.</returns>
        public float Determinant() {
            var m11 = _rows[0].x;
            var m12 = _rows[0].y;
            var m13 = _rows[0].z;
            var m21 = _rows[1].x;
            var m22 = _rows[1].y;
            var m23 = _rows[1].z;
            var m31 = _rows[2].x;
            var m32 = _rows[2].y;
            var m33 = _rows[2].z;

            float d = SMath.Abs(
                (m11 * ((m22 * m33) - (m23 * m32))) +
                (m12 * ((m23 * m31) - (m21 * m33))) +
                (m13 * ((m21 * m32) - (m22 * m31)))
            );

            //Debug.Log("DETERMINANT: " + d);

            return d;
        }
    }
}
