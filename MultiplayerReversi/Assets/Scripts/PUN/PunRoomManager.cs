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
    public delegate bool Validation();
    public Validation ableToStartGame;

    private PhotonView pv;

    private void Awake() {
        pv = GetComponent<PhotonView>();
    }

    private void Start() {
        if (PhotonNetwork.InRoom) {
            UpdateUI();
            UpdatePlayerList();
            if (PhotonNetwork.IsMasterClient) {
                isMasterClient = true;
                onBecomeMasterClient.Invoke();
                PhotonHashtable propNeedToChange = new PhotonHashtable();
                propNeedToChange["currentState"] = State.Preparing;
                ChangeCustomProperties(propNeedToChange);
                onInitRoom.Invoke();
            } 
            else {
                if ((State)PhotonNetwork.CurrentRoom.CustomProperties["currentState"] == State.Playing) {
                    onEnterPlayingRoom.Invoke();
                }
            }
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
            propNeedToChange["currentState"] = State.Playing;
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
