using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class PunLobbyManager : MonoBehaviourPunCallbacks
{
    public UnityEvent onJoinedRoom;
    public UnityEvent onRoomListUpdate;
    public UnityEvent onDisconnected;

    public delegate bool StringValidation(string str);
    public StringValidation roomNameValidation;
    public StringValidation passwordValidation;

    public delegate string StringModifier(string str);
    public StringModifier roomNameModifier;
    public List<RoomInfo> currentRoomList {get; private set;} = new List<RoomInfo>();
    
    private void Awake() {
        PunManager.instance.currentLobby = this;
    }

    private void Start() {
        OnRoomListUpdate(PunManager.instance.roomListBuffer);
    }
    
    private void Update() {
        if (!PhotonNetwork.IsConnected) {
            onDisconnected.Invoke();
        }
    }

    private void OnDestroy() {
        PunManager.instance.currentLobby = null;
    }

    public struct CreateRoomInfo {
        public string roomName;
        public byte maxPlayerNum;
        public bool hasPassword;
        public string password;
    }

    public bool CreateRoom(CreateRoomInfo info) {
        if ( info.hasPassword && 
            (info.password.Length <= 0 ||
            (passwordValidation != null && !passwordValidation(info.password)))) {
            Debug.LogWarning("Invalid room password: " + info.password);
            return false;
        }

        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps["hasPassword"] = info.hasPassword;
        roomProps["password"] = info.password;

        RoomOptions options = new RoomOptions();
        options.CustomRoomProperties = roomProps;
        options.CustomRoomPropertiesForLobby = new string[] {"hasPassword", "password"};
        options.MaxPlayers = info.maxPlayerNum;

        return CreateRoom(info.roomName, options);
    }

    protected bool CreateRoom(string _roomName, RoomOptions options) {
        if (ProcessRoomName(_roomName, out string roomName)) {
            PhotonNetwork.CreateRoom(roomName, options);
            Debug.Log("Create room: " + roomName);
            return true;
        } 
        else {
            return false;
        }
    }

    public bool ProcessRoomName(string origin, out string result) {
        if (origin.Length > 0) {
            origin = origin.Trim();
            if (roomNameValidation != null && !roomNameValidation(origin)) {
                Debug.LogWarning("Invalid room name: " + origin);
                result = "Invalid room name";
                return false;
            } else {
                result = (roomNameModifier != null) ? roomNameModifier(origin) : origin;
                return true;
            }
        } 
        else {
            Debug.LogError("The len of room name cannot be zero.");
            result = "Invalid room name";
            return false;
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        currentRoomList.Clear();
        foreach(var room in roomList) {
            if (room.PlayerCount > 0) currentRoomList.Add(room);
        }
        onRoomListUpdate.Invoke();
    }

    public bool GetRoomInfo(string roomName, out RoomInfo roomInfo) {
        foreach (var room in currentRoomList) {
            if (room.Name == roomName) {
                roomInfo = room;
                return true;
            }
        }
        roomInfo = null;
        return false;
    }

    public bool JoinRoom(string roomName, string password) {
        if (PhotonNetwork.InRoom) {
            Debug.LogError("Joining room failed. Already in a room.");
            return false;
        }
        if (GetRoomInfo(roomName, out RoomInfo roomInfo)) {
            if (roomInfo.PlayerCount < roomInfo.MaxPlayers) {
                if ((bool)roomInfo.CustomProperties["hasPassword"]) {
                    if (password == (string)roomInfo.CustomProperties["password"]) {
                        return JoinRoom(roomName);
                    } else {
                        Debug.LogError("Wrong password.");
                        return false;
                    }
                } else {
                    return JoinRoom(roomName);
                }
            } else {
                Debug.LogError("The room is full." + roomName);
                return false;
            }
        } else {
            Debug.LogError("Cannot get info of room: " + roomName);
            return false;
        }
    }

    private bool JoinRoom(string roomName) {
        if (PhotonNetwork.JoinRoom(roomName)) {
            return true;
        } 
        else {
            Debug.LogError("Joining room failed.");
            return false;
        }
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room");
        onJoinedRoom.Invoke();
    }
}
