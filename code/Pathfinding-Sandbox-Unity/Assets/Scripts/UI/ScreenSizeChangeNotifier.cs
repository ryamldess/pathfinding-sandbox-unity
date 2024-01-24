/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.OO;
using dk.Singleton;

using UnityEngine;

namespace SnS.UI {
	/// <summary>
	/// Screen size change notifier.
	/// </summary>
	public class ScreenSizeChangeNotifier : MonoSingleton {
		public const string SCREEN_SIZE_CHANGED = "SCREEN_SIZE_CHANGED";

		private Observable _notifier = null;
		private float _screenWidth = 0.0f;
		private float _screenHeight = 0.0f;

		#region Monobehaviour

		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			//GameObject.DontDestroyOnLoad(this);

			_notifier = gameObject.AddComponent<Observable>();
			_screenWidth = Screen.width;
			_screenHeight = Screen.height;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected sealed override void Start() {
			base.Start();

			InvokeRepeating("DetectScreenSizeChange", 0.0f, 0.5f);
		}

		#endregion

		/// <summary>
		/// Adds the change listener.
		/// </summary>
		/// <param name="listener">Listener.</param>
		public void AddChangeListener(IObserver listener) {
			_notifier.AddObserver(listener);
		}

		/// <summary>
		/// Detects the screen size change.
		/// </summary>
		protected void DetectScreenSizeChange() {
			if (Screen.width != _screenWidth || 
			    Screen.height != _screenHeight) {
				_screenWidth = Screen.width;
				_screenHeight = Screen.height;
				_notifier.NotifyObservers(SCREEN_SIZE_CHANGED);
			}
		}
	}
}
