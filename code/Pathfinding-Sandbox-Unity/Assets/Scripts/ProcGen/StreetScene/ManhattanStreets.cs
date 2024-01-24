/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using System;

using UnityEngine;

namespace PFS.ProcGen.StreetScene {
    /// <summary>
    /// A value object for metadata about generating an orthogonal grid of streets.
    /// </summary>
    [Serializable]
    public class ManhattanStreets : System.Object {
        [SerializeField]
        public int northSouthStreets = 1;
        [SerializeField]
        public int eastWestStreets = 1;
        [SerializeField]
        public int streetWidth = 1;
    }
}
