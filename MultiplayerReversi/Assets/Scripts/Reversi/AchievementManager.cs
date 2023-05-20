using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AchievementManager : MonoBehaviour{
    //<Summary>
    //Singleton class, only exist get method
    //</Summary>
    private static int WinCount;
    private static int LoseCount;
    private static int DrawCount;
    private static int TotalGameCount;
    private static Dictionary<string, bool> AchievementProgress = new Dictionary<string, bool>();

    private void Awake(){
        if (TryGetComponent<AchievementManager>(out AchievementManager _instance)){
            Instance = _instance;
        }
        else {
            Instance = gameObject.AddComponent<AchievementManager>();
        }
    }
    public static AchievementManager Instance {get; private set;}
    internal void UnlockAchievement(string achievementName){
        if(AchievementList.Contains(achievementName)){
            AchievementProgress[achievementName] = true;
        }
    }
    internal void AddWinCount(){
        WinCount++;
        TotalGameCount++;
    }
    internal void AddLoseCount(){
        LoseCount++;
        TotalGameCount++;
    }
    internal void AddDrawCount(){
        DrawCount++;
        TotalGameCount++;
    }
    private static void ExportData(){
        //TODO export data to file
    }

    private static List<string> AchievementList  = new List<string>(){
            "Nice move",
            "Sheeeesh",
            "Huh? TF?",
            "Black King",
            "Black Emperor",
            "Black God",
            "White King",
            "White Emperor",
            "White God",
            "Winner"
    };

}

class AchievementHandler 
{
    //TODO add achs
    private static int BlackChessCount = 0;
    private static int WhiteChessCount = 0;
    public static void HandlePlaceChess(List<string> flankedChesses){
        if(flankedChesses.Count > 5) {
            AchievementManager.Instance.UnlockAchievement("Nice move");
        }
        if(flankedChesses.Count > 10) {
            AchievementManager.Instance.UnlockAchievement("Sheeeesh");
        }
        if(flankedChesses.Count > 20) {
            AchievementManager.Instance.UnlockAchievement("Huh? TF?");
        }
    }

    public static void HandleEndGame(Dictionary<string, ReversiChess> chessesOnBoard, GameResult result, ReversiManager.Side side){
          //Copy and handle flip first
        Dictionary<string, ReversiChess.State> chessStates = new Dictionary<string, ReversiChess.State>();
        foreach (var kvp in chessesOnBoard)
        {
            chessStates.Add(kvp.Key, kvp.Value.CurrentState);
        }

        //iterate and get stats
        BlackChessCount = 0;
        WhiteChessCount = 0;
        foreach (var kvp in chessStates)
        {
            if(kvp.Value == ReversiChess.State.Black) BlackChessCount++;
            else if(kvp.Value == ReversiChess.State.White) WhiteChessCount++;
        }
        
        if(result == GameResult.Win){
            // Win achs 
            // dont ask me why i use 20 40 60, copilot gives me these numbers
            // and dont ask me about the achs name, copilot gives me these names
            if(side == ReversiManager.Side.Black){
                if(BlackChessCount > 20) AchievementManager.Instance.UnlockAchievement("Black King");
                if(BlackChessCount > 40) AchievementManager.Instance.UnlockAchievement("Black Emperor");
                if(BlackChessCount > 60) AchievementManager.Instance.UnlockAchievement("Black God");
            }
            else{
                if(WhiteChessCount > 20) AchievementManager.Instance.UnlockAchievement("White King");
                if(WhiteChessCount > 40) AchievementManager.Instance.UnlockAchievement("White Emperor");
                if(WhiteChessCount > 60) AchievementManager.Instance.UnlockAchievement("White God");
            }
            AchievementManager.Instance.UnlockAchievement("Winner");
            AchievementManager.Instance.AddWinCount();
        }
        else if(result == GameResult.Lose){
            // Lose achs
            AchievementManager.Instance.AddLoseCount();
        }
        else if(result == GameResult.Tie){
            // Tie achs
            AchievementManager.Instance.AddDrawCount();
        }
    }

    public enum GameResult{
        Win, Lose, Tie
    }
}


