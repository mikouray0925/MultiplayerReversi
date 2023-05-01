using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class PunLobbyManager : MonoBehaviourPunCallbacks
{
    public UnityEvent onJoinedLobby;
    public UnityEvent onJoinedRoom;
    public UnityEvent onDisconnected;

    private void Start() {
        if (PhotonNetwork.IsConnected && PhotonNetwork.JoinLobby()) {
            Debug.Log("Try joining lobby");
        }
    }
    
    private void Update() {
        if (!PhotonNetwork.IsConnected) {
            onDisconnected.Invoke();
        }
    }

    public override void OnJoinedLobby() {
        onJoinedLobby.Invoke();
        Debug.Log("Joined lobby");
    }

    public bool ProcessRoomName(string origin, out string result) {
        if (origin.Length > 0) {
            result = origin.Trim();
            return true;
        } else {
            result = "new room";
            return false;
        }
        
    }

    public bool CreateRoom(string _roomName) {
        if (ProcessRoomName(_roomName, out string roomName)) {
            PhotonNetwork.CreateRoom(roomName);
            Debug.Log("Create room: " + roomName);
            return true;
        } else {
            Debug.LogWarning("Invalid room name: " + roomName);
            return false;
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room");
        onJoinedRoom.Invoke();
    }
}
