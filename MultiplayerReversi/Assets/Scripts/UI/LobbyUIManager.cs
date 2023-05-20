using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LobbyUIManager : MonoBehaviour
{
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
            unit.LobbyUIManager = this;
            unit.updateAchievement(achievement.Key, achievement.Value);
            unit.GetComponentInChildren<Button>().onClick.AddListener(delegate{ShowInfo(AchievementDescription[achievement.Key]);});
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
        InfoText.text = info;
    }
}
