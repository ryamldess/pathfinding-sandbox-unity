/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.MVC;

using UnityEngine;

namespace SnS.UI.Debug {
	/// <summary>
	/// Debug user interface model.
	/// </summary>
	public class DebugUIModel : BaseModel {
		#region Inspector properties
		
		#region MVC relationships
		
		public FPSCounterView fpsCounterView = null;
		public ScreenSizeReadoutView screenSizeReadoutView = null;
		
		#endregion
		
		#endregion
		
		#region Monobehaviours
		
		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			_mvcID = GlobalMVCProperties.DEBUG_UI_MODEL_ID;
			
			AddView(fpsCounterView, GlobalMVCProperties.FPS_COUNTER_VIEW_ID);
			AddView(screenSizeReadoutView, GlobalMVCProperties.SCREEN_SIZE_READOUT_VIEW_ID);
		}
		
		#endregion

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		public override void Initialize() {}

		/// <summary>
		/// Performs any necessary operations after initialization.
		/// </summary>
		public override void PostInitialize() {
            throw new System.NotImplementedException();
        }
    }
}
