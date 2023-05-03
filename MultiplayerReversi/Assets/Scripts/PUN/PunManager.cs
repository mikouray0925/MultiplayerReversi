using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks
{
    public static PunManager instance;
    public PunLobbyManager currentLobby;

    [Header ("Event")]
    public UnityEvent onStartConnecting;
    public UnityEvent onConnectedToMaster;
    public UnityEvent onDisconnected;
    public UnityEvent onJoinedLobby;

    public List<RoomInfo> currentRoomList {get; private set;} = new List<RoomInfo>();

    public bool isConnectedToMaster {get; private set;} = false;
    
    void Awake() {
        instance = this;
    }

    private void Update() {
        if (isConnectedToMaster && !PhotonNetwork.IsConnected) {
            onDisconnected.Invoke();
            isConnectedToMaster = false;
        }
    }

    public void Connect(string localPlayerName) {
        PhotonNetwork.LocalPlayer.NickName = localPlayerName;
        PhotonNetwork.ConnectUsingSettings();
        onStartConnecting.Invoke();
        Debug.Log("Start connecting");
    }

    public override void OnConnectedToMaster() {
        onConnectedToMaster.Invoke();
        isConnectedToMaster = true;
        Debug.Log("Connected to master");
    }

    public void JoinLobby() {
        if (PhotonNetwork.IsConnected && PhotonNetwork.JoinLobby()) {
            Debug.Log("Try joining lobby");
        }
    }

    public override void OnJoinedLobby() {
        onJoinedLobby.Invoke();
        Debug.Log("Joined lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        currentRoomList.Clear();
        foreach(var room in roomList) {
            if (room.PlayerCount > 0) currentRoomList.Add(room);
        }
    }
}
