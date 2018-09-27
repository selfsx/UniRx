using System;
using System.Reflection;

namespace UniRx.Async.Utils {
  public static class EditorReflect {
    private static Assembly editorAssembly;
    private static Type editorType;
    private static Type editorPrefsType;

    public static bool GetBool(string key) {
      var method = GetEditorPrefsTypeRef().GetMethod("GetBool", new []{ typeof(string) });
      var result = method.Invoke(null, new object[] { key });

      return (bool) result;
    }

    public static void SetBool(string key, bool value) {
      var method = GetEditorPrefsTypeRef().GetMethod("SetBool");
      method.Invoke(null, new object[] { key, value });
    }

    public static void OnPlayModeStateChanged(Action callbackAction) {
      var t = GetAssemlyRef().GetType("UnityEditor.PlayModeStateChange");
      var eMethod = typeof(EditorReflect).GetMethod("OnPlayModeStateChangedImpl");
      var gMethod = eMethod.MakeGenericMethod(t);

      gMethod.Invoke(null, new object[]{ callbackAction });
    }

    public static void OnPlayModeStateChangedImpl<T>(Action callbackAction) {
      var eventType = GetEditorApplicationTypeRef().GetEvent("playModeStateChanged");
      eventType.AddEventHandler(null, CreateCallback<T>(callbackAction));
    }

    public static bool IsPlayingOrWillChangePlaymode() {
      var propertyInfo = GetEditorApplicationTypeRef().GetProperty("isPlayingOrWillChangePlaymode");
      return (bool) propertyInfo.GetValue(null, null);
    }

    public static bool IsPlaying() {
      var propertyInfo = GetEditorApplicationTypeRef().GetProperty("isPlaying");
      return (bool) propertyInfo.GetValue(null, null);
    }

    private static Assembly GetAssemlyRef() {
      return editorAssembly ?? (editorAssembly = Assembly.Load("UnityEditor"));
    }

    private static Type GetEditorApplicationTypeRef() {
      return editorType ?? (editorType = GetAssemlyRef().GetType("UnityEditor.EditorApplication"));
    }

    private static Type GetEditorPrefsTypeRef() {
      return editorPrefsType ?? (editorPrefsType = GetAssemlyRef().GetType("UnityEditor.EditorPrefs"));
    }

    private static Action<T> CreateCallback<T>(Action callbackAction) {
      return t => callbackAction();
    }
  }
}
