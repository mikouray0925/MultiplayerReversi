using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MikouBehavior : MonoBehaviour
{
    public GlobalManager globalManager {
        get {
            return GlobalManager.instance;
        }
        private set {}
    }

    public SceneController sceneController {
        get {
            return SceneController.instance;
        }
        private set {}
    }

    public AudioManager audioController {
        get {
            return AudioManager.instance;
        }
        private set {}
    }
}
