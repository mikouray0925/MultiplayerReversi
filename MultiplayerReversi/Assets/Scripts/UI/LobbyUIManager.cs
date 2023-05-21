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
    public GameObject InfoTextBG;
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
        UpdateStats();
    }

    public void ShowAchievementPanel()
    {
        InfoTextBG.GetComponent<Image>().color = new Color(0,1,0.7921f,0);
        AchievementPanel.SetActive(true);
        Mask.SetActive(true);
        LeanTween.alpha(Mask.GetComponent<RectTransform>(), 1f, 0.2f).setEaseOutSine();
        InfoText.text = "";
        LeanTween.alpha(InfoTextBG.GetComponent<RectTransform>(), 1f, 0.5f).setEaseOutSine();
        foreach(var achievement in AchievementList)
        {
            achievement.FadeIn();
        }
    }

    public void HideAchievementPanel()
    {
        InfoText.text = "";
        foreach(var achievement in AchievementList)
        {
            achievement.FadeOut();
        }
        LeanTween.alpha(InfoTextBG.GetComponent<RectTransform>(), 0f, 0.5f).setEaseOutSine();
        LeanTween.alpha(Mask.GetComponent<RectTransform>(), 0f, 0.55f).setEaseOutSine().setOnComplete(delegate(){
            AchievementPanel.SetActive(false);
            Mask.SetActive(false);
        });
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
        demoIndex = 0;
        ShowDemo();
        HowToPlayPanel.transform.localScale = new Vector3(0.01f, 0.01f, 1);
        HowToPlayPanel.SetActive(true);
        LeftButton.gameObject.SetActive(false);
        RightButton.gameObject.SetActive(false);
        LeanTween.scaleX(HowToPlayPanel, 1, 0.3f).setEaseOutBack();
        LeanTween.scaleY(HowToPlayPanel, 1, 0.3f).setEaseOutBack().setDelay(0.35f).setOnComplete(delegate(){
            LeftButton.gameObject.SetActive(true);
            RightButton.gameObject.SetActive(true);
            demoIndex = 1;
            ShowDemo();
        });
        Mask.SetActive(true);
        
    }
    public void HideHowToPlayPanel()
    {
        LeftButton.gameObject.SetActive(false);
        RightButton.gameObject.SetActive(false);
        demoIndex = 0;
        ShowDemo();
        LeanTween.scaleX(HowToPlayPanel, 0.01f, 0.2f).setEaseOutBack();
        LeanTween.scaleY(HowToPlayPanel, 0.01f, 0.2f).setEaseOutBack().setDelay(0.25f).setOnComplete(delegate(){
            HowToPlayPanel.SetActive(false);
            Mask.SetActive(false);
        });
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

    [Header("Stats")]
    public GameObject StatsPanel;
    public Text StatsText;
    public void UpdateStats(){
        string stats = "";
        stats += "Total Games Played: " + PlayerPrefs.GetInt("TotalGameCount", 0) + "\n";
        stats += "Total Games Won: " + PlayerPrefs.GetInt("WinCount", 0) + "\n";
        stats += "Total Games Lost: " + PlayerPrefs.GetInt("LoseCount", 0) + "\n";
        stats += "Total Games Draw: " + PlayerPrefs.GetInt("DrawCount", 0) + "\n";
        stats += "Win Ratio: " +  (PlayerPrefs.GetInt("TotalGameCount", 0) == 0 ? "N/A" : ((float)PlayerPrefs.GetInt("WinCount", 0) / (float)PlayerPrefs.GetInt("TotalGameCount", 0) * 100f).ToString("F2")) + "%\n";
        StatsText.text = stats;
    }
}
