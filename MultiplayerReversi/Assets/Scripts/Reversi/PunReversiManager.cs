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
    public bool boardDataLoaded;

    public bool isHintSpawned = false;

    public enum GameState
    {
        Paused, WaitingForAllReady, WaitingForOrder, End
    }
    public GameState currentState = GameState.Paused;

    bool selfReady = false;
    // only for master
    bool blackReady = false;
    bool whiteReady = false;
    bool placeChessAckReceived = false;

    PhotonView pv;

    private void Awake()
    {
        if (TryGetComponent<PhotonView>(out PhotonView _pv))
        {
            pv = _pv;
        }
        else
        {
            pv = gameObject.AddComponent<PhotonView>();
        }
        roomManager = GetComponent<PunRoomManager>();
        reversiManager = GetComponent<ReversiManager>();
    }

    public void Initialize()
    {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        propNeedToChange["gameState"] = GameState.Paused;
        reversiManager.currentSide = ReversiManager.Side.Black;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        propNeedToChange["blackActId"] = PhotonNetwork.LocalPlayer.ActorNumber;
        propNeedToChange["whiteActId"] = -1;
        propNeedToChange["lastUploadGameDataTime"] = -1.0;

        for (int row = 1; row <= 8; row++)
        {
            for (char col = 'A'; col <= 'H'; col++)
            {
                propNeedToChange[row.ToString() + col] = ReversiChess.State.Unused;
            }
        }

        roomManager.ChangeCustomProperties(propNeedToChange);
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.InRoom && roomManager != null &&
            (PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Playing)
        {
            if (PhotonNetwork.IsMasterClient) DoMasterOnlyBusiness();
            else DoNonMasterOnlyBusiness();
            DoCommonBusiness();
        }
    }

    private void DoMasterOnlyBusiness()
    {
        PhotonHashtable propNeedToChange = new PhotonHashtable();

        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if (gameState == GameState.Paused)
        {
            if (BlackPlayer != null && WhitePlayer != null)
            {
                currentState = GameState.WaitingForAllReady;
            }
            else
            {
                FillEmptyPlayer(propNeedToChange);
            }
        }
        if (gameState == GameState.WaitingForAllReady)
        {
            if (BlackPlayer != null && blackReady &&
                WhitePlayer != null && whiteReady)
            {
                currentState = GameState.WaitingForOrder;
                placeChessAckReceived = false;
            }
        }
        if (gameState == GameState.WaitingForOrder)
        {
            if (placeChessAckReceived)
            {
                ReversiManager.GameResult gameResult = reversiManager.GetGameResult(out ReversiManager.Side sideOfNextRound);
                if (gameResult == ReversiManager.GameResult.BlackWin)
                {
                    // TODO
                    currentState = GameState.End;
                }
                else if (gameResult == ReversiManager.GameResult.WhiteWin)
                {
                    // TODO
                    currentState = GameState.End;
                }
                else
                {
                    reversiManager.currentSide = sideOfNextRound;
                    currentState = GameState.WaitingForAllReady;
                }
            }
        }

        propNeedToChange["gameState"] = currentState;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        roomManager.ChangeCustomProperties(propNeedToChange);
        CallMasterUploadGameData();
    }

    private void DoNonMasterOnlyBusiness()
    {

    }

    private void DoCommonBusiness()
    {
        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        reversiManager.currentSide = (ReversiManager.Side)PhotonNetwork.CurrentRoom.CustomProperties["currentSide"];
        if (gameState == GameState.Paused)
        {

        }
        if (gameState == GameState.WaitingForAllReady)
        {
            if (reversiManager.chessesOnBoard == null)
            {
                reversiManager.SpawnChesses(OnChessClicked, LoadChessesData);
            }
            if (boardDataLoaded && reversiManager.NoChessIsFlipping() && !selfReady) CallMasterSomePlayerIsReady();
            if (isHintSpawned) isHintSpawned = false;
        }
        if (gameState == GameState.WaitingForOrder)
        {
            // TODO: Hint player where can be placed.
            if (!isHintSpawned)
            {
                Dictionary<string, List<string>> LegalMoves = reversiManager.FindLegalMoves();
                Debug.Log(LegalMoves);
                foreach (var kvp in LegalMoves)
                {
                    reversiManager.chessesOnBoard[kvp.Key].hint.gameObject.SetActive(true);
                }
                isHintSpawned = true;
            }
        }
    }
    //Only for checking player input
    private void Update()
    {
        //Raycast check if hint is clicked
        if (Input.GetMouseButtonDown(0) && IsMyTurn())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Mouse Clicked");
            if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("HighLight")))
            {
                //If hits a highlight
                if (hit.transform.gameObject.tag == "HighLight")
                {
                    Highlight highlight = hit.transform.GetComponent<Highlight>();
                    Debug.Log("Highlight at " + highlight.chess.boardIndex + " is clicked");
                    highlight.chess.meshRenderer.enabled = true;
                    if(reversiManager.PlaceChess(highlight.chess.boardIndex)){
                        Debug.Log("Place Chess result: Success");
                        OnChessClicked(highlight.chess.boardIndex);
                    }
                    
                }
            }
        }
    }

    private void FillEmptyPlayer(PhotonHashtable propNeedToChangeBuffer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1)
            {
                foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
                {
                    if (kvp.Value.ActorNumber != (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"])
                    {
                        propNeedToChangeBuffer["blackActId"] = kvp.Value.ActorNumber;
                        break;
                    }
                }
            }
            else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1)
            {
                foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
                {
                    if (kvp.Value.ActorNumber != (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"])
                    {
                        propNeedToChangeBuffer["whiteActId"] = kvp.Value.ActorNumber;
                        break;
                    }
                }
            }
        }
    }

    public bool IsMyTurn()
    {
        if ((GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] == GameState.WaitingForOrder)
        {
            if (reversiManager.currentSide == ReversiManager.Side.Black &&
                BlackPlayer != null &&
                PhotonNetwork.LocalPlayer.ActorNumber == BlackPlayer.ActorNumber)
            {
                return true;
            }
            if (reversiManager.currentSide == ReversiManager.Side.White &&
                WhitePlayer != null &&
                PhotonNetwork.LocalPlayer.ActorNumber == WhitePlayer.ActorNumber)
            {
                return true;
            }
        }
        return false;
    }

    private void OnChessClicked(string boardIndexToPlace)
    {
        SendPlaceChessAckToMaster(boardIndexToPlace);
    }

    private void SendPlaceChessAckToMaster(string boardIndexToPlace)
    {
        pv.RPC("RpcReceivePlaceChessAck", RpcTarget.MasterClient, boardIndexToPlace);
    }
    [PunRPC]
    private void RpcReceivePlaceChessAck(string boardIndexToPlace, PhotonMessageInfo info)
    {
        // one round can only receive one ack
        Debug.Log("Place chess ack received");
        if (!placeChessAckReceived)
        {
            // TODO: double check this place is valid if needed
            // TODO: call everyone to load game data if needed
            // TODO: check every thing is valid if needed
            Debug.Log("Sending Place Chess Order to all");
            SendPlaceChessOrderToAll(boardIndexToPlace);
            placeChessAckReceived = true;
        }
    }

    private void SendPlaceChessOrderToAll(string boardIndexToPlace)
    {
        pv.RPC("RpcReceivePlaceChessOrder", RpcTarget.All, boardIndexToPlace);
    }
    [PunRPC]
    private void RpcReceivePlaceChessOrder(string boardIndexToPlace, PhotonMessageInfo info)
    {
        // TODO
        //reversiManager.PlaceChess(boardIndexToPlace);
    }

    public void CallMasterUploadGameData()
    {
        pv.RPC("RpcUploadGameData", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcUploadGameData(PhotonMessageInfo info)
    {
        if (PhotonNetwork.InRoom && roomManager)
        {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            if (reversiManager.chessesOnBoard != null)
            {
                for (int row = 1; row <= 8; row++)
                {
                    for (char col = 'A'; col <= 'H'; col++)
                    {
                        string boardIndex = row.ToString() + col;
                        propNeedToChange[boardIndex] = reversiManager.chessesOnBoard[boardIndex].currentState;
                    }
                }

                propNeedToChange["lastUploadGameDataTime"] = PhotonNetwork.Time;
            }
            roomManager.ChangeCustomProperties(propNeedToChange);
        }
    }

    private void CallMasterSomePlayerIsReady()
    {
        selfReady = true;
        pv.RPC("RpcSomePlayerIsReady", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcSomePlayerIsReady(PhotonMessageInfo info)
    {
        if (BlackPlayer != null && BlackPlayer.ActorNumber == info.Sender.ActorNumber)
        {
            blackReady = true;
            Debug.Log("Recieved black ready RPC");
        }
        if (WhitePlayer != null && WhitePlayer.ActorNumber == info.Sender.ActorNumber)
        {
            whiteReady = true;
            Debug.Log("Recieved white ready RPC");
        }
    }

    private void LoadChessesData()
    {
        StartCoroutine(LoadChessesDataCoroutine());
    }
    IEnumerator LoadChessesDataCoroutine()
    {
        if (isLoadingBoardData) yield break;
        isLoadingBoardData = true;

        CallMasterUploadGameData();
        double callUploadTime = PhotonNetwork.Time;
        Debug.Log(callUploadTime);
        while ((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"] < callUploadTime)
        {
            Debug.Log((double)PhotonNetwork.CurrentRoom.CustomProperties["lastUploadGameDataTime"]);
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var kvp in reversiManager.chessesOnBoard)
        {
            kvp.Value.CurrentState = (ReversiChess.State)PhotonNetwork.CurrentRoom.CustomProperties[kvp.Value.BoardIndex];
        }
        boardDataLoaded = true;
        Debug.Log("Board data loaded");

        isLoadingBoardData = false;
    }

    public void StartGame()
    {
        reversiManager.SpawnChesses(OnChessClicked, InitGame);
    }

    private void InitGame()
    {
        reversiManager.chessesOnBoard["4D"].CurrentState = ReversiChess.State.White;
        reversiManager.chessesOnBoard["4E"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5D"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5E"].CurrentState = ReversiChess.State.White;
        boardDataLoaded = true;
        CallMasterUploadGameData();
    }

    public Player BlackPlayer
    {
        get
        {
            if (PhotonNetwork.InRoom && roomManager)
            {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                if (actorId < 1) return null;
                if (roomManager.players.TryGetValue(actorId, out Player player))
                {
                    return player;
                }
                else return null;
            }
            else return null;
        }
    }

    public Player WhitePlayer
    {
        get
        {
            if (PhotonNetwork.InRoom && roomManager)
            {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                if (actorId < 1) return null;
                if (roomManager.players.TryGetValue(actorId, out Player player))
                {
                    return player;
                }
                else return null;
            }
            else return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        if (PhotonNetwork.InRoom && roomManager && PhotonNetwork.IsMasterClient)
        {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1)
            {
                propNeedToChange["blackActId"] = player.ActorNumber;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }

            else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1)
            {
                propNeedToChange["whiteActId"] = player.ActorNumber;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (PhotonNetwork.InRoom && roomManager && PhotonNetwork.IsMasterClient)
        {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == player.ActorNumber)
            {
                propNeedToChange["blackActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                blackReady = false;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == player.ActorNumber)
            {
                propNeedToChange["whiteActId"] = -1;
                propNeedToChange["gameState"] = GameState.Paused;
                whiteReady = false;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }
        }
    }
}
