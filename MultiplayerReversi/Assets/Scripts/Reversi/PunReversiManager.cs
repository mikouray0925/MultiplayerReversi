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
    public PunRoomManager inRoom;
    public PunSceneController sceneController;
    public string gameSceneName;
    public Dictionary<string, ReversiChess> chessesOnBoard;
    private bool isSpawningChesses;
    private bool isLoadingBoardData;
    private bool boardDataLoaded;

    public enum GameState {
        Paused, WaitingForAllReady, WaitingForOrder, End
    }
    public GameState currentState {get; private set;} = GameState.Paused;

    public enum Side {
        Black, White
    }

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
    }

    public void Initialize() {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        propNeedToChange["gameState"] = GameState.Paused;
        propNeedToChange["currentSide"] = Side.Black;
        propNeedToChange["blackActId"] = PhotonNetwork.LocalPlayer.ActorNumber;
        propNeedToChange["whiteActId"] = -1;
        propNeedToChange["lastUploadGameDataTime"] = -1.0;

        for (int row = 1; row <= 8; row++) {
            for (char col = 'A'; col <= 'H'; col++) {
                propNeedToChange[row.ToString() + col] = ReversiChess.State.Unused;
            }
        }

        inRoom.ChangeCustomProperties(propNeedToChange);
    }

    private void Update() {
        if ( PhotonNetwork.InRoom && inRoom &&
            (PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Playing) {
            if (PhotonNetwork.IsMasterClient) DoMasterClientPlayBusiness();
            DoClientPlayBusiness();
        }
    }

    private void DoMasterClientPlayBusiness() {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        
        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if (gameState == GameState.Paused) {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] != -1 &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] != -1) {
                currentState = GameState.WaitingForAllReady;
            }
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

        if (gameState == GameState.WaitingForAllReady) {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] != -1 &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] != -1 &&
                blackReady && whiteReady) {
                currentState = GameState.WaitingForOrder;
            }
        }

        propNeedToChange["gameState"] = currentState;
        inRoom.ChangeCustomProperties(propNeedToChange);
        CallMasterUploadGameData();
    }

    private void DoClientPlayBusiness() {
        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if (gameState == GameState.Paused) {
            
        }
        if (gameState == GameState.WaitingForAllReady) {
            if (chessesOnBoard == null && !SceneController.instance.isChangingScene) {
                SpawnChesses(LoadBoardData);
            }  
            if (boardDataLoaded && NoChessIsFlipping()) CallMasterSomePlayerIsReady();
        }
    }

    public void CallMasterUploadGameData() {
        pv.RPC("RpcUploadGameData", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcUploadGameData(PhotonMessageInfo info) {
        if (PhotonNetwork.InRoom && inRoom) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            if (chessesOnBoard != null) {
                for (int row = 1; row <= 8; row++) {
                    for (char col = 'A'; col <= 'H'; col++) {
                        string boardIndex = row.ToString() + col;
                        propNeedToChange[boardIndex] = chessesOnBoard[boardIndex].currentState;
                    }
                }

                propNeedToChange["lastUploadGameDataTime"] = PhotonNetwork.Time;
            }
            inRoom.ChangeCustomProperties(propNeedToChange);
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

    public delegate void Callback();
    private void SpawnChesses(Callback onChessesSpawned = null) {
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

    private void LoadBoardData() {
        StartCoroutine(LoadBoardDataCoroutine());
    }
    IEnumerator LoadBoardDataCoroutine() {
        if (isLoadingBoardData) yield break;
        isLoadingBoardData = true;

        CallMasterUploadGameData();
        double callUploadTime = PhotonNetwork.Time;
        Debug.Log(callUploadTime);
        while ((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"] < callUploadTime) {
            Debug.Log((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"]);
            yield return new WaitForSeconds(0.1f);
        } 

        foreach (var kvp in chessesOnBoard) {
            kvp.Value.CurrentState = (ReversiChess.State)PhotonNetwork.CurrentRoom.CustomProperties[kvp.Value.BoardIndex];
        }
        boardDataLoaded = true;
        Debug.Log("Board data loaded");

        isLoadingBoardData = false;
    }

    public void StartGame() {
        SpawnChesses(InitGame);
    }

    private void InitGame() {
        chessesOnBoard["4D"].CurrentState = ReversiChess.State.White;
        chessesOnBoard["4E"].CurrentState = ReversiChess.State.Black;
        chessesOnBoard["5D"].CurrentState = ReversiChess.State.Black;
        chessesOnBoard["5E"].CurrentState = ReversiChess.State.White;
        boardDataLoaded = true;
        CallMasterUploadGameData();
    }

    public bool NoChessIsFlipping() {
        return true;
    }

    public Player BlackPlayer {
        get {
            if (PhotonNetwork.InRoom && inRoom) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                if (inRoom.players.TryGetValue(actorId, out Player player)) {
                    return player;
                } else return null;
            } else return null;
        }
    }

    public Player WhitePlayer {
        get {
            if (PhotonNetwork.InRoom && inRoom) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                if (inRoom.players.TryGetValue(actorId, out Player player)) {
                    return player;
                } else return null;
            } else return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player player) {
        if (PhotonNetwork.InRoom && inRoom && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1) {
                propNeedToChange["blackActId"] = player.ActorNumber;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }

            else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1) {
                propNeedToChange["whiteActId"] = player.ActorNumber;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player player) {
        if (PhotonNetwork.InRoom && inRoom && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == player.ActorNumber) {
                propNeedToChange["blackActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                blackReady = false;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == player.ActorNumber) {
                propNeedToChange["whiteActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                whiteReady = false;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }
        }
    }
}
