using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AchievementUnit : MonoBehaviour
{
    public LobbyUIManager lobbyUIManager;
    private static Color UnlockedColor = new Color(0.7f,0.7f,0.7f, 1);
    private static Color LockedColor = new Color(0.5f,0.5f,0.5f, 1);
    private Color color;
    public Text Text;
    public ColorBlock colorBlock;
    public Button button;
    private string AchievementName;
    private string Description;
    private bool IsUnlocked;
    public void updateAchievement(string name, bool isUnlocked, string description)
    {
        AchievementName = name;
        IsUnlocked = isUnlocked;
        Description = description;
        Text.text = AchievementName;
        color = IsUnlocked ? UnlockedColor : LockedColor;
        colorBlock.normalColor = color;
        colorBlock.highlightedColor = color - new Color(0.1f,0.1f,0.1f, 0);
        colorBlock.pressedColor = color - new Color(0.2f,0.2f,0.2f, 0);
        colorBlock.selectedColor = color;
        colorBlock.disabledColor = color - new Color(0.3f,0.3f,0.3f, 0);
        colorBlock.colorMultiplier = 1;
        button.colors = colorBlock;
        button.onClick.AddListener(delegate{
            lobbyUIManager.ShowInfo(Description);
        });
    }

    public void FadeIn(){
        button.gameObject.GetComponent<Image>().color = new Color(1,1,1,0);
        gameObject.GetComponent<Image>().color = new Color(1,1,1,0);
        LeanTween.alpha(button.gameObject.GetComponent<RectTransform>(), 1, 0.5f).setEaseOutBack();
        LeanTween.alpha(gameObject.GetComponent<RectTransform>(), 1, 0.5f).setEaseOutBack();
    }
    public void FadeOut(){
        LeanTween.alpha(button.gameObject.GetComponent<RectTransform>(), 0, 0.55f).setEaseOutBack();
        LeanTween.alpha(gameObject.GetComponent<RectTransform>(), 0, 0.55f).setEaseOutBack();
    }
}
