/* 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015, 2016 Steve Sedlmayr and Droidknot LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
**/

using dk.Singleton;

using System;
using System.IO;

using UnityEditor;
using UnityEngine;

using DKSingleton = dk.Singleton.Singleton;

namespace dk.Tools.Debug {
	/// <summary>
	/// Task log window.
	/// </summary>
	[ToDo("Refactor this to use the Singleton util once that class can handle objects that derive from System.Object.", true)]
	public class TaskLogWindow : EditorWindow, ISingleton {
		private string _logContents = string.Empty;
		private Vector2 _scrollPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="dk.Tools.Debug.TaskLogWindow"/> class.
		/// </summary>
		public TaskLogWindow() {
			this.SetObjectInstance();
		}

		#region Monobehaviours

		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake() {
			FillLog();
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		void OnGUI() {
			GUILayout.Label("Task Log", EditorStyles.boldLabel);
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, EditorStyles.textArea);
			GUILayout.Label(_logContents);
			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy() {
			DKSingleton.RemoveInstance<TaskLogWindow>();
		}

		#endregion

		/// <summary>
		/// Fills the log.
		/// </summary>
		private void FillLog() {
			string configFilePath = Application.dataPath;
			configFilePath = Path.Combine(configFilePath, "Config/TaskLogger.json");
			string json = string.Empty;
			string logPath = string.Empty;
			
			try {
				if (File.Exists(configFilePath)) {
					json = File.ReadAllText(configFilePath);
				}
				
				TaskLoggerConfig config = JsonUtility.FromJson<TaskLoggerConfig>(json);
				logPath = config.logPath;
				
				if (File.Exists(logPath)) {
					_logContents = File.ReadAllText(logPath);
				}
			} catch(Exception exception) {
				throw exception;
			}
		}
	}
}
