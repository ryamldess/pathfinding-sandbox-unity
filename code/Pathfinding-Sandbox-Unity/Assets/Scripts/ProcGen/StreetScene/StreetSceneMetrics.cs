/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using System;
using System.Collections.Generic;

using PFS.Math;

using UnityEngine;

namespace PFS.ProcGen.StreetScene {
    /// <summary>
    /// Encapsulates fields pertaining to data about a street scene utilized during procedural generation of a StreetSceneProduct.
    /// </summary>
    [Serializable]
    public class StreetSceneMetrics : System.Object {
        [SerializeField]
        public List<BlockData> blockMetadata = default(List<BlockData>);
        [SerializeField]
        public List<Vector3> intersectionMetadata = default(List<Vector3>);
        [SerializeField]
        public int navPlaneWidth = 0;
        [SerializeField]
        public int navPlaneDepth = 0;

        [SerializeField]
        public int[] eastWestZPositions = new int[0];
        [SerializeField]
        public int[] northSouthXPositions = new int[0];

        [SerializeField]
        public CartesianDataGrid2D<int> navGridWeights = new CartesianDataGrid2D<int>(0,0);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreetSceneMetrics() {}

        /// <summary>
        /// Explicit destruction method.
        /// </summary>
        public void Destroy() {
            foreach (BlockData data in blockMetadata) {
                data.Destroy();
            }

            blockMetadata.Clear();
            intersectionMetadata.Clear();
            blockMetadata = default(List<BlockData>);
            intersectionMetadata = default(List<Vector3>);
            navPlaneWidth = 0;
            navPlaneDepth = 0;
            eastWestZPositions = new int[0];
            northSouthXPositions = new int[0];
            navGridWeights = new CartesianDataGrid2D<int>(0, 0);
        }
    }

    /// <summary>
    /// Encapsulates Vector3's representing the 4 corners of a city block.
    /// </summary>
    [Serializable]
    public class BlockData : System.Object {
        [SerializeField]
        public Vector3 neCorner = default(Vector3);
        [SerializeField]
        public Vector3 nwCorner = default(Vector3);
        [SerializeField]
        public Vector3 seCorner = default(Vector3);
        [SerializeField]
        public Vector3 swCorner = default(Vector3);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BlockData() {}

        /// <summary>
        /// Explicit destruction method.
        /// </summary>
        public void Destroy() {
            neCorner = default(Vector3);
            nwCorner = default(Vector3);
            seCorner = default(Vector3);
            swCorner = default(Vector3);
        }
    }
}
