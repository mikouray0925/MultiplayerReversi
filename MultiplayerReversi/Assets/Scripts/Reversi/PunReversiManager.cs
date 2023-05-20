using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private double LastSlowUpdateTime = 0.0;
    // only for master
    public bool blackReady {get; private set;} = false;
    public bool whiteReady {get; private set;} = false;
    public bool placeChessAckReceived {get; private set;} = false;
    public bool gameResultChecked {get; private set;} = false;

    // for all
    public bool isPlayerReadySent {get; private set;} = false;
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
        blackActIdCache = PhotonNetwork.LocalPlayer.ActorNumber;
        whiteActIdCache = -1;
        propNeedToChange["lastUploadGameDataTime"] = -1.0;

        for (int row = 1; row <= 8; row++)
        {
            for (char col = 'A'; col <= 'H'; col++)
            {
                propNeedToChange[row.ToString() + col] = ReversiChess.State.Unused;
            }
        }
        roomManager.ChangeCustomProperties(propNeedToChange);


        TweenManager.instance.TweenReset();
        if(PhotonNetwork.IsMasterClient){
            blackReady = false;
            whiteReady = false;
        }
    }

    private void FixedUpdate()
    {
        if(PhotonNetwork.Time - LastSlowUpdateTime > .2) {
            if(!PhotonNetwork.IsMasterClient){
                blackActIdCache = (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                whiteActIdCache = (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                if((PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Playing) {
                    currentState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
                }
            }
            LastSlowUpdateTime = PhotonNetwork.Time;
        }
        if (PhotonNetwork.InRoom && roomManager != null)
        {
            if(PhotonNetwork.IsMasterClient &&
            (PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Preparing ){
                FillEmptyPlayer();
                PhotonHashtable propNeedToChange = new PhotonHashtable();
                propNeedToChange["blackActId"] = blackActIdCache;
                propNeedToChange["whiteActId"] = whiteActIdCache;
                roomManager.ChangeCustomProperties(propNeedToChange);
            }
            if((PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == PunRoomManager.State.Playing){
                if(TweenManager.instance.isPaused && currentState != GameState.Paused) TweenManager.instance.PlayGameResumeAnimation();
                if (PhotonNetwork.IsMasterClient) DoMasterOnlyBusiness();
                else DoNonMasterOnlyBusiness();
                DoCommonBusiness();
            }
            
        }
    }

    private void DoMasterOnlyBusiness()
    {
        PhotonHashtable propNeedToChange = new PhotonHashtable();

        GameState gameState = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if ((BlackPlayer == null || WhitePlayer == null) && gameState != GameState.Paused) {
            Debug.Log("One of the player is null, game paused.");
            gameState = GameState.Paused;
            OnGamePlayToPaused();
        }
        if (gameState == GameState.Paused)
        {
            if (BlackPlayer != null && WhitePlayer != null)
            {
                OnPausedToWaitingForAllReady();
                currentState = GameState.WaitingForAllReady;
            }
            else
            {
                currentState = GameState.Paused;
                if(BlackPlayer == null) blackActIdCache = -1;
                if(WhitePlayer == null) whiteActIdCache = -1;
                FillEmptyPlayer();
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
                blackReady = false;
                whiteReady = false;
            }
        }
        if (gameState == GameState.WaitingForOrder)
        {
            if (placeChessAckReceived && reversiManager.NoChessIsFlipping() && !gameResultChecked)
            {
                gameResultChecked = true;
                CallMasterUploadGameData();
                ReversiManager.GameResult gameResult = reversiManager.GetGameResult(out ReversiManager.Side sideOfNextRound);
                if (gameResult == ReversiManager.GameResult.BlackWin)
                {
                    // TODO
                    currentState = GameState.End;
                    propNeedToChange["Winner"] = PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                    blackReady = false;
                    whiteReady = false;
                }
                else if (gameResult == ReversiManager.GameResult.WhiteWin)
                {
                    // TODO
                    currentState = GameState.End;
                    propNeedToChange["Winner"] = PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                    blackReady = false;
                    whiteReady = false;
                }
                else
                {
                    reversiManager.currentSide = sideOfNextRound;
                    currentState = GameState.WaitingForAllReady;
                }
            }
        }
        if(gameState == GameState.End){
            if (BlackPlayer != null && blackReady &&
                WhitePlayer != null && whiteReady)
            {
                currentState = GameState.Paused;
                roomManager.CallEndGameToAll();
            }
        }

        propNeedToChange["gameState"] = currentState;
        propNeedToChange["currentSide"] = reversiManager.currentSide;
        propNeedToChange["blackActId"] = blackActIdCache;
        propNeedToChange["whiteActId"] = whiteActIdCache;
        roomManager.ChangeCustomProperties(propNeedToChange);
        CallMasterUploadGameData();
    }

    private void DoNonMasterOnlyBusiness()
    {
        if((currentState == GameState.WaitingForAllReady || currentState == GameState.WaitingForOrder)
         && (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] == GameState.Paused)
        {
           OnGamePlayToPaused();
        }
        if(currentState == GameState.Paused && (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] != GameState.Paused)
        {
            OnPausedToWaitingForAllReady();
        }
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
            if (boardDataLoaded && reversiManager.NoChessIsFlipping() && !isPlayerReadySent) {
                CallMasterSomePlayerIsReady();
            }
            isHintUpdated = false;
        }
        if (currentState == GameState.WaitingForOrder)
        {
            // TODO: Hint player where can be placed.
            if (!isHintUpdated && reversiManager.NoChessIsFlipping())
            {
                isHintUpdated = true;

                bool _err = false;
                foreach (var kvp in reversiManager.chessesOnBoard)
                {
                    if((ReversiChess.State)PhotonNetwork.CurrentRoom.CustomProperties[kvp.Key] != kvp.Value.currentState)
                    {
                        _err = true;
                    }
                }
                if(_err)
                {
                    Debug.LogError("Chess data not match!");
                    Debugger.instance.isDebugging = true;
                }

                reversiManager.clearHints();
                Dictionary<string, List<string>> LegalMoves = reversiManager.FindLegalMoves(reversiManager.currentSide);
                reversiManager.showHints(LegalMoves);
                Debugger.instance.readyFakePlaceChess = true;
            }
            isPlayerReadySent = false;
        }
        if (currentState == GameState.End)
        {
            reversiManager.clearHints();
            if(PhotonNetwork.LocalPlayer.ActorNumber == (int)PhotonNetwork.CurrentRoom.CustomProperties["Winner"])
            {
                TweenManager.instance.PlayVictoryAnimation();
                AchievementHandler.HandleEndGame(reversiManager.chessesOnBoard, AchievementHandler.GameResult.Win, GetPlayerSide(PhotonNetwork.LocalPlayer.ActorNumber));
            }
            else
            {
                TweenManager.instance.PlayDefeatAnimation();
            }
            if(!isPlayerReadySent){
                CallMasterSomePlayerIsReady();
                isPlayerReadySent = true;
            } 
        }
    }

    void OnPausedToWaitingForAllReady()
    {
        TweenManager.instance.PlayGameResumeAnimation();
    }

    void OnGamePlayToPaused()
    {
        if(TweenManager.instance) Debug.Log(TweenManager.instance);
        else Debug.Log("TweenManager.instance is null");
        TweenManager.instance.PlayGamePauseAnimation();
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
                        AchievementHandler.HandlePlaceChess(reversiManager.lastFoundLegalMoves[highlight.chess.boardIndex]);
                        Debug.Log("Flanked Chess: " + reversiManager.lastFoundLegalMoves[highlight.chess.boardIndex].Count);
                        OnChessClicked(highlight.chess.boardIndex);
                    }
                    
                }
            }
        }
    }

    private void FillEmptyPlayer()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            if (blackActIdCache == -1)
            {
                foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
                {
                    if (kvp.Value.ActorNumber != whiteActIdCache)
                    {
                        blackActIdCache = kvp.Value.ActorNumber;
                        break;
                    }
                }
                
            }
            else if (whiteActIdCache == -1)
            {
                foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
                {
                    if (kvp.Value.ActorNumber != blackActIdCache)
                    {
                        whiteActIdCache = kvp.Value.ActorNumber;
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
            if(GetPlayerSide(PhotonNetwork.LocalPlayer.ActorNumber) == reversiManager.currentSide)
            {
                return true;
            }
            else return false;
        }
        return false;
    }

    private void OnChessClicked(string boardIndexToPlace)
    {
        if (IsMyTurn()) SendPlaceChessAckToMaster(boardIndexToPlace);
    }

    public void DebugFakePlaceChess()
    {
        //get random key from last found legal moves
        string key = reversiManager.lastFoundLegalMoves.ElementAt(Random.Range(0, reversiManager.lastFoundLegalMoves.Count)).Key;
        Highlight highlight = reversiManager.chessesOnBoard[key].hint;
        if(reversiManager.PlaceChess(highlight.chess.boardIndex)){
            Debug.Log("Fake Player Place Chess result: Success");
            OnChessClicked(highlight.chess.boardIndex);
        }
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
        //Debug.Log("Called master some player is ready");
        isPlayerReadySent = true;
    }
    [PunRPC]
    private void RpcSomePlayerIsReady(PhotonMessageInfo info)
    {
        if(currentState == GameState.WaitingForAllReady){
            if (BlackPlayer != null && BlackPlayer.ActorNumber == info.Sender.ActorNumber)
            {
                blackReady = true;
                Debug.Log("Recieved black ready RPC, Current state in prop: " + (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] + " time:" + PhotonNetwork.Time);
            }
            if (WhitePlayer != null && WhitePlayer.ActorNumber == info.Sender.ActorNumber)
            {
                whiteReady = true;
                Debug.Log("Recieved white ready RPC, Current state in prop: " + (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] + " time:" + PhotonNetwork.Time);
            }
        }
        else Debug.Log("Recieved ready RPC, Current state in prop: " + (GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"] + " time:" + PhotonNetwork.Time);
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
        reversiManager.ClearAllChesses();
        reversiManager.chessesOnBoard["4D"].CurrentState = ReversiChess.State.White;
        reversiManager.chessesOnBoard["4E"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5D"].CurrentState = ReversiChess.State.Black;
        reversiManager.chessesOnBoard["5E"].CurrentState = ReversiChess.State.White;
        boardDataLoaded = true;
        CallMasterUploadGameData();
    }

    public int blackActIdCache, whiteActIdCache;

    public Player BlackPlayer
    {
        get
        {
            if (PhotonNetwork.InRoom && roomManager)
            {
                if (blackActIdCache < 1) return null;
                if (roomManager.players.TryGetValue(blackActIdCache, out Player player))
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
                if (whiteActIdCache < 1) return null;
                if (roomManager.players.TryGetValue(whiteActIdCache, out Player player))
                {
                    return player;
                }
                else return null;
            }
            else return null;
        }
    }

    public ReversiManager.Side GetPlayerSide(int actId)
    {
        if (actId == blackActIdCache) return ReversiManager.Side.Black;
        else if (actId == whiteActIdCache) return ReversiManager.Side.White;
        else return ReversiManager.Side.Error;
    }

    public void SwitchPlayerSide(){
        StringBuilder sb = new StringBuilder();
        sb.Append("Switch Player Side Callback activated, Timestamp: " + PhotonNetwork.Time + "\n");
        if(PhotonNetwork.IsMasterClient){
            sb.Append("Passed Master Client Check\n");
            int temp = blackActIdCache;
            blackActIdCache = whiteActIdCache;
            whiteActIdCache = temp;
            sb.Append("New blackActId: " + whiteActIdCache + "\n");
            sb.Append("New whiteActId: " + temp + "\n");
        }
        Debug.Log(sb.ToString());
    }

    public void OnBecomeMasterClient(){
        //Fetch original data
        reversiManager.currentSide = (ReversiManager.Side)PhotonNetwork.CurrentRoom.CustomProperties["currentSide"];
        if(reversiManager.currentSide != ReversiManager.Side.Black && reversiManager.currentSide != ReversiManager.Side.White){
            reversiManager.currentSide = ReversiManager.Side.Black;
        }
    }
}
