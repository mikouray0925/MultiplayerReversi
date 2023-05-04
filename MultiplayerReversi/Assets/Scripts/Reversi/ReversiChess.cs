using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiChess : MonoBehaviour
{
    public enum State {
        Unuse,
        Black,
        White,
        Flipping
    }

    public State currentState {get; private set;} = State.Unuse;
    public string boardIndex {get; private set;} = "";

    public string BoardIndex {
        get {
            return boardIndex;
        }
        set {
            if (boardIndex.Length == 0) {
                boardIndex = value;
            } else {
                Debug.LogWarning("boardIndex can be set only once.");
            }
        }
    }
}
