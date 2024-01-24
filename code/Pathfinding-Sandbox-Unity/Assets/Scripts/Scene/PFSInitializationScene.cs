/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.IO;
using dk.Scene;
using dk.Singleton;
using dk.Tools.Debug;

using System;

using UnityEngine;

using SceneLoadParameters = dk.IO.SceneLoader.SceneLoadParameters;

namespace PFS.Scene {
    /// <summary>
    /// Script to start and manage the initialization scene.
    /// </summary>
    public class PFSInitializationScene : InitializationScene {
        [SerializeField]
        private DebugCommandRegistrar _debugCommandRegistrar = null;
        [SerializeField]
        private InputHandler2D _inputHandler2D = null;

        public DebugCommandRegistrar DebugCommandRegistrar { get => _debugCommandRegistrar; }

        #region Monobehaviours

        /// <summary>
        /// Starts this instance.
        /// </summary>
        protected sealed override void Start() {
            base.Start();
        }

        #endregion

        /// <summary>
        /// Initializes the debug commands.
        /// </summary>
        protected override void InitializeDebugCommands() {
            base.InitializeDebugCommands();
        }

        /// <summary>
        /// Initalizes input for the app.
        /// </summary>
        protected override void InitializeInput() {
            if (_inputHandler2D != null)
            {
                if (Directives.isRuntimeMobile || Directives.isRuntimeEditor)
                {
                    if (Directives.isRuntimeEditor) _inputHandler2D.RegisterInput(InputHandle.MouseLeftButton);

                    _inputHandler2D.RegisterInput(InputHandle.TouchSingle);
                    _inputHandler2D.RegisterRaycast2DMethod(Raycast2DMethod.EVENT_SYSTEM);
                    _inputHandler2D.debug = Directives.isRuntimeDebug;
                    _inputHandler2D.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Initializes local game data, if there is any.
        /// </summary>
        protected override void InitializeLocalGameData() {
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// Initializes and queues scene loading dependencies.
        /// </summary>
        protected override void InitializeSceneLoadData() {
            this._sceneLoadParameters.Add(new SceneLoadParameters("PFS.UI", typeof(PFSUIScene), "PFS.UI.Logic.Root", true, true));
            this._sceneLoadParameters.Add(new SceneLoadParameters("PFS.Main", typeof(PFSMainScene), "PFS.Main.Logic.Root", true, true));
        }

        /// <summary>
        /// Shows the splash menu, if there is one.
        /// </summary>
        protected override void ShowSplashMenu() {
            //throw new System.NotImplementedException();
        }
    }
}
