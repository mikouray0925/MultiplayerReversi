using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    public Button button;
    private void Awake(){
        button.interactable = true;
        LeanTween.scale(button.gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutSine);
    }
    public void PlayMultiplayerMode() {
        UserInquirer.instance.InquirString(PunManager.instance.Connect, "Enter name:");
    }
    public void afterSubmitLoading(){
        button.interactable = false;
        LeanTween.scale(button.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInSine);
    }
}
