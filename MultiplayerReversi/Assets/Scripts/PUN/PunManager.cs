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
    public PunRoomManager currentRoom;

    [Header ("Event")]
    public UnityEvent onStartConnecting;
    public UnityEvent onConnectedToMaster;
    public UnityEvent onJoinedLobby;

    public List<RoomInfo> currentRoomList {get; private set;} = new List<RoomInfo>();
    
    void Awake() {
        instance = this;
    }
    public void Connect() {
        PhotonNetwork.ConnectUsingSettings();
        onStartConnecting.Invoke();
        Debug.Log("Start connecting");
    }

    public override void OnConnectedToMaster() {
        onConnectedToMaster.Invoke();
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
