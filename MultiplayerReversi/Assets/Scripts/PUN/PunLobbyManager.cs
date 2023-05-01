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
    public UnityEvent onDisconnected;

    public delegate bool StringValidation(string str);
    public StringValidation roomNameValidation;
    public StringValidation passwordValidation;

    public delegate string StringModifier(string str);
    public StringModifier roomNameModifier;

    public List<GameObject> roomInfoListReceivers;
    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    
    private void Update() {
        if (!PhotonNetwork.IsConnected) {
            onDisconnected.Invoke();
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

    private bool CreateRoom(string _roomName, RoomOptions options) {
        if (ProcessRoomName(_roomName, out string roomName)) {
            PhotonNetwork.CreateRoom(roomName, options);
            Debug.Log("Create room: " + roomName);
            return true;
        } 
        else {
            return false;
        }
    }

    public bool CreateRoom(string _roomName, byte maxPlayerNum = 0, bool hasPassword = false, string password = "") {
        if ( password.Length <= 0 ||
            (passwordValidation != null && !passwordValidation(password))) {
            Debug.LogWarning("Invalid room password: " + password);
            return false;
        }

        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps["hasPassword"] = hasPassword;
        roomProps["password"] = password;

        RoomOptions options = new RoomOptions();
        options.CustomRoomProperties = roomProps;
        options.MaxPlayers = maxPlayerNum;

        return CreateRoom(_roomName, options);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        currentRoomList.Clear();
        foreach(var room in roomList) {
            if (room.PlayerCount > 0) currentRoomList.Add(room);
        }
        foreach (var receiver in roomInfoListReceivers) {
            receiver.SendMessage("OnReceiveRoomInfoList", currentRoomList);
        }
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
