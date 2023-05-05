using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ReversiManager : MonoBehaviour
{
    public Dictionary<string, ReversiChess> chessesOnBoard;
    public Dictionary<string, List<string>> legalMoves = new Dictionary<string, List<string>>();
    public bool isSpawningChesses { get; private set; }

    public enum Side
    {
        Black = 1, White = 2
    }
    private List<Vector2Int> directions = new List<Vector2Int>() {
        new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
        new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
    };
    public Side currentSide = Side.Black;
    public Side Oppo_side() {
        return (currentSide == Side.Black) ? Side.White : Side.Black;
    }
    public delegate void Callback();
    public void SpawnChesses(Highlight.Callback onClickChess = null, Callback onChessesSpawned = null)
    {
        StartCoroutine(SpawnChessesCoroutine(onClickChess, onChessesSpawned));
    }
    IEnumerator SpawnChessesCoroutine(Highlight.Callback onClickChess = null, Callback onChessesSpawned = null)
    {
        if (isSpawningChesses) yield break;
        isSpawningChesses = true;

        while (true)
        {
            ReversiChessSpawner spawner = FindObjectOfType<ReversiChessSpawner>();
            if (spawner)
            {
                chessesOnBoard = spawner.SpawnChesses(onClickChess);
                if (chessesOnBoard != null) break;
                else Debug.LogError("Chess spawner cannot spawn chess.");
            }
            else
            {
                Debug.Log("Not yet find ChessSpawner");
            }
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Chesses spawned");
        syncBoardwithLocalData();
        isSpawningChesses = false;
        if (onChessesSpawned != null) onChessesSpawned();
    }

    public void syncBoardwithLocalData(){
        foreach (var kvp in chessesOnBoard)
        {
            kvp.Value.zeroSecRotate();
        }
    }

    public Dictionary<string, List<string>> FindLegalMoves(Side side)
    {
        legalMoves.Clear();
        for (int row = 1; row <= 8; row++)
        {
            for (char col = 'A'; col <= 'H'; col++)
            {
                string boardIndex = row.ToString() + col;
                if (IsMoveLegal(boardIndex, out List<string> outflanked,side))
                {
                    legalMoves[boardIndex] = outflanked;
                }
            }
        }
        StringBuilder sb;
        sb = new StringBuilder("Legal moves: ");
        foreach (string key in legalMoves.Keys)
        {
            sb.Append(key + ", ");
        }
        Debug.Log(sb.ToString());
        return legalMoves;
    }

    private bool IsMoveLegal(string boardIndex, out List<string> outflanked, Side side)
    {
        outflanked = new List<string>();
        if (chessesOnBoard.TryGetValue(boardIndex, out ReversiChess placingChess))
        {
            if (placingChess.CurrentState != ReversiChess.State.Unused) return false;
            else
            {
                foreach (Vector2Int dir in directions)
                {
                    string currentIndex = boardIndex;
                    //Check if still inside board
                    List<string> outflankedInThisDir = new List<string>();
                    while (currentIndex[0] >= '1' && currentIndex[0] <= '8' && currentIndex[1] >= 'A' && currentIndex[1] <= 'H')
                    {
                        currentIndex = ((char)(currentIndex[0] + dir.x)).ToString() + (char)(currentIndex[1] + dir.y);
                        if (chessesOnBoard.TryGetValue(currentIndex, out ReversiChess currentChess))
                        {
                            //If the direction ends at an unused chess, then it is not a legal move
                            if (currentChess.CurrentState == ReversiChess.State.Unused)
                            {
                                outflankedInThisDir.Clear();
                                break;
                            }
                            //If the direction ends at our chess, then it is a legal move, return the list
                            else if ((int)currentChess.CurrentState == (int)side)
                            {
                                break;
                            }
                            //If the direction ends at opponent's chess, then add it to the list
                            else
                            {
                                outflankedInThisDir.Add(currentIndex);
                            }
                        }
                        else
                        {
                            //Debug.Log("Fail to get chess at " + currentIndex);
                            break;
                        }
                    }
                    outflanked.AddRange(outflankedInThisDir);
                }
                //Debug.Log("Legal moves for " + boardIndex + ": " + outflanked.Count);
                return outflanked.Count > 0;
            }
        }
        else return false;
    }

    public bool IsValidPlacingIndex(string placingIndex)
    {
        return legalMoves.ContainsKey(placingIndex);
    }
    public bool PlaceChess(string placingIndex)
    {
        if (chessesOnBoard.TryGetValue(placingIndex, out ReversiChess placingChess))
        {
            if (currentSide == Side.Black)
            {
                placingChess.CurrentState = ReversiChess.State.Black;
                placingChess.PlaceBlack();
            }
            else
            {
                placingChess.CurrentState = ReversiChess.State.White;
                placingChess.PlaceWhite();
            }
            //hide all highlights
            foreach (var kvp in chessesOnBoard)
            {
                kvp.Value.hint.gameObject.SetActive(false);
            }
            List<string> clampedChesses = legalMoves[placingIndex];
            Debug.Log("Clamped chesses: " + clampedChesses.Count);
            if (clampedChesses.Count > 0)
            {
                foreach (var clampedChess in clampedChesses)
                {
                    StartCoroutine(chessesOnBoard[clampedChess].Flip());
                }
                //update chessesOnBoard into new state

                return true;
            }
            else return false;
        }
        else return false;
    }

    public bool NoChessIsFlipping()
    {
        return ReversiChess.NoChessIsFlipping;
    }

    public enum GameResult
    {
        NotYetFinished, BlackWin, WhiteWin, Tie
    }
    public GameResult GetGameResult(out Side sideOfNextTurn)
    {
        sideOfNextTurn = Oppo_side();
        Dictionary<string, List<string>> legalMoves = FindLegalMoves(sideOfNextTurn);

        if(legalMoves.Count == 0)
        {
            sideOfNextTurn = currentSide;
            legalMoves = FindLegalMoves(sideOfNextTurn);
            if(legalMoves.Count == 0)
            {
                int blackCount = 0, whiteCount = 0;
                foreach(var kvp in chessesOnBoard)
                {
                    if (kvp.Value.CurrentState == ReversiChess.State.Black) blackCount++;
                    else if (kvp.Value.CurrentState == ReversiChess.State.White) whiteCount++;
                }
                if (blackCount > whiteCount) return GameResult.BlackWin;
                else if (blackCount < whiteCount) return GameResult.WhiteWin;
                else return GameResult.Tie;
            }
        }
        else
        {
            sideOfNextTurn = currentSide;
        }

        return GameResult.NotYetFinished;
    }
}
