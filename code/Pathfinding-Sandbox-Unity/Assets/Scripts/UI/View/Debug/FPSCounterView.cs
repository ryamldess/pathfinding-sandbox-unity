/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.MVC;

using UnityEngine;
using UnityEngine.UI;

namespace SnS.UI.Debug {
	/// <summary>
	/// FPS counter view.
	/// </summary>
	public class FPSCounterView : UIView {
		private const float _captureInterval = 0.5f;
		private const string _display = "Ave. FPS ({0}s interval): {1}\nCurrent FPS: {2}";

		private int _frameCount = 0;
		private float _deltaTime = 0.0f;
		private float _averageFPS = 0.0f;

		#region Inspector properties

		public Text textField;

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
			_mvcID = GlobalMVCProperties.FPS_COUNTER_VIEW_ID;
			
			AddModel(uiModel, GlobalMVCProperties.DEBUG_UI_MODEL_ID, _mvcID);
			AddController(uiController, GlobalMVCProperties.DEBUG_UI_CONTROLLER_ID, _mvcID);
		}

		#endregion

		/// <summary>
		/// Updates the view.
		/// </summary>
		public override void UpdateView() {
			float deltaTime = Time.deltaTime;
			float currentFPS = 1 / deltaTime;

			_deltaTime += deltaTime;
			_frameCount++;

			if (_deltaTime > _captureInterval ) {
				_averageFPS = _frameCount / _deltaTime;
				_deltaTime = 0.0f;
				_frameCount = 0;
			}

			textField.text = string.Format(_display, _captureInterval, _averageFPS, currentFPS);
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		override protected void Reset() {
			textField.text = string.Empty;
		}
	}
}