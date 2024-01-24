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

using System;
using System.Diagnostics;
using System.IO;

using UnityEditor;
using UnityEngine;

using DKSingleton = dk.Singleton.Singleton;

namespace dk.Tools.Debug {
	/// <summary>
	/// Task log writer.
	/// </summary>
	class TaskLogWriter : Editor {
		/// <summary>
		/// Updates the todos.
		/// </summary>
		[MenuItem("Task Log Writer/Update To-Dos")]
		static void UpdateTodos() {
			GameObject logRoot = new GameObject();
            TaskLogger taskLogger = logRoot.AddComponent<TaskLogger>();
			taskLogger.LogToPath(true);
			GameObject.DestroyImmediate(logRoot, true);

			TaskLogWindow window = DKSingleton.GetObjectInstance<TaskLogWindow>();

			if (window != null) window.Close();
		}

		/// <summary>
		/// Opens the log file.
		/// </summary>
		[MenuItem("Task Log Writer/Open Task Log")]
		static void OpenLogFile() {
            //DebugLogger.Log("TaskLogWriter->OpenLogFile");

            string configFilePath = Application.dataPath;
			configFilePath = Path.Combine(configFilePath, "Config/TaskLogger.json");
			string json = string.Empty;
			string logPath = string.Empty;

            //DebugLogger.Log("configFilePath: " + configFilePath);

			try {
				if (File.Exists(configFilePath)) {
					json = File.ReadAllText(configFilePath);
				}

                //DebugLogger.Log("json: " + json);

                TaskLoggerConfig config = JsonUtility.FromJson<TaskLoggerConfig>(json);
				logPath = config.logPath;
                ProcessStartInfo startInfo = null;

                //DebugLogger.Log("logPath: " + logPath);

                if (Application.platform.Equals(RuntimePlatform.WindowsEditor)) {
                    string notepad = "Notepad.exe";
                    startInfo = new ProcessStartInfo(notepad);
                    startInfo.Arguments = logPath;
                } else if (Application.platform.Equals(RuntimePlatform.OSXEditor)) {
                    string textEdit = "open -a TextEdit " + logPath;
                    startInfo = new ProcessStartInfo(textEdit);
                }

                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process process = Process.Start(startInfo);
                process.EnableRaisingEvents = true;
                process.Exited += delegate {
					//
				};
			} catch(Exception exception) {
                DebugLogger.LogError("An error occurred: " + exception.ToString());

				throw(exception);
			}
		}

		/// <summary>
		/// Displays the log in editor.
		/// </summary>
		[MenuItem ("Task Log Writer/Open Task Log Window")]
		public static void  DisplayLogInEditor() {
			TaskLogWindow window = DKSingleton.GetObjectInstance<TaskLogWindow>();

			if (window == null) EditorWindow.GetWindow<TaskLogWindow>();
		}

		/// <summary>
		/// Writes the todos to jira.
		/// </summary>
		[MenuItem("Task Log Writer/Generate Jira Tasks")]
		static void WriteTodosToJira() {
            DebugLogger.Log("This feature is not yet implemented.");
		}
	}
}
