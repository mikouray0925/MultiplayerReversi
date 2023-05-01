using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{   
    public static SceneController instance {get; private set;}
    public GameObject globalManager;
    public List<GameObject> objNeedToKeep = new List<GameObject>();

    public bool isChangingScene {get; private set;}
    public float changineSceneProgress {get; private set;}

    private void Awake() {
        instance = this;
    }

    public void ChangeScene(string sceneName) {
        ChangeScene(sceneName, new List<GameObject>());
    }

    public void ChangeScene(string sceneName, List<GameObject> moreObjNeedToMove) {
        if (isChangingScene) return;
        StartCoroutine(ChangingSceneCoroutine(sceneName, moreObjNeedToMove));
    }
    IEnumerator ChangingSceneCoroutine(string sceneName, List<GameObject> moreObjNeedToMove, bool showLoadingScreen = true) {
        isChangingScene = true;

        Scene currentScene = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone) {
            changineSceneProgress = asyncLoad.progress;
            yield return null;
        }
        changineSceneProgress = 1f;

        Scene nextScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.MoveGameObjectToScene(globalManager, nextScene);
        foreach (GameObject obj in objNeedToKeep) {
            SceneManager.MoveGameObjectToScene(obj, nextScene);
        }
        foreach (GameObject obj in moreObjNeedToMove) {
            SceneManager.MoveGameObjectToScene(obj, nextScene);
        }

        SceneManager.UnloadSceneAsync(currentScene);
        isChangingScene = false;
    }
}
