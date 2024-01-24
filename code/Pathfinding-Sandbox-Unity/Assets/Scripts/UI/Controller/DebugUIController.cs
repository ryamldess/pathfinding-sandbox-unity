/** 
 * Copyright (c) 2023-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.MVC;
using dk.Singleton;
using dk.Tools.Debug;

using System.Collections;

using UnityEngine;

namespace SnS.UI.Debug {
	/// <summary>
	/// Debug user interface controller.
	/// </summary>
	public class DebugUIController : BaseController {
		#region Inspector properties
		
		#region MVC relationships

		public DebugConsole debugConsole = null;
		public FPSCounterView fpsCounterView = null;
		public ScreenSizeReadoutView screenSizeReadoutView = null;
		public DebugUIModel debugUIModel = null;
		public UIModel uiModel = null;

		#endregion
		
		#endregion
		
		#region Monobehaviours
		
		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			if (Directives.isRuntimeDebug) {
				_mvcID = GlobalMVCProperties.DEBUG_UI_CONTROLLER_ID;
	
				AddModel(debugUIModel, GlobalMVCProperties.DEBUG_UI_MODEL_ID);
				AddModel(uiModel, GlobalMVCProperties.UI_MODEL_ID);
				AddView(debugConsole, GlobalMVCProperties.DEBUG_CONSOLE_ID);
				AddView(fpsCounterView, GlobalMVCProperties.FPS_COUNTER_VIEW_ID);
				AddView(screenSizeReadoutView, GlobalMVCProperties.SCREEN_SIZE_READOUT_VIEW_ID);
			}
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected sealed override void Start() {
			base.Start();
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update() {
			if ((Input.acceleration.x > 2 || Input.acceleration.x < -2) || 
			    (Input.acceleration.y > 2 || Input.acceleration.y < -2)) {
				if (debugConsole.visualTarget.activeSelf) {
					debugConsole.Hide(false);
				} else {
					debugConsole.Show(false);
				}
			}
		}
		
		#endregion

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		protected override void Initialize() {
			StartCoroutine(DeferInitialization(0.5f));
		}

		/// <summary>
		/// Defers the initialization.
		/// </summary>
		/// <returns>The initialization.</returns>
		/// <param name="delay">Delay.</param>
		private IEnumerator DeferInitialization(float delay) {
			yield return new WaitForSeconds(delay);

			GlobalUISceneProperties uiProperties = Singleton.GetInstance<GlobalUISceneProperties>();
			DebugLogger logger = Singleton.GetInstance<DebugLogger>();
			
			if (Directives.isRuntimeDebug) {
				debugConsole.mvcID = GlobalMVCProperties.DEBUG_CONSOLE_ID;
				debugConsole.visualTarget = uiProperties.debugConsoleVisualTarget;

				if (logger != null) {
					logger.isDebug = true;
					logger.console = debugConsole;
				}

				InvokeRepeating("UpdateController", 0.0f, uiModel.updateInterval60FPS);
			} else {
				debugConsole.Hide(false);
				fpsCounterView.Hide(false);
				screenSizeReadoutView.Hide(false);
				
				if (debugConsole != null) GameObject.DestroyImmediate(debugConsole.gameObject, true);
				if (fpsCounterView != null) GameObject.DestroyImmediate(fpsCounterView.gameObject, true);
				if (screenSizeReadoutView != null) GameObject.DestroyImmediate(screenSizeReadoutView.gameObject, true);
				if (debugUIModel != null) GameObject.DestroyImmediate(debugUIModel.gameObject, true);
				
				if (IsInvoking("UpdateController")) CancelInvoke("UpdateController");
			}
		}

		/// <summary>
		/// Updates the controller.
		/// </summary>
		/// <returns>The controller.</returns>
		private void UpdateController() {
			if (!Directives.isRuntimeDebug) {
				CancelInvoke("UpdateController");
				return;
			}

			UpdateViews();
			UpdateKeys();
		}
		
		/// <summary>
		/// Updates the views.
		/// </summary>
		private void UpdateViews() {
			if (!Directives.isRuntimeDebug) return;

			debugConsole.UpdateView();
			fpsCounterView.UpdateView();
			screenSizeReadoutView.UpdateView();
		}

		/// <summary>
		/// Updates the keys.
		/// </summary>
		private void UpdateKeys() {
			//
		}
	}
}
