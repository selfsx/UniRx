using UnityEngine;
using UniRx.Utils;

namespace UniRx {

  // [InitializeOnLoad]
  public class ScenePlaybackDetector {
    private static bool _isPlaying = false;

    private static bool AboutToStartScene {
      get {
        return EditorReflect.GetBool("AboutToStartScene");
      }
      set {
        EditorReflect.SetBool("AboutToStartScene", value);
      }
    }

    public static bool IsPlaying {
      get {
        return _isPlaying;
      }
      set {
        if (_isPlaying != value) {
          _isPlaying = value;
        }
      }
    }

    // This callback is notified after scripts have been reloaded.
    // [DidReloadScripts]
    public static void OnDidReloadScripts() {
      // Filter DidReloadScripts callbacks to the moment where playmodeState transitions into isPlaying.
      if (AboutToStartScene) {
        IsPlaying = true;
      }
    }

    // InitializeOnLoad ensures that this constructor is called when the Unity Editor is started.
    static ScenePlaybackDetector() {
      EditorReflect.OnPlayModeStateChanged(() => {
          // Before scene start:          isPlayingOrWillChangePlaymode = false;  isPlaying = false
          // Pressed Playback button:     isPlayingOrWillChangePlaymode = true;   isPlaying = false
          // Playing:                     isPlayingOrWillChangePlaymode = false;  isPlaying = true
          // Pressed stop button:         isPlayingOrWillChangePlaymode = true;   isPlaying = true
          if (EditorReflect.IsPlayingOrWillChangePlaymode() && !EditorReflect.IsPlaying()) 
          {
            AboutToStartScene = true;
          } else {
            AboutToStartScene = false;
          }
  
          // Detect when playback is stopped.
          if (!EditorReflect.IsPlaying()) 
          {
            IsPlaying = false;
          }
        });
    }
  }
}
