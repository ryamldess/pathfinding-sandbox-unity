/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;
using dk.Tools.Debug;

using PFS.ProcGen.StreetScene;
using PFS.Scene;

namespace PFS.Commands.Debug {
    /// <summary>
    /// Debug command to enable visual aids in this project.
    /// </summary>
    public class EnableVisualizationCommand : DebugCommand {
        private PFSMainScene _mainScene = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EnableVisualizationCommand() {}

        /// <summary>
        /// Constructor with main scene reference.
        /// </summary>
        /// <param name="mainScene">The main scene reference.</param>
        public EnableVisualizationCommand(PFSMainScene mainScene) {
            _mainScene = mainScene;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="arguments">Arguments.</param>
        public override void ExecuteCommand(string[] arguments) {
			base.ExecuteCommand(arguments);

            if (arguments.Length == 2) EnableVisualization();
        }

        /// <summary>
        /// Undoes the command.
        /// </summary>
        public override void UndoCommand() {
            UnityEngine.Debug.Log("This command has no undo function.");
        }

        /// <summary>
		/// Clone this instance.
		/// </summary>
		public override IDebugCommand Clone() {
            return this.MemberwiseClone() as DebugCommand;
        }

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <returns>The help.</returns>
        public override string GetHelp() {
            return "Usage: envis [path | blk | nav | spln | strt]";
		}

        /// <summary>
        /// Enable the specified visualization.
        /// </summary>
		protected void EnableVisualization() {
            StreetSceneGenerator streetGen = Singleton.GetInstance<StreetSceneGenerator>() as StreetSceneGenerator;

            if (arguments.Length == 2) {
                if (_mainScene != null) if (_arguments[1] == "path") _mainScene.DrawPathfindingAids = true;

                if (streetGen == null) return;

                if (_arguments[1] == "blk") streetGen.DrawBlockMetaData = true;
                if (_arguments[1] == "nav") streetGen.DrawNavGridData = true;
                if (_arguments[1] == "spln") streetGen.DrawStreetSplineVisuals = true;
                if (_arguments[1] == "strt") streetGen.DrawStreetPlanes = true;
            }
        }
	}
}
