using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiChessSpawner : MonoBehaviour
{
    public static ReversiChessSpawner Instance;
    public GameObject chessPrefab;
    public GameObject hintPrefab;
    public float slotLength;
    
    void Start()
    {
        Instance = this;
    }
    public Dictionary<string, ReversiChess> SpawnChesses(Highlight.Callback onClickChess) {
        if (chessPrefab) {
            Dictionary<string, ReversiChess> chesses = new Dictionary<string, ReversiChess>();
            Vector3 offset = Vector3.left * slotLength * 3.5f + Vector3.forward * slotLength * 3.5f;
            Vector3 currentPos = transform.position + offset;

            for (int row = 1; row <= 8; row++) {
                for (char col = 'A'; col <= 'H'; col++) {
                    string boardIndex = row.ToString() + col;
                    ReversiChess chess = Instantiate(chessPrefab, currentPos, Quaternion.identity, transform).GetComponent<ReversiChess>();
                    chess.gameObject.SetActive(true); 
                    chess.BoardIndex = boardIndex;
                    Highlight hint = Instantiate(hintPrefab, currentPos + new Vector3(0,-0.08f,0), Quaternion.identity, chess.gameObject.transform).GetComponent<Highlight>();
                    hint.gameObject.SetActive(false);
                    hint.GetComponent<Highlight>().chess = chess;
                    hint.onClicked = onClickChess;
                    chess.hint = hint;
                    chess.meshRenderer.enabled = false;
                    chesses[boardIndex] = chess;
                    currentPos += Vector3.right * slotLength;
                }
                currentPos += Vector3.back * slotLength;
                currentPos += Vector3.left * slotLength * 8;
            }
            return chesses;
        } else return null;
    }
}
