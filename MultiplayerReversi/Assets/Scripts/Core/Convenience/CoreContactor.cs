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

    public void AddKeepingObj(GameObject obj) {
        SceneController.instance.objNeedToKeep.Add(obj);
    }

    public void RemoveKeepingObj(GameObject obj) {
        SceneController.instance.objNeedToKeep.Remove(obj);
    }
}
