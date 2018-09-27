#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UniRx.Async;
using UniRx.Async.Internal;
using UniRx.Async.Utils;

namespace Plugins.UniRx.Scripts.Async.Internal
{
    public static class TaskTracker
    {
        static int trackingId = 0;

        public const string EnableAutoReloadKey = "UniTaskTrackerWindow_EnableAutoReloadKey";
        public const string EnableTrackingKey = "UniTaskTrackerWindow_EnableTrackingKey";
        public const string EnableStackTraceKey = "UniTaskTrackerWindow_EnableStackTraceKey";

        public static class EditorEnableState
        {
            static bool enableAutoReload;
            public static bool EnableAutoReload
            {
                get { return enableAutoReload; }
                set
                {
                    enableAutoReload = value;
                    EditorReflect.SetBool(EnableAutoReloadKey, value);
                }
            }

            static bool enableTracking;
            public static bool EnableTracking
            {
                get { return enableTracking; }
                set
                {
                    enableTracking = value;
                    EditorReflect.SetBool(EnableTrackingKey, value);
                }
            }

            static bool enableStackTrace;
            public static bool EnableStackTrace
            {
                get { return enableStackTrace; }
                set
                {
                    enableStackTrace = value;
                    EditorReflect.SetBool(EnableStackTraceKey, value);
                }
            }
        }



        static List<KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>> listPool = new List<KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>>();

        static readonly WeakDictionary<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)> tracking = new WeakDictionary<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>();



        public static void TrackActiveTask(IAwaiter task, int skipFrame = 1)
        {
            if (RichUnity.IsAnyEditor())
            {
                dirty = true;
                if (!EditorEnableState.EnableTracking) return;
                var stackTrace = EditorEnableState.EnableStackTrace ? DiagnosticsExtensions.CleanupAsyncStackTrace(new StackTrace(skipFrame, true)) : "";
                tracking.TryAdd(task, (Interlocked.Increment(ref trackingId), DateTime.UtcNow, stackTrace));
            }
        }


        public static void TrackActiveTask(IAwaiter task, string stackTrace)
        {
            if (RichUnity.IsAnyEditor())
            {
                dirty = true;
                if (!EditorEnableState.EnableTracking) return;
                var success = tracking.TryAdd(task, (Interlocked.Increment(ref trackingId), DateTime.UtcNow, stackTrace));
            }
        }

        public static string CaptureStackTrace(int skipFrame) {
            if (RichUnity.IsAnyEditor()) {
                if (!EditorEnableState.EnableTracking) return "";
                var stackTrace = EditorEnableState.EnableStackTrace ? DiagnosticsExtensions.CleanupAsyncStackTrace(new StackTrace(skipFrame + 1, true)) : "";
                return stackTrace;
            }

            return null;
        }

        public static void RemoveTracking(IAwaiter task) {
            if (RichUnity.IsAnyEditor()) {
                dirty = true;
                if (!EditorEnableState.EnableTracking) return;
                var success = tracking.TryRemove(task);
            }
        }

        static bool dirty;

        public static bool CheckAndResetDirty()
        {
            var current = dirty;
            dirty = false;
            return current;
        }

        /// <summary>(trackingId, awaiterType, awaiterStatus, createdTime, stackTrace)</summary>
        public static void ForEachActiveTask(Action<int, string, AwaiterStatus, DateTime, string> action)
        {
            lock (listPool)
            {
                var count = tracking.ToList(ref listPool, clear: false);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        string typeName = null;
                        var keyType = listPool[i].Key.GetType();
                        if (keyType.IsNested)
                        {
                            typeName = keyType.DeclaringType.Name + "." + keyType.Name;
                        }
                        else
                        {
                            typeName = keyType.Name;
                        }

                        action(listPool[i].Value.trackingId, typeName, listPool[i].Key.Status, listPool[i].Value.addTime, listPool[i].Value.stackTrace);
                        listPool[i] = new KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>(null, (0, default(DateTime), null)); // clear
                    }
                }
                catch
                {
                    listPool.Clear();
                    throw;
                }
            }
        }
    }
}

#endif
