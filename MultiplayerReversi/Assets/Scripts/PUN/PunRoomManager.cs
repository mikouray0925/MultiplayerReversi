using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class PunRoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject roomUI;
    public Text roomNameText;
    public Text blackPlayerNameText;
    public Text whitePlayerNameText;
    public Text blackIsHostText;
    public Text whiteIsHostText;
    [Header("Board Asset")]
    public int BoardTexNumber = 0;
    public GameObject BoardTex;
    public GameObject Board;
    public Image BoardImageDemo;
    public Material BoardMaterialTransparent;
    public Material BoardMaterialDefault;
    public Sprite BoardDefault;
    public GameObject ButtonLeft;
    public GameObject ButtonRight;
    [Header ("Component")]
    public PunSceneController sceneController;
    public PunRoomChatManager chatManager;
    //ONLY get component after initial, don't set reference in inspector
    private PunReversiManager reversiManager;

    [Header ("Event")]
    public UnityEvent onInitRoom;
    public UnityEvent onLeftRoom;
    public UnityEvent onPlayerListUpdate;
    public UnityEvent onPlayerEntered;
    public UnityEvent onPlayerLeft;
    public UnityEvent onBecomeMasterClient;
    public UnityEvent onNoLongerMasterClient;
    public UnityEvent onMasterStartGame;
    public UnityEvent onGameStarted;
    public UnityEvent onEnterPlayingRoom;
    public UnityEvent onReturnToRoom;

    public Dictionary<int, Player> players {get; private set;} = new Dictionary<int, Player>();
    public bool isMasterClient {get; private set;} = false;

    public enum State {
        Preparing,
        Playing
    }
    public enum GameState {
        Paused, WaitingForAllReady, WaitingForOrder, End
    }
    public State currentState {get; private set;} = State.Preparing;
    
    public delegate bool Validation();
    public Validation ableToStartGame;
    public PhotonView pv;
    public bool isAnotherGame = false;
    private void Awake() {
        if (TryGetComponent<PhotonView>(out PhotonView _pv)) {
            pv = _pv;
        } else {
            pv = gameObject.AddComponent<PhotonView>();
        }
        chatManager.pv = pv;
        BoardTexNumber = AchievementManager.BoardTexId;
    }

    private void Start() {
        if (PhotonNetwork.InRoom) {
            if (PhotonNetwork.IsMasterClient) {
                isMasterClient = true;
                onBecomeMasterClient.Invoke();
                PhotonHashtable propNeedToChange = new PhotonHashtable();
                propNeedToChange["roomState"] = State.Preparing;
                ChangeCustomProperties(propNeedToChange);
                onInitRoom.Invoke();
            } 
            else {
                currentState = (State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"];
                if (currentState == State.Preparing) {
                    Debug.Log("Enter preparing room.");
                } 
                else if (currentState == State.Playing) {
                    Debug.Log("Enter playing room.");
                    onEnterPlayingRoom.Invoke();
                }
            }

            reversiManager = GetComponent<PunReversiManager>();
            UpdatePlayerList();
            UpdateUI();
            BoardTexButtonCallback(0);
        }
    }

    
    private void FixedUpdate() {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) {
            if (PhotonNetwork.IsMasterClient) {
                PhotonHashtable propNeedToChange = new PhotonHashtable();
                propNeedToChange["masterSceneName"] = SceneManager.GetActiveScene().name;
                propNeedToChange["roomState"] = currentState;
                ChangeCustomProperties(propNeedToChange);
            } 
            else {
                currentState = (State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"];
            }
        }
        UpdateUI();
    }

    public void UpdateUI() {
        // If UI is active then update
        if (roomUI.activeSelf) {
            roomNameText.text = PhotonNetwork.CurrentRoom.Name;

            if(reversiManager.BlackPlayer != null) {
                if(reversiManager.BlackPlayer.NickName != "")
                    blackPlayerNameText.text = reversiManager.BlackPlayer.NickName;
                else blackPlayerNameText.text = "Player " + reversiManager.BlackPlayer.ActorNumber.ToString();
                if(reversiManager.BlackPlayer.IsLocal)  blackPlayerNameText.text += " (You)";

                if(PhotonNetwork.MasterClient.ActorNumber == reversiManager.BlackPlayer.ActorNumber) {
                    blackIsHostText.text = "Host";
                }
                else blackIsHostText.text = "";

            }
            else {
                blackPlayerNameText.text = "Waiting...";
                blackIsHostText.text = "";
            }

            if(reversiManager.WhitePlayer != null) {
                if(reversiManager.WhitePlayer.NickName != "")
                    whitePlayerNameText.text = reversiManager.WhitePlayer.NickName;
                else whitePlayerNameText.text = "Player " + reversiManager.WhitePlayer.ActorNumber.ToString();
                if(reversiManager.WhitePlayer.IsLocal)  whitePlayerNameText.text += " (You)";

                if(PhotonNetwork.MasterClient.ActorNumber == reversiManager.WhitePlayer.ActorNumber) {
                    whiteIsHostText.text = "Host";
                }
                else whiteIsHostText.text = "";
            }
            else {
                whitePlayerNameText.text = "Waiting...";
                whiteIsHostText.text = "";
            }
        }
    }

    public void BoardTexButtonCallback(int i){
        // If MaterialHolder is not loaded, use default
        if(!MaterialHolder.instance.isLoadSuccess) {
            BoardImageDemo.sprite = BoardDefault;
            BoardTex.SetActive(false);
            Board.GetComponent<MeshRenderer>().material = BoardMaterialDefault;

            ButtonLeft.SetActive(false);
            ButtonRight.SetActive(false);
            return;
        }
        BoardTexNumber += i;
        if(BoardTexNumber < 0) BoardTexNumber = 3;
        if(BoardTexNumber > 3) BoardTexNumber = 0;
        AchievementManager.BoardTexId = BoardTexNumber;

        ButtonLeft.SetActive(true);
        ButtonRight.SetActive(true);

        if(BoardTexNumber == 0) {
            BoardImageDemo.sprite = BoardDefault;
            BoardTex.SetActive(false);
            Board.GetComponent<MeshRenderer>().material = BoardMaterialDefault;
            return;
        }

        BoardTex.SetActive(true);
        Board.GetComponent<MeshRenderer>().material = BoardMaterialTransparent;

        if(MaterialHolder.instance.getSprite(BoardTexNumber - 1,out Sprite sprite)){
            BoardImageDemo.sprite = sprite;
            BoardImageDemo.gameObject.SetActive(true);
            ButtonLeft.SetActive(true);
            ButtonRight.SetActive(true);
        }
        if(MaterialHolder.instance.getMaterial(BoardTexNumber - 1,out Material material)){
            BoardTex.SetActive(true);
            BoardTex.GetComponent<MeshRenderer>().material = material;
            Board.GetComponent<MeshRenderer>().material = BoardMaterialTransparent;
        }
    }
    public void UpdatePlayerList() {
        players.Clear();
        if (PhotonNetwork.CurrentRoom != null) {
            foreach (var kvp in PhotonNetwork.CurrentRoom.Players) {
                players.Add(kvp.Key, kvp.Value);
            }
        }
        onPlayerListUpdate.Invoke();
    }
    public void SwitchMasterClient() {
        if (PhotonNetwork.IsMasterClient) {
            if(players.Count>1) PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerListOthers[0]);
        }
    }    
    public override void OnMasterClientSwitched(Player newMasterClient) {
        if (!isMasterClient &&  PhotonNetwork.IsMasterClient) {
            onBecomeMasterClient.Invoke();
            isMasterClient = true;

            reversiManager.OnBecomeMasterClient();
        }
        if (isMasterClient && !PhotonNetwork.IsMasterClient) {
            onNoLongerMasterClient.Invoke();
            isMasterClient = false;
        }
    }
    public override void OnPlayerEnteredRoom(Player player) {
        UpdatePlayerList();
        onPlayerEntered.Invoke();
    }
    
    public override void OnPlayerLeftRoom(Player player) {
        UpdatePlayerList();
        onPlayerLeft.Invoke();
    }

    public void LeaveRoom() {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() {
        onLeftRoom.Invoke();
    }

    public void CallStartGameToAll() {
        if (!PhotonNetwork.IsMasterClient) return;
        if ( ableToStartGame == null ||
            (ableToStartGame != null && ableToStartGame())) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            currentState = State.Playing;
            propNeedToChange["roomState"] = State.Playing;
            ChangeCustomProperties(propNeedToChange);
            pv.RPC("RpcStartGame", RpcTarget.All);
            onMasterStartGame.Invoke();
        }
    }

    
    [PunRPC]
    private void RpcStartGame(PhotonMessageInfo info) {
        currentState = State.Playing;
        if(isAnotherGame) reversiManager.NewGameInitialize();
        BoardTexButtonCallback(0);
        onGameStarted.Invoke();
    }
    
    [PunRPC]
    private void RpcReturnToRoom(PhotonMessageInfo info) {
        currentState = State.Preparing;
        BoardTex.SetActive(false);
        Board.GetComponent<Renderer>().material = BoardMaterialDefault;
        reversiManager.EndGameCleanUp();
        isAnotherGame = true;
        if (PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            propNeedToChange["roomState"] = currentState;
            ChangeCustomProperties(propNeedToChange);
        }
        onReturnToRoom.Invoke();
    }

    public void CallReturnToRoomToAll() {
        pv.RPC("RpcReturnToRoom", RpcTarget.All);
    }

    [PunRPC]
    private void RpcEndGame(PhotonMessageInfo info) {
        currentState = State.Preparing;
        reversiManager.currentState = PunReversiManager.GameState.Paused;
        //TODO add EndGame event
        //Achievement, Record, etc.
        //onGameEnded.Invoke();
    }

    public void CallEndGameToAll() {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        currentState = State.Preparing;
        propNeedToChange["roomState"] = currentState;
        ChangeCustomProperties(propNeedToChange);
        pv.RPC("RpcEndGame", RpcTarget.All);
        //TODO: add onMasterEndGame event
        //onMasterEndGame.Invoke();
    }

    public void ChangeCustomProperties(PhotonHashtable propertiesNeedToChange) {
        if (isMasterClient) {
            PhotonHashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (var kvp in propertiesNeedToChange) {
                properties[kvp.Key] = kvp.Value;
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
    }

    
}
