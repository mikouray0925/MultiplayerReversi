using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System.Text;

public class PunReversiManager : MonoBehaviourPunCallbacks
{
    public PunRoomManager roomManager;
    public ReversiManager reversiManager;

    public bool isLoadingBoardData {get; private set;}
    public bool boardDataLoaded {get; private set;}

    public enum GameState
    {
        Paused, WaitingForAllReady, WaitingForOrder, End
    }
    public GameState currentState = GameState.Paused;

    // only for master
    public bool blackReady {get; private set;} = false;
    public bool whiteReady {get; private set;} = false;
    public bool placeChessAckReceived {get; private set;} = false;
    public bool gameResultChecked {get; private set;} = false;

    public bool isHintUpdated {get; private set;} = false;
    
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
        if (BlackPlayer == null || WhitePlayer == null) gameState = GameState.Paused;
        if (gameState == GameState.Paused)
        {
            if (BlackPlayer != null && WhitePlayer != null)
            {
                currentState = GameState.WaitingForAllReady;
            }
            else
            {
                currentState = GameState.Paused;
                if(BlackPlayer == null) propNeedToChange["blackActId"] = -1;
                if(WhitePlayer == null) propNeedToChange["whiteActId"] = -1;
                FillEmptyPlayer(ref propNeedToChange);
            }
        }
        if (gameState == GameState.WaitingForAllReady)
        {
            if (BlackPlayer != null && blackReady &&
                WhitePlayer != null && whiteReady)
            {
                currentState = GameState.WaitingForOrder;
                gameResultChecked = false;
                placeChessAckReceived = false;
            }
        }
        if (gameState == GameState.WaitingForOrder)
        {
            blackReady = false;
            whiteReady = false;
            if (placeChessAckReceived && reversiManager.NoChessIsFlipping() && !gameResultChecked)
            {
                CallMasterUploadGameData();
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
                    Debug.Log("Change to side: " + reversiManager.currentSide);
                    currentState = GameState.WaitingForAllReady;
                }
                gameResultChecked = true;
            }
        }

        propNeedToChange["gameState"] = currentState;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        roomManager.ChangeCustomProperties(propNeedToChange);
        CallMasterUploadGameData();
    }

    private void DoNonMasterOnlyBusiness()
    {
        currentState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        reversiManager.currentSide = (ReversiManager.Side)PhotonNetwork.CurrentRoom.CustomProperties["currentSide"];
    }

    private void DoCommonBusiness()
    {
        //ReversiManager.Side currentSide = (ReversiManager.Side)PhotonNetwork.CurrentRoom.CustomProperties["currentSide"];
        if (currentState == GameState.Paused)
        {
            reversiManager.clearHints();
        }
        if (currentState == GameState.WaitingForAllReady)
        {
            if (reversiManager.chessesOnBoard == null)
            {
                reversiManager.SpawnChesses(OnChessClicked, LoadChessesData);
            }
            if (boardDataLoaded && reversiManager.NoChessIsFlipping()) CallMasterSomePlayerIsReady();
            isHintUpdated = false;
        }
        if (currentState == GameState.WaitingForOrder)
        {
            // TODO: Hint player where can be placed.
            if (!isHintUpdated && reversiManager.NoChessIsFlipping())
            {
                reversiManager.clearHints();
                Dictionary<string, List<string>> LegalMoves = reversiManager.FindLegalMoves(reversiManager.currentSide);
                Debug.Log(LegalMoves);
                reversiManager.showHints(LegalMoves);
                isHintUpdated = true;
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
                    if(reversiManager.PlaceChess(highlight.chess.boardIndex)){
                        Debug.Log("Place Chess result: Success");
                        OnChessClicked(highlight.chess.boardIndex);
                    }
                    
                }
            }
        }
    }

    private void FillEmptyPlayer(ref PhotonHashtable propNeedToChangeBuffer)
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
            if (reversiManager.currentSide == ReversiManager.Side.Black) {
                Debug.Log("It's black round.");
            }
            else {
                Debug.Log("It's white round.");
            }
            if (reversiManager.currentSide == ReversiManager.Side.Black &&
                BlackPlayer != null &&
                PhotonNetwork.LocalPlayer.ActorNumber == BlackPlayer.ActorNumber)
            {
                Debug.Log("It's my round.");
                return true;
            }
            if (reversiManager.currentSide == ReversiManager.Side.White &&
                WhitePlayer != null &&
                PhotonNetwork.LocalPlayer.ActorNumber == WhitePlayer.ActorNumber)
            {
                Debug.Log("It's my round.");
                return true;
            }
        }
        return false;
    }

    private void OnChessClicked(string boardIndexToPlace)
    {
        if (IsMyTurn()) SendPlaceChessAckToMaster(boardIndexToPlace);
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
            reversiManager.PlaceChess(boardIndexToPlace);
            SendPlaceChessOrderToOther(boardIndexToPlace, info.Sender);
            Debug.Log("Sending Place Chess Order to other");
            placeChessAckReceived = true;
        }
    }

    private void SendPlaceChessOrderToOther(string boardIndexToPlace, Player sender)
    {
        pv.RPC("RpcReceivePlaceChessOrder", RpcTarget.Others, boardIndexToPlace, sender);
    }
    [PunRPC]
    private void RpcReceivePlaceChessOrder(string boardIndexToPlace, Player sender, PhotonMessageInfo info)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber != sender.ActorNumber)
        {
            if (reversiManager.PlaceChess(boardIndexToPlace)) {
                Debug.Log("Place chess at " + boardIndexToPlace);
            }
            else {
                Debug.Log("Place chess failed");
                // maybe reload game data is needed
            }
        }
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
        pv.RPC("RpcSomePlayerIsReady", RpcTarget.MasterClient);
    }
    [PunRPC]
    private void RpcSomePlayerIsReady(PhotonMessageInfo info)
    {
        if (BlackPlayer != null && BlackPlayer.ActorNumber == info.Sender.ActorNumber)
        {
            blackReady = true;
            // Debug.Log("Recieved black ready RPC, Current state in prop: " + (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] + " time:" + PhotonNetwork.Time);
        }
        if (WhitePlayer != null && WhitePlayer.ActorNumber == info.Sender.ActorNumber)
        {
            whiteReady = true;
            // Debug.Log("Recieved white ready RPC, Current state in prop: " + (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] + " time:" + PhotonNetwork.Time);
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
        reversiManager.syncBoardwithLocalData();
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
}
