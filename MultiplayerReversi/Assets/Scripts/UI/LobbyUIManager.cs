using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LobbyUIManager : MonoBehaviour
{
    [Header("Achievement")]
    public GameObject AchievementPanel;
    public GameObject AchievementPrefab;
    public GameObject Mask;
    private List<AchievementUnit> AchievementList = new List<AchievementUnit>();
    private static Dictionary<string,string> AchievementDescription = new Dictionary<string, string>(){
        // "Nice move" : "Make a move that flips 5 or more pieces",
        // "Sheeeesh" :"Make a move that flips 10 or more pieces",
        // "Huh? TF?" : "Make a move that flips 20 or more pieces",
        // "Black King" : "Win a game with 20 or more black pieces",
        // "Black Emperor" : "Win a game with 40 or more black pieces",
        // "Black God" : "Win a game with 60 or more black pieces",
        // "White King" : "Win a game with 20 or more white pieces",
        // "White Emperor" : "Win a game with 40 or more white pieces",
        // "White God" : "Win a game with 60 or more white pieces",
        // "Winner" : "Win a game"
        // format {Key,Value}

        {"Nice Move", "Make a move that flips 5 or more pieces"},
        {"Sheeeesh", "Make a move that flips 10 or more pieces"},
        {"Huh? TF?", "Make a move that flips 20 or more pieces"},
        {"Black King", "Win a game with 20 or more black pieces"},
        {"Black Emperor", "Win a game with 40 or more black pieces"},
        {"Black God", "Win a game with 60 or more black pieces"},
        {"White King", "Win a game with 20 or more white pieces"},
        {"White Emperor", "Win a game with 40 or more white pieces"},
        {"White God", "Win a game with 60 or more white pieces"},
        {"Winner", "Win a game"}
    };
    public Text InfoText;
    public void Awake()
    {
        AchievementPanel.SetActive(false);

        foreach(var achievement in AchievementManager.Instance.GetAchievementProgress())
        {
            AchievementUnit unit = Instantiate(AchievementPrefab, AchievementPanel.transform).GetComponent<AchievementUnit>();
            AchievementList.Add(unit);
            unit.lobbyUIManager = this;
            unit.updateAchievement(achievement.Key, achievement.Value, AchievementDescription[achievement.Key]);
        }
    }

    public void ShowAchievementPanel()
    {
        AchievementPanel.SetActive(true);
        Mask.SetActive(true);
        InfoText.text = "";
    }

    public void HideAchievementPanel()
    {
        AchievementPanel.SetActive(false);
        Mask.SetActive(false);
    }

    public void ShowInfo(string info)
    {
        Debug.Log(info);
        InfoText.text = info;
    }


    [Header("How to Play")]
    public GameObject HowToPlayPanel;
    public GameObject Demo1;
    public GameObject Demo2;
    public GameObject Demo3;
    public Button LeftButton;
    public Button RightButton;
    private int demoIndex = 1;
    public void ShowHowToPlayPanel()
    {
        HowToPlayPanel.SetActive(true);
        Mask.SetActive(true);
        demoIndex = 1;
        ShowDemo();
    }
    public void HideHowToPlayPanel()
    {
        HowToPlayPanel.SetActive(false);
        Mask.SetActive(false);
    }

    public void NextDemo(){
        demoIndex++;
        if(demoIndex > 3){
            demoIndex = 3;
        }
        ShowDemo();
    }
    public void PrevDemo(){
        demoIndex--;
        if(demoIndex < 1){
            demoIndex = 1;
        }
        ShowDemo();
    }

    private void ShowDemo(){
        Demo1.SetActive(false);
        Demo2.SetActive(false);
        Demo3.SetActive(false);
        RightButton.interactable = true;
        LeftButton.interactable = true;
        if(demoIndex == 1){
            Demo1.SetActive(true);
            LeftButton.interactable = false;
        }else if(demoIndex == 2){
            Demo2.SetActive(true);
        }else if(demoIndex == 3){
            Demo3.SetActive(true);
            RightButton.interactable = false;
        }
    }
}
