/** 
 * Copyright (c) 2023-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using dk.Singleton;

using SnS.UI;

using UnityEngine;

namespace SnS.UI {
	/// <summary>
	/// Global user interface scene properties.
	/// </summary>
	public class GlobalUISceneProperties : MonoSingleton {
		#region Inspector properties

		//

		#region HUD Controls

		//

		#region Mobile controls

		//

		#endregion

		#region Debug

		[SerializeField]
		internal GameObject debugConsoleVisualTarget = null;

		#endregion

		#endregion

		//

		#endregion

		#region Monobehaviours
		
		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			GameObject.DontDestroyOnLoad(this);
		}
		
		#endregion
	}
}
