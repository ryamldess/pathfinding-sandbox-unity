/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;

using SnS.UI.Debug;

using UnityEngine;

namespace PFS.Scene {
    /// <summary>
    /// Main script for the UI scene.
    /// </summary>
    public class PFSUIScene : MonoSingleton {
        #region Inspector Properties

        public DebugUIController DebugUIController { get => _debugUIController; set => _debugUIController = value; }

        [SerializeField]
        protected DebugUIController _debugUIController = null;

        #endregion

        /// <summary>
        /// Start this instance.
        /// </summary>
        protected sealed override void Start() {
            base.Start();
        }
    }
}
