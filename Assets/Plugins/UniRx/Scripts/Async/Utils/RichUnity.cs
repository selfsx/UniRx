using UnityEngine;

namespace UniRx.Async.Utils {
  public static class RichUnity {
    public static bool IsAnyEditor() {
      return Application.platform == RuntimePlatform.OSXEditor ||
        Application.platform == RuntimePlatform.LinuxEditor ||
        Application.platform == RuntimePlatform.WindowsEditor;
    }
  }
}
