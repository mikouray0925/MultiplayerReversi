using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunRoomManager : MonoBehaviourPunCallbacks
{
    

    [Header("UI & Settings")]
    public Text roomNameText;
    private RoomOptions roomOptions;

    

    [Header ("Event")]
    public UnityEvent onLeftRoom;
    public UnityEvent onPlayerListUpdate;
    public UnityEvent onPlayerEntered;
    public UnityEvent onPlayerLeft;
    public UnityEvent<Player> onPlayerEntered_Player;
    public UnityEvent<Player> onPlayerLeft_Player;
    public UnityEvent onBecomeMasterClient;
    public UnityEvent onNoLongerMasterClient;
    public UnityEvent onGameStarted;

    public List<Player> playerList {get; private set;} = new List<Player>();
    public bool isMasterClient {get; private set;} = false;
    public delegate bool Validation();
    public Validation ableToStartGame;

    private void Start() {
        roomOptions = new RoomOptions();
        if (PhotonNetwork.InRoom) {
            UpdateUI();
            UpdatePlayerList();
            if (PhotonNetwork.IsMasterClient) {
                isMasterClient = true;
                onBecomeMasterClient.Invoke();
            }
        }
    }

    public void UpdatePlayerList() {
        playerList.Clear();
        if (PhotonNetwork.CurrentRoom != null) {
            foreach (var kvp in PhotonNetwork.CurrentRoom.Players) {
                playerList.Add(kvp.Value);
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
        onPlayerEntered_Player.Invoke(player);
    }
    
    public override void OnPlayerLeftRoom(Player player) {
        UpdatePlayerList();
        onPlayerLeft.Invoke();
        onPlayerLeft_Player.Invoke(player);
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
            onGameStarted.Invoke();
        }
    }



    // UI and Settings

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        foreach(var kvp in changedProps){
            roomOptions.CustomRoomProperties[kvp.Key] = kvp.Value;
        }
        UpdateUI();
    }

    private void UpdateUI(){
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }
}
