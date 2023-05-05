using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiManager : MonoBehaviour
{
    public Dictionary<string, ReversiChess> chessesOnBoard;
    public bool isSpawningChesses {get; private set;}

    public enum Side {
        Black, White
    }
    public Side currentSide = Side.Black;

    public delegate void Callback();
    public void SpawnChesses(ReversiChess.Callback onClickChess = null, Callback onChessesSpawned = null) {
        StartCoroutine(SpawnChessesCoroutine(onClickChess, onChessesSpawned));
    }
    IEnumerator SpawnChessesCoroutine(ReversiChess.Callback onClickChess = null, Callback onChessesSpawned = null) {
        if (isSpawningChesses) yield break;
        isSpawningChesses = true;
        
        while (true) {
            ReversiChessSpawner spawner = FindObjectOfType<ReversiChessSpawner>();
            if (spawner) {
                chessesOnBoard = spawner.SpawnChesses(onClickChess);
                if (chessesOnBoard != null) break;
                else Debug.LogError("Chess spawner cannot spawn chess.");
            } else {
                Debug.Log("Not yet find ChessSpawner");
            }
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Chesses spawned");
        isSpawningChesses = false;
        if (onChessesSpawned != null) onChessesSpawned();
    }

    public List<ReversiChess> GetClampedChesses(string placingIndex) {
        // TODO: get clamped chesses if the index is placed with a chess.
        return new List<ReversiChess>();
    }

    public bool IsValidPlacingIndex(string placingIndex) {
        // TODO: check whether this is a valid index to place chess.
        return false;
    }

    public bool PlaceChess(string placingIndex) {
        if (chessesOnBoard.TryGetValue(placingIndex, out ReversiChess placingChess)) {
            if (currentSide == Side.Black) 
                placingChess.CurrentState =  ReversiChess.State.Black;
            else                           
                placingChess.CurrentState =  ReversiChess.State.White;

            List<ReversiChess> clampedChesses = GetClampedChesses(placingIndex);
            if (clampedChesses.Count > 0) {
                foreach(var clampedChess in clampedChesses) {
                    clampedChess.Flip();
                }
                return true;
            }
            else return false;
        }
        else return false;
    }

    public bool NoChessIsFlipping() {
        // TODO: I'm tired. You do it.
        return false;
    }

    public enum GameResult {
        NotYetFinished, BlackWin, WhiteWin
    }
    public GameResult GetGameResult(out Side sideOfNextTurn) {
        // TODO
        sideOfNextTurn = Side.Black;
        return GameResult.NotYetFinished;
    }
}
