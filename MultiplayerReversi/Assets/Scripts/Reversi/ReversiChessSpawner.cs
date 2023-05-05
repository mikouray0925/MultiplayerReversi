using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiChessSpawner : MonoBehaviour
{
    public GameObject chessPrefab;
    public float slotLength;

    public Dictionary<string, ReversiChess> SpawnChesses(ReversiChess.Callback onClickChess) {
        if (chessPrefab) {
            Dictionary<string, ReversiChess> chesses = new Dictionary<string, ReversiChess>();
            Vector3 offset = Vector3.left * slotLength * 3.5f + Vector3.forward * slotLength * 3.5f;
            Vector3 currentPos = transform.position + offset;

            for (int row = 1; row <= 8; row++) {
                for (char col = 'A'; col <= 'H'; col++) {
                    string boardIndex = row.ToString() + col;
                    ReversiChess chess = Instantiate(chessPrefab, currentPos, Quaternion.identity, transform).GetComponent<ReversiChess>(); 
                    chess.BoardIndex = boardIndex;
                    chess.onClicked = onClickChess;
                    chesses[boardIndex] = chess;
                    chess.gameObject.SetActive(false);
                    currentPos += Vector3.right * slotLength;
                }
                currentPos += Vector3.back * slotLength;
                currentPos += Vector3.left * slotLength * 8;
            }
            return chesses;
        } else return null;
    }
}
