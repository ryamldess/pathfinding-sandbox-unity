/** 
 * Copyright (c) 2016-2024 Steve Sedlmayr and Droidknot LLC.
 **/

#if UNITY_EDITOR
#define DEBUG
#endif

using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

/// <summary>
/// Directives.
/// </summary>
public static class Directives {
	public enum MajorOS {
		UNSUPPORTED, 
		WINDOWS, 
		MACOSX, 
		ANDROID, 
		IOS
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="Directives"/> is runtime debug.
	/// </summary>
	/// <value><c>true</c> if is runtime debug; otherwise, <c>false</c>.</value>
	public static bool isRuntimeDebug {
		get {
			#if DEBUG
			return true;
			#else
			return isRuntimeEditor;
			#endif
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="Directives"/> is runtime editor.
	/// </summary>
	/// <value><c>true</c> if is runtime editor; otherwise, <c>false</c>.</value>
	public static bool isRuntimeEditor {
		get {
			return Application.isEditor;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="Directives"/> is runtime mobile.
	/// </summary>
	/// <value><c>true</c> if is runtime mobile; otherwise, <c>false</c>.</value>
	public static bool isRuntimeMobile {
		get {
			return (Application.isMobilePlatform || 
			        SystemInfo.operatingSystem.StartsWith("Android") ||
			        SystemInfo.operatingSystem.StartsWith("iOS") ||
			        SystemInfo.operatingSystem.StartsWith("iPhone"));
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="Directives"/> is runtime web.
	/// </summary>
	/// <value><c>true</c> if is runtime web; otherwise, <c>false</c>.</value>
	public static bool isRuntimeWeb {
		get {
			return (isRuntimeWebGL);
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="Directives"/> is runtime web G.
	/// </summary>
	/// <value><c>true</c> if is runtime web G; otherwise, <c>false</c>.</value>
	public static bool isRuntimeWebGL {
		get {
			#if UNITY_WEBGL
				return true;
			#else
				return false;
			#endif
		}
	}

	/// <summary>
	/// Gets the major runtime OS.
	/// </summary>
	/// <value>The major runtime OS.</value>
	public static MajorOS majorRuntimeOS {
		get {
			string versionString = SystemInfo.operatingSystem;

			if (versionString.Contains("Windows")) {
				return MajorOS.WINDOWS;
			}
			else if (versionString.Contains("Mac OS X")) {
				return MajorOS.MACOSX;
			}
			else if (versionString.Contains("Android")) {
				return MajorOS.ANDROID;
			}
			else if (versionString.Contains("iOS") || versionString.Contains("iPhone")) {
				return MajorOS.IOS;
			}
			else { // Fail-over in case the above string values are unreliable
				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WP8 || UNITY_WP8_1
				return MajorOS.WINDOWS;
				#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				return MajorOS.MACOSX;
				#elif UNITY_ANDROID
				return MajorOS.ANDROID;
				#elif UNITY_IOS || UNITY_IPHONE
				return MajorOS.IOS;
				#else
				return MajorOS.UNSUPPORTED;
				#endif
			}
		}
	}

	/// <summary>
	/// Gets the data path.
	/// </summary>
	/// <value>The data path.</value>
	public static string dataPath {
		get {
			string path = "";
			
			#if (UNITY_IPHONE || UNITY_IOS) && !(UNITY_EDITOR || UNITY_EDITOR_OSX)
			
			if (majorIOSVersion >= 8) {
				path = Application.persistentDataPath;
			} else {
				path = Application.dataPath.Replace("/Data", ""); // Strip "/Data" from path
				path = path.Substring(0, path.LastIndexOf('/')); // Strip application name
				path = Path.Combine(path, "Documents");
			}
			
			#elif UNITY_ANDROID && !(UNITY_EDITOR || UNITY_EDITOR_OSX)
			path = Application.persistentDataPath;
			#else
			path = Application.dataPath;
			#endif
			
			return path;
		}
	}
	
	/// <summary>
	/// Gets the streaming assets path.
	/// </summary>
	/// <value>The streaming assets path.</value>
	public static string streamingAssetsPath {
		get {
			string path = "";
			
			#if (UNITY_IPHONE || UNITY_IOS) && !(UNITY_EDITOR || UNITY_EDITOR_OSX)
			path = Application.persistentDataPath;
			#elif UNITY_ANDROID && !(UNITY_EDITOR || UNITY_EDITOR_OSX)
			path = "jar:file://" + Application.dataPath + "!/assets/";
			#else
			path = Path.Combine(Application.dataPath, "StreamingAssets");
			#endif
			
			return path;
		}
	}
	
	#region iOS properties

	#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR && !UNITY_EDITOR_OSX

	public enum IOSDeviceType {
		UNKNOWN, 
		IPHONE, 
		IPHONE_PLUS, 
		IPAD, 
		IPAD_MINI, 
		IPAD_AIR, 
		IPOD, 
		IPOD_TOUCH
	}

	/// <summary>
	/// Is the type of the iOS device.
	/// </summary>
	/// <returns>The iOS device type.</returns>
	public static IOSDeviceType iOSDeviceType() {
		string device = UnityEngine.iOS.Device.generation.ToString().ToUpper();
		
		if (device.Contains("IPHONE")) {
			return IOSDeviceType.IPHONE;
		} 
		else if (device.Contains("IPHONE") && device.Contains("PLUS")) {
			return IOSDeviceType.IPHONE_PLUS;
		} 
		else if (device.Contains("IPAD")) {
			return IOSDeviceType.IPAD;
		}
		else if (device.Contains("IPAD") && device.Contains("MINI")) {
			return IOSDeviceType.IPAD_MINI;
		}
		else if (device.Contains("IPAD") && device.Contains("AIR")) {
			return IOSDeviceType.IPAD_AIR;
		}
		else if (device.Contains("IPOD")) {
			return IOSDeviceType.IPOD;
		}
		else if (device.Contains("IPOD") && device.Contains("TOUCH")) {
			return IOSDeviceType.IPOD_TOUCH;
		}

		return IOSDeviceType.UNKNOWN;
	}

	/// <summary>
	/// Gets the iOS device generation.
	/// </summary>
	/// <value>The iOS device generation.</value>
	public static UnityEngine.iOS.DeviceGeneration iOSDeviceGeneration {
		get {
			return UnityEngine.iOS.Device.generation;
		}
	}

	/// <summary>
	/// Gets the specific IOS device generation number.
	/// </summary>
	/// <value>The specific IOS device generation number.</value>
	public static int specificIOSDeviceGenerationNumber {
		get {
			int specificGenerationNumber = 0;
			string device = UnityEngine.iOS.Device.generation.ToString ();
			Match match = Regex.Match(device, @"\d\B");
			int.TryParse(match.Value, out specificGenerationNumber);

			return specificGenerationNumber;
		}
	}

	/// <summary>
	/// Gets the full IOS version.
	/// </summary>
	/// <value>The full IOS version.</value>
	public static float fullIOSVersion {
		get {
			float version = -1.0f;

			string versionString = UnityEngine.iOS.Device.systemVersion.ToString();
			float.TryParse(versionString.Substring(0, 1), out version);
			
			return version;
		}
	}

	/// <summary>
	/// Gets the major IOS version.
	/// </summary>
	/// <value>The major IOS version.</value>
	public static int majorIOSVersion {
		get {
			int osVersion = -1;
			string versionString = SystemInfo.operatingSystem.Replace("iPhone OS ", "");
			int.TryParse(versionString.Substring(0, 1), out osVersion);
			
			return osVersion;
		}
	}
	
	#endif

	#endregion
}
