/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;
using dk.Tools.Debug;

using PFS.ProcGen.StreetScene;
using PFS.Scene;

using System.Collections.Generic;

using UnityEngine;

namespace PFS.Commands.Debug {
    /// <summary>
    /// Debug command to trigger arbitrary street scene generation.
    /// </summary>
    public class GenerateStreetSceneCommand : DebugCommand {
        private const int _DEFAULT_NS_MIN_IN = 1;
        private const int _DEFAULT_NS_MAX_EX = 3;
        private const int _DEFAULT_EW_MIN_IN = 2;
        private const int _DEFAULT_EW_MAX_EX = 4;

        private PFSMainScene _mainScene = null;
        private List<StreetSceneProduct> _streetScenes = new List<StreetSceneProduct>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GenerateStreetSceneCommand() {}

        /// <summary>
        /// Constructor with main scene reference.
        /// </summary>
        /// <param name="mainScene">The main scene reference.</param>
        public GenerateStreetSceneCommand(PFSMainScene mainScene) {
            _mainScene = mainScene;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="arguments">Arguments.</param>
        public override void ExecuteCommand(string[] arguments) {
			base.ExecuteCommand(arguments);

            int nsStreetsMinInclusive = _DEFAULT_NS_MIN_IN;
            int nsStreetsMaxExclusive = _DEFAULT_NS_MAX_EX;
            int ewStreetsMinInclusive = _DEFAULT_EW_MIN_IN;
            int ewStreetsMaxExclusive = _DEFAULT_EW_MAX_EX;
            int streetDimension = 1;

            if (arguments.Length >= 2) int.TryParse(arguments[1], out nsStreetsMinInclusive);
            if (arguments.Length >= 3 && arguments[1] != "-X") int.TryParse(arguments[2], out nsStreetsMaxExclusive);
            if (arguments.Length >= 4) int.TryParse(arguments[3], out ewStreetsMinInclusive);
            if (arguments.Length == 5) int.TryParse(arguments[4], out ewStreetsMaxExclusive);

            if (arguments.Length == 1) GenerateStreetScene();
            if (arguments.Length == 2) GenerateStreetScene(nsStreetsMinInclusive);

            if (arguments.Length == 3) {
                if (arguments[1] == "-X") {
                    int.TryParse(arguments[2], out streetDimension);
                    GenerateStreetScene(streetDimension, streetDimension + 1, streetDimension, streetDimension + 1);
                } else {
                    GenerateStreetScene(nsStreetsMinInclusive, nsStreetsMaxExclusive);
                }
            }
            
            if (arguments.Length == 4) GenerateStreetScene(nsStreetsMinInclusive, nsStreetsMaxExclusive, ewStreetsMinInclusive);
            if (arguments.Length == 5) GenerateStreetScene(nsStreetsMinInclusive, nsStreetsMaxExclusive, ewStreetsMinInclusive, ewStreetsMaxExclusive);
        }

        /// <summary>
        /// Undoes the command.
        /// </summary>
        public override void UndoCommand() {
            if (_streetScenes.Count > 0) {
                GameObject.DestroyImmediate(_streetScenes[_streetScenes.Count - 1].gameObject);
                _streetScenes.RemoveAt(_streetScenes.Count - 1);

                _mainScene.StreetScene = (_streetScenes.Count >= 1) ? _streetScenes[_streetScenes.Count - 1] : null;
                _mainScene.UpdateAfterStreetSceneGen();
            }
		}

        /// <summary>
		/// Clone this instance.
		/// </summary>
		public override IDebugCommand Clone() {
            return this.MemberwiseClone() as DebugCommand;
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>The help.</returns>
        public override string GetHelp() {
            return "Usage: genst | genst -X {street dimension} | genst [n-s streets min inclusive][n-s streets max exclusive][e-w streets min inclusive][e-w streets max exclusive]";
		}

        /// <summary>
        /// Generates a street scene.
        /// </summary>
		protected void GenerateStreetScene(
            int nsStreetsMinInclusive=_DEFAULT_NS_MIN_IN, 
            int nsStreetsMaxExclusive=_DEFAULT_NS_MAX_EX, 
            int ewStreetsMinInclusive=_DEFAULT_EW_MIN_IN, 
            int ewStreetsMaxExclusive=_DEFAULT_EW_MAX_EX
        ) {
            if (_mainScene == null) return;
            
            if (nsStreetsMinInclusive >= nsStreetsMaxExclusive ||
                ewStreetsMinInclusive >= ewStreetsMaxExclusive)
            {
                DebugLogger.LogError("A minimum inclusive dimension is larger than a maximum exclusive dimension.");
                return;
            }

            if (
                nsStreetsMinInclusive == _DEFAULT_NS_MIN_IN && 
                nsStreetsMaxExclusive == _DEFAULT_NS_MAX_EX && 
                ewStreetsMinInclusive == _DEFAULT_EW_MIN_IN && 
                ewStreetsMaxExclusive == _DEFAULT_EW_MAX_EX
            ) {
                float scaledXDimensionModifier = _mainScene.NavPlane.transform.localScale.x / 10.0f;
                float scaledZDimensionModifier = _mainScene.NavPlane.transform.localScale.z / 10.0f;

                nsStreetsMinInclusive *= (int)scaledXDimensionModifier;
                nsStreetsMaxExclusive *= (int)scaledXDimensionModifier;
                ewStreetsMinInclusive *= (int)scaledZDimensionModifier;
                ewStreetsMaxExclusive *= (int)scaledZDimensionModifier;
            }

            StreetSceneGenerator streetGen = Singleton.GetInstance<StreetSceneGenerator>() as StreetSceneGenerator;
            StreetSceneGeneratorParameters streetGenParams = new StreetSceneGeneratorParameters();

            streetGenParams.name = "StreetScene_" + (_streetScenes.Count + 1);
            streetGenParams.localScale = _mainScene.NavPlane.transform.localScale;
            streetGenParams.parent = _mainScene.SceneRoot;

            ManhattanStreets manhattanStreets = new ManhattanStreets();
            manhattanStreets.northSouthStreets = UnityEngine.Random.Range(nsStreetsMinInclusive, nsStreetsMaxExclusive);
            manhattanStreets.eastWestStreets = UnityEngine.Random.Range(ewStreetsMinInclusive, ewStreetsMaxExclusive);
            manhattanStreets.streetWidth = 8;

            streetGenParams.manhattanStreets = manhattanStreets;

            List<StreetSceneProduct> streetScenes = streetGen.Generate(streetGenParams);

            _streetScenes.AddRange(streetScenes);

            _mainScene.StreetScene = _streetScenes[0];
            _mainScene.UpdateAfterStreetSceneGen();
        }
	}
}
