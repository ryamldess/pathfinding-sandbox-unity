/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.Extension;
using dk.MVC;
using dk.Tools.Debug;

using UnityEngine;

namespace SnS.UI {
	/// <summary>
	/// User interface model.
	/// </summary>
	public class UIModel : BaseModel {
		#region Inspector properties
		
		#region MVC relationships

		//

		#endregion
		
		#endregion

		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval1024FPS { get { return GlobalProperties.UPDATE_INTERVAL_1024_FPS; } }

		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval240FPS { get { return GlobalProperties.UPDATE_INTERVAL_240_FPS; } }
		
		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval120FPS { get { return GlobalProperties.UPDATE_INTERVAL_120_FPS; } }
		
		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval60FPS { get { return GlobalProperties.UPDATE_INTERVAL_60_FPS; } }

		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval30FPS { get { return GlobalProperties.UPDATE_INTERVAL_30_FPS; } }

		[HideInInspector]
		[ToDo("Move this to another model? Pertains to other things besides UI.")]
		public float updateInterval15FPS { get { return GlobalProperties.UPDATE_INTERVAL_15_FPS; } }

		/// <summary>
		/// Gets or sets the state of the user interface controller state.
		/// </summary>
		/// <value>The state of the user interface controller state.</value>
		public int uiControllerState {
			get {
				return _uiControllerState;
			}
			set {
				_uiControllerState = value;
			}
		}

		private int _uiControllerState = 0x0;

		#region Monobehaviours
		
		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			_mvcID = GlobalMVCProperties.UI_MODEL_ID;

			//AddView(registerLoginView, GlobalMVCProperties.REGISTER_LOGIN_VIEW_ID);
		}
		
		#endregion

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		public override void Initialize() {
			//
		}

		/// <summary>
		/// Performs any necessary operations after initialization.
		/// </summary>
        public override void PostInitialize() {
            throw new System.NotImplementedException();
        }

        /*/// <summary>
        /// Sets the view state flag.
        /// </summary>
        /// <param name="targetFlag">Target flag.</param>
        /// <param name="on">On.</param>
        public void SetViewStateFlag(UIControllerStateID targetFlag, int on) {
			targetFlag.SetFlagSimple<UIControllerStateID>(on >= 1, ref _uiControllerState);
		}*/
	}
}
