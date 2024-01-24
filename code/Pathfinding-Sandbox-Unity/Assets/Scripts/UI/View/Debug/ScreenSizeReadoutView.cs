/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.MVC;
using dk.Tools.Debug;

using System;

using UnityEngine;
using UnityEngine.UI;

namespace SnS.UI.Debug {
	/// <summary>
	/// Screen size readout view.
	/// </summary>
	public class ScreenSizeReadoutView : UIView {
		private const string _display = "Screen Resolution:\n{0} x {1}";

		#region Inspector properties
		
		public Text readout = null;
		
		#region MVC relationships
		
		public DebugUIController uiController = null;
		public DebugUIModel uiModel = null;
		
		#endregion
		
		#endregion
		
		#region Monobehaviours
		
		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			_mvcID = GlobalMVCProperties.SCREEN_SIZE_READOUT_VIEW_ID;
			
			AddModel(uiModel, GlobalMVCProperties.DEBUG_UI_MODEL_ID, _mvcID);
			AddController(uiController, GlobalMVCProperties.DEBUG_UI_CONTROLLER_ID, _mvcID);
		}

		#endregion
		
		/// <summary>
		/// Updates the view.
		/// </summary>
		public override void UpdateView() {
			readout.text = string.Format(_display, Screen.width, Screen.height);
		}
		
		/// <summary>
		/// Reset this instance.
		/// </summary>
		override protected void Reset() {
			//
		}
	}
}
