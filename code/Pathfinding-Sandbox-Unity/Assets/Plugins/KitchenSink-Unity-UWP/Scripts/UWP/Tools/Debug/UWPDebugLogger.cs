using dk.Singleton;
using dk.Tools.Debug;
using dk.UWP.Interfaces;

using DKSingleton = dk.Singleton.Singleton;

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

#if NETFX_CORE

using System.Diagnostics;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

#endif

namespace dk.UWP.Tools.Debug {
    /// <summary>
    /// UWP implementation of DebugLogger.
    /// </summary>
    public class UWPDebugLogger : IUWPDebugLoggerWrapper {
        /// <summary>
        /// Writes log messages to an external log file.
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLogFile(string message) {
            #if NETFX_CORE

            DebugLogger instance = DKSingleton.GetInstance<DebugLogger>();

            Task thread0 = null;
            Task thread1 = null;
            Task thread2 = null;

            try {
                thread0 = new Task(async () =>
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var item = await folder.TryGetItemAsync("output.log");
                    
                    if (item == null) {
                        var file = await folder.CreateFileAsync("output.log", CreationCollisionOption.ReplaceExisting);
                    }
                });
                    
                thread0.Start();
                thread0.Wait();

                if (thread0 != null && thread0.IsCompleted) {
                    thread1 = new Task(async () =>
                    {
                        var folder = ApplicationData.Current.LocalFolder;    
                        var logFile = await folder.GetFileAsync("output.log");
                        
                        List<string> lines = new List<string>();
                        lines.Add("--------------------------------------------------------------------------------");
                        lines.Add(message);
                    
                        await FileIO.AppendLinesAsync(logFile, lines);
                    });
                
                    thread1.Start();
                    thread1.Wait();
                }

                // For Testing
                /*if (thread1 != null && thread1.IsCompleted) {
                    UnityEngine.Debug.Log("thread1 complete.");

                    try {
                        thread2 = new Task(async () =>
                        {
                            var folder = ApplicationData.Current.LocalFolder;    
                            var logFile = await folder.GetFileAsync("output.log");

                            string logText = await FileIO.ReadTextAsync(logFile);

                            UnityEngine.Debug.Log("LOG TEXT: " + logText);
                        });
                    
                        thread2.Start();
                        thread2.Wait();
                    } catch (Exception e) {
                        UnityEngine.Debug.Log("ERROR: " + e.Message + "\n" + e.StackTrace);
                    }
                }

                if (thread2 != null && thread2.IsCompleted) {
                     UnityEngine.Debug.Log("thread2 complete.");
                }*/
            } catch (Exception e) {
                UnityEngine.Debug.Log("ERROR: " + e.Message + "\n" + e.StackTrace);
            }

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }
    }
}
