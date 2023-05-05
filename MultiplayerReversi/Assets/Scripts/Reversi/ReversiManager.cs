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
    public void SpawnChesses(Callback onChessesSpawned = null) {
        StartCoroutine(SpawnChessesCoroutine(onChessesSpawned));
    }
    IEnumerator SpawnChessesCoroutine(Callback onChessesSpawned = null) {
        if (isSpawningChesses) yield break;
        isSpawningChesses = true;
        
        while (true) {
            ReversiChessSpawner spawner = FindObjectOfType<ReversiChessSpawner>();
            if (spawner) {
                chessesOnBoard = spawner.SpawnChesses();
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
}
