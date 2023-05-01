using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MikouPunBehavior : MonoBehaviourPunCallbacks
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

    public PunManager punManager {
        get {
            return PunManager.instance;
        }
        private set {}
    }
}
