using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PunReversiManager : MonoBehaviourPunCallbacks
{
    public PunRoomManager roomManager;
    public ReversiManager reversiManager;
    private bool isLoadingBoardData;
    private bool boardDataLoaded;

    public enum GameState {
        Paused, WaitingForAllReady, WaitingForOrder, End
    }
    public GameState currentState = GameState.Paused;

    // only for master
    bool blackReady = false;
    bool whiteReady = false;

    PhotonView pv;

    private void Awake() {
        if (TryGetComponent<PhotonView>(out PhotonView _pv)) {
            pv = _pv;
        } else {
            pv = gameObject.AddComponent<PhotonView>();
        }
        roomManager = GetComponent<PunRoomManager>();
        reversiManager = GetComponent<ReversiManager>();
    }

    public void Initialize() {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        propNeedToChange["gameState"] = GameState.Paused;
        reversiManager.currentSide = ReversiManager.Side.Black;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        propNeedToChange["blackActId"] = PhotonNetwork.LocalPlayer.ActorNumber;
        propNeedToChange["whiteActId"] = -1;
        propNeedToChange["lastUploadGameDataTime"] = -1.0;

        for (int row = 1; row <= 8; row++) {
            for (char col = 'A'; col <= 'H'; col++) {
                propNeedToChange[row.ToString() + col] = ReversiChess.State.Unused;
            }
        }
        
        roomManager.ChangeCustomProperties(propNeedToChange);
    }

    private void Update() {
        if (PhotonNetwork.InRoom && roomManager != null &&
            (PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Playing) {
            if (PhotonNetwork.IsMasterClient) DoMasterOnlyBusiness();
            else DoNonMasterOnlyBusiness();
            DoCommonBusiness();
        }
    }

    private void DoMasterOnlyBusiness() {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        
        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if (gameState == GameState.Paused) {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] != -1 &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] != -1) {
                currentState = GameState.WaitingForAllReady;
            }
            // Set opponent as opposite side
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2) {
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1) {
                    foreach (var kvp in PhotonNetwork.CurrentRoom.Players) {
                        if (kvp.Value.ActorNumber != (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"]) {
                            propNeedToChange["blackActId"] = kvp.Value.ActorNumber;
                            break;
                        }
                    }
                } 
                else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1) {
                    foreach (var kvp in PhotonNetwork.CurrentRoom.Players) {
                        if (kvp.Value.ActorNumber != (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"]) {
                            propNeedToChange["whiteActId"] = kvp.Value.ActorNumber;
                            break;
                        }
                    }
                }
            }
        }
        // TODO: if one player left, return to previous game state?
        if (gameState == GameState.WaitingForAllReady) {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] != -1 &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] != -1 &&
                blackReady && whiteReady) {
                currentState = GameState.WaitingForOrder;
            }
        }

        propNeedToChange["gameState"] = currentState;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        roomManager.ChangeCustomProperties(propNeedToChange);
        CallMasterUploadGameData();
    }

    private void DoNonMasterOnlyBusiness() {

    }

    private void DoCommonBusiness() {
        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if (gameState == GameState.Paused) {
            
        }
        if (gameState == GameState.WaitingForAllReady) {
            if (reversiManager.chessesOnBoard == null) {
                reversiManager.SpawnChesses(LoadChessesData);
            }  
            if (boardDataLoaded && NoChessIsFlipping()) CallMasterSomePlayerIsReady();
        }
    }

    public void CallMasterUploadGameData() {
        pv.RPC("RpcUploadGameData", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcUploadGameData(PhotonMessageInfo info) {
        if (PhotonNetwork.InRoom && roomManager) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            if (reversiManager.chessesOnBoard != null) {
                for (int row = 1; row <= 8; row++) {
                    for (char col = 'A'; col <= 'H'; col++) {
                        string boardIndex = row.ToString() + col;
                        propNeedToChange[boardIndex] = reversiManager.chessesOnBoard[boardIndex].currentState;
                    }
                }

                propNeedToChange["lastUploadGameDataTime"] = PhotonNetwork.Time;
            }
            roomManager.ChangeCustomProperties(propNeedToChange);
        }
    }

    private void CallMasterSomePlayerIsReady() {
        pv.RPC("RpcSomePlayerIsReady", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcSomePlayerIsReady(PhotonMessageInfo info) {
        if(BlackPlayer != null && BlackPlayer.ActorNumber == info.Sender.ActorNumber) {
            blackReady = true;
        }
        if (WhitePlayer != null && WhitePlayer.ActorNumber == info.Sender.ActorNumber) {
            whiteReady = true;
        }
    }

    private void LoadChessesData() {
        StartCoroutine(LoadChessesDataCoroutine());
    }
    IEnumerator LoadChessesDataCoroutine() {
        if (isLoadingBoardData) yield break;
        isLoadingBoardData = true;

        CallMasterUploadGameData();
        double callUploadTime = PhotonNetwork.Time;
        Debug.Log(callUploadTime);
        while ((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"] < callUploadTime) {
            Debug.Log((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"]);
            yield return new WaitForSeconds(0.1f);
        } 

        foreach (var kvp in reversiManager.chessesOnBoard) {
            kvp.Value.CurrentState = (ReversiChess.State)PhotonNetwork.CurrentRoom.CustomProperties[kvp.Value.BoardIndex];
        }
        boardDataLoaded = true;
        Debug.Log("Board data loaded");

        isLoadingBoardData = false;
    }

    public void StartGame() {
        reversiManager.SpawnChesses(InitGame);
    }

    private void InitGame() {
        reversiManager.chessesOnBoard["4D"].CurrentState = ReversiChess.State.White;
        reversiManager.chessesOnBoard["4E"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5D"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5E"].CurrentState = ReversiChess.State.White;
        boardDataLoaded = true;
        CallMasterUploadGameData();
    }

    public bool NoChessIsFlipping() {
        return true;
    }

    public Player BlackPlayer {
        get {
            if (PhotonNetwork.InRoom && roomManager) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                if (roomManager.players.TryGetValue(actorId, out Player player)) {
                    return player;
                } else return null;
            } else return null;
        }
    }

    public Player WhitePlayer {
        get {
            if (PhotonNetwork.InRoom && roomManager) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                if (roomManager.players.TryGetValue(actorId, out Player player)) {
                    return player;
                } else return null;
            } else return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player player) {
        if (PhotonNetwork.InRoom && roomManager && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1) {
                propNeedToChange["blackActId"] = player.ActorNumber;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }

            else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1) {
                propNeedToChange["whiteActId"] = player.ActorNumber;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player player) {
        if (PhotonNetwork.InRoom && roomManager && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == player.ActorNumber) {
                propNeedToChange["blackActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                blackReady = false;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == player.ActorNumber) {
                propNeedToChange["whiteActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                whiteReady = false;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }
        }
    }
}
