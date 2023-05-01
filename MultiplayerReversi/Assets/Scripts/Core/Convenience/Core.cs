using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Core
{
    public static GlobalManager globalManager {
        get {
            return GlobalManager.instance;
        }
        private set {}
    }

    public static SceneController sceneController {
        get {
            return SceneController.instance;
        }
        private set {}
    }

    public static AudioManager audioManager {
        get {
            return AudioManager.instance;
        }
        private set {}
    }
}
