using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreContactor : MonoBehaviour
{
    public void ChangeScene(string sceneName) {
        SceneController.instance.ChangeScene(sceneName);
    }

    public void PlayUserInterfaceSFX(AudioClip clip) {
        AudioManager.instance.PlayUserInterfaceSFX(clip);
    }
}
