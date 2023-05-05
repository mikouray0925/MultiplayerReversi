using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiChess : MonoBehaviour
{
    public enum State {
        Unused,
        Black,
        White,
        FlippingToBlack,
        FlippingToWhite
    }

    public State currentState {get; private set;} = State.Unused;

    public State CurrentState {
        get {
            return currentState;
        }
        set {
            gameObject.SetActive(value != State.Unused);
            if (value == State.Black || value == State.FlippingToBlack) {
                transform.rotation = Quaternion.identity;
            }
            if (value == State.White || value == State.FlippingToWhite) {
                transform.eulerAngles = new Vector3(180f, 0, 0);
            }
            currentState = value;
        }
    }

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

    public delegate void Callback(string index);
    public Callback onClicked;

    private void Update() {
        // TODO: check player click
    }

    public void Flip() {
        // TODO: flip the chess. remember to update currentState.
    }
}
