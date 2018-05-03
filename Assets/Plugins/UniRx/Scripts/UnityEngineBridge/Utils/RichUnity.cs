using UnityEngine;

namespace UniRx.Utils {
  public static class RichUnity {
    public static bool IsAnyEditor() {
      return Application.platform == RuntimePlatform.OSXEditor ||
        Application.platform == RuntimePlatform.LinuxEditor ||
        Application.platform == RuntimePlatform.WindowsEditor;
    }
  }
}
