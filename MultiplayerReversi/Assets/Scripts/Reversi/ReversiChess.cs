using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiChess : MonoBehaviour
{
    public enum State {
        Unused = 0,
        Black = 1,
        White = 2,
        FlippingToBlack,
        FlippingToWhite
    }
    public static bool NoChessIsFlipping = true;
    public State currentState {get; private set;} = State.Unused;
    public int stateID;
    public MeshRenderer meshRenderer;
    [SerializeField] Animator animator;
    [SerializeField] Transform model_transform;
    public State CurrentState {
        get {
            return currentState;
        }
        set {
            meshRenderer.enabled = (value != State.Unused);
            if (value == State.Black) {
                model_transform.rotation = Quaternion.identity;
            }
            if (value == State.White) {
                model_transform.eulerAngles = new Vector3(180f, 0, 0);
            }
            currentState = value;
            stateID = (int)value;
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

    public Highlight hint;

    public void PlaceWhite(){
        animator.enabled = true;
        NoChessIsFlipping = false;
        animator.Play("PlaceWhite");
        StartCoroutine("animSleep");
    }
    public void PlaceBlack(){
        animator.enabled = true;
        NoChessIsFlipping = false;
        animator.Play("PlaceBlack");
        StartCoroutine("animSleep");
    }

    public void zeroSecRotate(){
        if (currentState == State.Black) {
            //animator.enabled = true;
            //animator.Play("ZeroSecRotateBlack");
            //StartCoroutine(animSleep(0.1f).ToString());
            model_transform.rotation = Quaternion.identity;
        }
        if (currentState == State.White) {
            //animator.enabled = true;
            //animator.Play("ZeroSecRotateWhite");
            //StartCoroutine(animSleep(0.1f).ToString());
            model_transform.rotation = Quaternion.Euler(180f, 0, 0);
        }
    }
    public IEnumerable animSleep(){
        yield return new WaitForSeconds(0.4f);
        animator.enabled = false;
    }
    public IEnumerator Flip() {
        // TODO: flip the chess. remember to update currentState.
        animator.enabled = true;
        WaitForSeconds wait = new WaitForSeconds(0.4f);
        yield return wait;
        Debug.Log("Flipping " + boardIndex+ "State: " + currentState);
        if (currentState == State.Black) {
            animator.Play("BlackToWhite");
            currentState = State.FlippingToWhite;
            Invoke("OnFlipEnd", 0.84f);
        } else if (currentState == State.White) {
            animator.Play("WhiteToBlack");
            currentState = State.FlippingToBlack;
            Invoke("OnFlipEnd", 0.84f);
        }
    }

    private void OnFlipEnd() { 
        if (currentState == State.FlippingToBlack) {
            currentState = State.Black;
        } else if (currentState == State.FlippingToWhite) {
            currentState = State.White;
        }
        Debug.Log("Flipped " + boardIndex + "State: " + currentState);
        stateID = (int)currentState;
        NoChessIsFlipping = true;
        animator.enabled = false;
    }
}
