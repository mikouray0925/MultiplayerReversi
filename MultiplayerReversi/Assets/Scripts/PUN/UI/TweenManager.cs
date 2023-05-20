using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TweenManager : MonoBehaviour
{
    // <summary>
    // TweenManager is a singleton class that manages all the tweening in the game.
    // Every tweening should be public and setted as reference in the inspector.
    // </summary>
    [Header("Common")]
    public static TweenManager instance;
    public Image background;
    public GameObject returnButton;
    bool isPlayingEndAnimation = false;
    Rect screenSize;

    [Header("Debug")]

    public bool playVictory = false;
    public bool playDefeat = false;
    
    private void Awake() {
        if (instance == null) 
            instance = this;
        screenSize = Camera.main.pixelRect;
    }

    private void Update(){
        if (playVictory) {
            playVictory = false;
            PlayVictoryAnimation();
        }
        if (playDefeat) {
            playDefeat = false;
            PlayDefeatAnimation();
        }
    }

    public void TweenReset(){
        isPlayingEndAnimation = false;
    }

    [Header("Victory")]
    public GameObject victoryPanel;
    public void PlayVictoryAnimation() {
        if(isPlayingEndAnimation) return;
        isPlayingEndAnimation = true;

        victoryPanel.transform.position = new Vector3((screenSize.xMin + screenSize.xMax)/2 ,screenSize.yMax + 200,0);
        returnButton.transform.position = new Vector3((screenSize.xMin + screenSize.xMax)/2 ,screenSize.yMin - 100,0);

        LeanTween.alpha(background.rectTransform, 0.5f, 1f).setEaseInSine();
        victoryPanel.SetActive(true);
        LeanTween.moveY(victoryPanel, (screenSize.yMin + screenSize.yMax) * 0.6f, 2f).setEaseOutBounce();
        returnButton.SetActive(true);
        LeanTween.moveY(returnButton,(screenSize.yMin + screenSize.yMax) * 0.15f,1f).setEaseInSine().setDelay(2f);
    }

    [Header("Defeat")]
    public GameObject defeatPanel;
    public void PlayDefeatAnimation() {
        if(isPlayingEndAnimation) return;
        isPlayingEndAnimation = true;
        //reset position
        defeatPanel.transform.position = new Vector3((screenSize.xMin + screenSize.xMax)/2 ,screenSize.yMax + 200,0);
        returnButton.transform.position = new Vector3((screenSize.xMin + screenSize.xMax)/2 ,screenSize.yMin - 100,0);

        LeanTween.alpha(background.rectTransform, 0.5f, 1f).setEaseInSine();
        defeatPanel.SetActive(true);
        LeanTween.moveY(defeatPanel, (screenSize.yMin + screenSize.yMax) * 0.6f, 2f).setEaseOutBounce();
        returnButton.SetActive(true);
        LeanTween.moveY(returnButton,(screenSize.yMin + screenSize.yMax) * 0.15f,1f).setEaseInSine().setDelay(2f);
    }

    public void ClearGameEndAnimation(){
        isPlayingEndAnimation = false;
        isPaused = false;
        LeanTween.alpha(background.rectTransform, 0f, .5f).setEaseInSine();
        LeanTween.cancelAll();
        paused_parent.SetActive(isPaused);
        returnButton.SetActive(false);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
    }


    [Header("Play & Pause")]
    public bool isPaused = false;

    public GameObject paused_p;
    public GameObject paused_a;
    public GameObject paused_u;
    public GameObject paused_s;
    public GameObject paused_e;
    public GameObject paused_d;
    public GameObject paused_parent;
    public void PlayGamePauseAnimation() {
        returnButton.transform.position = new Vector3((screenSize.xMin + screenSize.xMax)/2 ,screenSize.yMin - 100,0);
        LeanTween.alpha(background.rectTransform, 0.5f, 1f).setEaseInSine();
        returnButton.SetActive(true);
        LeanTween.moveY(returnButton,(screenSize.yMin + screenSize.yMax) * 0.15f,1f).setEaseInSine();
        isPaused = true;
        paused_parent.SetActive(isPaused);
        
        LeanTween.moveLocalY(paused_p, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong);
        LeanTween.moveLocalY(paused_a, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong).setDelay(0.1f);
        LeanTween.moveLocalY(paused_u, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong).setDelay(0.2f);
        LeanTween.moveLocalY(paused_s, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong).setDelay(0.3f);
        LeanTween.moveLocalY(paused_e, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong).setDelay(0.4f);
        LeanTween.moveLocalY(paused_d, 40, 1f).setEaseInSine().setLoopType(LeanTweenType.pingPong).setDelay(0.5f);
    }

    public void PlayGameResumeAnimation() {
        isPaused = false;
        LeanTween.cancelAll();
        paused_parent.SetActive(isPaused);
        LeanTween.alpha(background.rectTransform, 0f, 1f).setEaseInSine();
        returnButton.GetComponent<RectTransform>().position = new Vector3(960,-100,0);
        returnButton.SetActive(false);
    }

}
