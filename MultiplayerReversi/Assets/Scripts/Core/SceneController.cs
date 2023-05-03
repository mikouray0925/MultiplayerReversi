using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{   
    public static SceneController instance {get; private set;}
    public GameObject globalManager;
    public HashSet<GameObject> objNeedToKeep = new HashSet<GameObject>();

    public bool isChangingScene {get; private set;}
    public float changineSceneProgress {get; private set;}

    public delegate void Callback();
    private struct ChangingSceneData {
        public string toSceneName;
        public Callback onChangingFinish;
    }
    private Queue<ChangingSceneData> changingSceneQueue = new Queue<ChangingSceneData>();

    private void Awake() {
        instance = this;
    }

    private void Update() {
        if (changingSceneQueue.Count > 0 && !isChangingScene) {
            ChangingSceneData data = changingSceneQueue.Dequeue();
            ChangeScene(data.toSceneName, data.onChangingFinish);
        }
    }

    public void ChangeScene(string sceneName) {
        ChangeScene(sceneName, null);
    }

    public void ChangeScene(string sceneName, Callback onChangingFinish) {
        if (isChangingScene) {
            ChangingSceneData data = new ChangingSceneData();
            data.toSceneName = sceneName;
            data.onChangingFinish = onChangingFinish;
            changingSceneQueue.Enqueue(data);
            return;
        }
        StartCoroutine(ChangingSceneCoroutine(sceneName, onChangingFinish));
    }

    IEnumerator ChangingSceneCoroutine(string sceneName, Callback onChangingFinish) {
        isChangingScene = true;
        Debug.Log("Change to scene: " + sceneName);

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

        SceneManager.UnloadSceneAsync(currentScene);
        isChangingScene = false;

        if (onChangingFinish != null) onChangingFinish();
    }
}
