using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PunRoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Text roomNameText;

    [Header ("Component")]
    public PunSceneController sceneController;

    [Header ("Event")]
    public UnityEvent onInitRoom;
    public UnityEvent onLeftRoom;
    public UnityEvent onPlayerListUpdate;
    public UnityEvent onPlayerEntered;
    public UnityEvent onPlayerLeft;
    public UnityEvent onBecomeMasterClient;
    public UnityEvent onNoLongerMasterClient;
    public UnityEvent onGameStarted;
    public UnityEvent onEnterPlayingRoom;

    public Dictionary<int, Player> players {get; private set;} = new Dictionary<int, Player>();
    public bool isMasterClient {get; private set;} = false;

    public enum State {
        Preparing,
        Playing
    }
    public State currentState {get; private set;} = State.Preparing;
    public delegate bool Validation();
    public Validation ableToStartGame;

    private PhotonView pv;

    private void Awake() {
        if (TryGetComponent<PhotonView>(out PhotonView _pv)) {
            pv = _pv;
        } else {
            pv = gameObject.AddComponent<PhotonView>();
        }
    }

    private void Start() {
        if (PhotonNetwork.InRoom) {
            UpdateUI();
            UpdatePlayerList();
            if (PhotonNetwork.IsMasterClient) {
                isMasterClient = true;
                onBecomeMasterClient.Invoke();
                PhotonHashtable propNeedToChange = new PhotonHashtable();
                propNeedToChange["roomState"] = State.Preparing;
                ChangeCustomProperties(propNeedToChange);
                onInitRoom.Invoke();
            } 
            else {
                if ((State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"] == State.Playing) {
                    Debug.Log("Enter playing room.");
                    onEnterPlayingRoom.Invoke();
                } else {
                    Debug.Log("Enter preparing room.");
                }
            }
        }
    }

    private void Update() {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            propNeedToChange["masterSceneName"] = SceneManager.GetActiveScene().name;
            propNeedToChange["roomState"] = currentState;
            ChangeCustomProperties(propNeedToChange);
        }
    }

    public void UpdateUI() {
        if (roomNameText) roomNameText.text = PhotonNetwork.CurrentRoom.Name;
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

    
    public override void OnMasterClientSwitched(Player newMasterClient) {
        if (!isMasterClient &&  PhotonNetwork.IsMasterClient) {
            onBecomeMasterClient.Invoke();
            isMasterClient = true;
        }
        if ( isMasterClient && !PhotonNetwork.IsMasterClient) {
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

    public void StartGame() {
        if ( ableToStartGame == null ||
            (ableToStartGame != null && ableToStartGame())) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            currentState = State.Playing;
            propNeedToChange["roomState"] = State.Playing;
            ChangeCustomProperties(propNeedToChange);
            onGameStarted.Invoke();
        }
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
