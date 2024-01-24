/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using System;

using UnityEngine;

namespace PFS.ProcGen.StreetScene {
    /// <summary>
    /// A value object for a StreetSceneGenerator instance.
    /// </summary>
    [Serializable]
    public class StreetSceneGeneratorParameters : System.Object {
        [SerializeField]
        public string name = string.Empty;
        [SerializeField]
        public Vector3 localScale = default(Vector3);
        [SerializeField]
        public GameObject parent = default(GameObject);
        [SerializeField]
        public Material material = default(Material);
        [SerializeField]
        public ManhattanStreets manhattanStreets = new ManhattanStreets();
    }
}
