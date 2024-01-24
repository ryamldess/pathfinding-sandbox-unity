/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using System;

using System.Collections.Generic;

using UnityEngine;

namespace PFS.ProcGen.StreetScene {
    /// <summary>
    /// An object encapsulating the geometry and any other artifacts generated by StreetSceneGenerator.
    /// </summary>
    public class StreetSceneProduct : MonoBehaviour {
        public GameObject Buildings { get => _buildings; set => _buildings = value; }

        protected GameObject _buildings = default(GameObject);

        public StreetSceneMetrics SceneMetrics { get => _sceneMetrics; set => _sceneMetrics = value; }

        protected StreetSceneMetrics _sceneMetrics = new StreetSceneMetrics();

        public GameObject StreetPlanes { get => _streetPlanes; set => _streetPlanes = value; }

        protected GameObject _streetPlanes = default(GameObject);

        public List<GameObject> VisualAids { get => _visualAids; set => _visualAids = value; }

        protected List<GameObject> _visualAids = new List<GameObject>();

        /// <summary>
        /// Cleans up this instance's references.
        /// </summary>
        private void OnDestroy() {
            try {
                GameObject.DestroyImmediate(_buildings);
                _sceneMetrics.Destroy();
                GameObject.DestroyImmediate(_streetPlanes);
                
                foreach (GameObject go in _visualAids) {
                    GameObject.DestroyImmediate(go);
                }

                _visualAids.Clear();
            } catch (Exception e) {
                ToString();
            }
        }
    }
}
