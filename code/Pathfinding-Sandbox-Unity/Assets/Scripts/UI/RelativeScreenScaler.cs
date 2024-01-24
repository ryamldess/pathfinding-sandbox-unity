/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.OO;
using dk.Singleton;
using dk.Tools.Debug;

using UnityEngine;
using UnityEngine.UI;

namespace SnS.UI {
	/// <summary>
	/// Relative screen scaler.
	/// </summary>
	[RequireComponent(typeof(LayoutElement))]
	[RequireComponent(typeof(RectTransform))]
	public class RelativeScreenScaler : MonoBehaviour, IObserver {
		#region Inspector properties

		public ScaleMode scaleMode = ScaleMode.SCALING_PERCENTAGE;

		public float screenWidthBasis = 0.0f;
		public float screenHeightBasis = 0.0f;
		public float minimumScale = 0.0f;
		public float maximumScale = 1.0f;
		public float fixedWidthPercentage = 1.0f;
		public float fixedHeightPercentage = 1.0f;

		#region Position scaling

		public float preferredXPosition = 0.0f;
		public float preferredYPosition = 0.0f;
		public bool preferEditorPosition = false;
		public bool scaleXPosition = false;
		public bool scaleYPosition = false;

		#endregion

		#endregion

		public enum ScaleMode {
			SCALING_PERCENTAGE, 
			FIXED_PERCENTAGE
		}

		private ScreenSizeChangeNotifier _notifier = null;
		private LayoutElement _layout = null;
		private RectTransform _rTransform = null;

		/// <summary>
		/// Gets or sets the name of the observer.
		/// </summary>
		/// <value>The name of the observer.</value>
		public string observerName {
			get { return _observerName; }
			set { _observerName = value; }
		}

		private string _observerName = string.Empty;

		#region Monobehaviours

		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			minimumScale = Mathf.Clamp01(minimumScale);

			_layout = GetComponent<LayoutElement>();
			_observerName = this.name;
			_rTransform = GetComponent<RectTransform>();
			_notifier = Singleton.GetInstance<ScreenSizeChangeNotifier>();
			_notifier.AddChangeListener(this);

			if (preferEditorPosition) {
				preferredXPosition = _rTransform.anchoredPosition.x;
				preferredYPosition = _rTransform.anchoredPosition.y;
			}

			Rescale();
		}

		#endregion

		/// <summary>
		/// Rescale this instance.
		/// </summary>
		protected void Rescale() {
			switch (scaleMode) {
				case ScaleMode.FIXED_PERCENTAGE: RescaleToFixedPercentage(); return;
				case ScaleMode.SCALING_PERCENTAGE: RescaleToScalingPercentage(); return;
			}
		}

		/// <summary>
		/// Rescales to fixed percentage.
		/// </summary>
		[ToDo("Factor in size scaling of assets at (1)")]
		protected void RescaleToFixedPercentage() {
			float widthPercent = Mathf.Clamp01 (fixedWidthPercentage);
			float heightPercent = Mathf.Clamp01 (fixedHeightPercentage);

			_rTransform.sizeDelta = new Vector2 (Screen.width * widthPercent, Screen.height * heightPercent);

			// Optional positional scaling; does not work with stretch anchors

			// 1
			if (!(scaleXPosition || scaleYPosition)) return;

			float xPivotOffset = 0.0f;
			float yPivotOffset = 0.0f;
			float anchorXPos = 0.0f;
			float anchorYPos = 0.0f;
			float minAnchorX = (_rTransform.anchorMin.x < _rTransform.anchorMax.x) ? _rTransform.anchorMin.x : _rTransform.anchorMax.x;
			float minAnchorY = (_rTransform.anchorMin.y < _rTransform.anchorMax.y) ? _rTransform.anchorMin.y : _rTransform.anchorMax.y;
			float scaledX = _rTransform.anchoredPosition.x;
			float scaledY = _rTransform.anchoredPosition.y;

			if (scaleXPosition) {
				if (preferredXPosition <= 1.0f && preferredXPosition >= -1.0f) {
					anchorXPos = (Screen.width * minAnchorX);
					xPivotOffset = _rTransform.sizeDelta.x * _rTransform.pivot.x;
					scaledX = -anchorXPos + xPivotOffset + (Screen.width * preferredXPosition);
				} else if (preferredXPosition > 1.0f || preferredXPosition < -1.0f) {
					scaledX = preferredXPosition * widthPercent;
				}
			}

			if (scaleYPosition) {
				if (preferredYPosition <= 1.0f && preferredYPosition >= -1.0f) {
					anchorYPos = (Screen.height * minAnchorY);
					yPivotOffset = _rTransform.sizeDelta.y * _rTransform.pivot.y;
					scaledY = -anchorYPos + yPivotOffset + (Screen.height * preferredYPosition);
				} else if (preferredYPosition > 1.0f || preferredYPosition < -1.0f) {
					scaledY = preferredYPosition * heightPercent;
				}
			}

			_rTransform.anchoredPosition = new Vector2(scaledX, scaledY);
		}

		/// <summary>
		/// Rescales to scaling percentage.
		/// </summary>
		protected void RescaleToScalingPercentage() {
			float scale = 0.0f;
			float scaleWidthPercent = Mathf.Clamp(Screen.width / screenWidthBasis, minimumScale, maximumScale);
			float scaleHeightPercent = Mathf.Clamp(Screen.height / screenHeightBasis, minimumScale, maximumScale);
			Vector3 newScale = _rTransform.localScale;
			
			if (scaleWidthPercent * _layout.preferredWidth > Screen.width) {
				scale = scaleHeightPercent;
			} else if (scaleHeightPercent * _layout.preferredHeight > Screen.height) {
				scale = scaleWidthPercent;
			} else {
				scale = scaleWidthPercent;
			}
			
			newScale = new Vector3(scale, scale, scale);
			_rTransform.localScale = newScale;
		}

		/// <summary>
		/// Notify the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		public void Notify(string type="") {
			if (type == ScreenSizeChangeNotifier.SCREEN_SIZE_CHANGED) Rescale();
		}
	}
}
