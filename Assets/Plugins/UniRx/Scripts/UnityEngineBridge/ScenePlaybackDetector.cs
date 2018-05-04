using UniRx.Utils;

namespace UniRx {
  public static class ScenePlaybackDetector {
    public static bool IsPlaying { get; private set; }
    
    // Place \w [InitializeOnLoad] ensures that this constructor is
    // called when the Unity Editor is started.
    public static void InitializeOnLoad() {
      EditorReflect.OnPlayModeStateChanged(() => {
        // Before scene start:          isPlayingOrWillChangePlaymode = false;  isPlaying = false
        // Pressed Playback button:     isPlayingOrWillChangePlaymode = true;   isPlaying = false
        // Playing:                     isPlayingOrWillChangePlaymode = false;  isPlaying = true
        // Pressed stop button:         isPlayingOrWillChangePlaymode = true;   isPlaying = true
        if (EditorReflect.IsPlayingOrWillChangePlaymode() && !EditorReflect.IsPlaying()) {
          AboutToStartScene = true;
        } else {
          AboutToStartScene = false;
        }

        // Detect when playback is stopped.
        if (!EditorReflect.IsPlaying()) {
          IsPlaying = false;
        }
      });
    }

    // Place \w [DidReloadScripts] This callback is notified
    // after scripts have been reloaded.
    public static void DidReloadScripts() {
      // Filter DidReloadScripts callbacks to the moment where
      // playmodeState transitions into isPlaying.
      if (AboutToStartScene) {
        IsPlaying = true;
      }      
    }
    
    private static bool AboutToStartScene {
      get { return EditorReflect.GetBool("AboutToStartScene"); }
      set { EditorReflect.SetBool("AboutToStartScene", value); }
    }    
  }
}
