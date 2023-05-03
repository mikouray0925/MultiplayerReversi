using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class PunLobbyManager : MonoBehaviourPunCallbacks
{
    [Header ("Event")]
    public UnityEvent onJoinedRoom;
    public UnityEvent onRoomListUpdate;

    public delegate bool StringValidation(string str);
    public StringValidation roomNameValidation;
    public StringValidation passwordValidation;

    public delegate string StringModifier(string str);
    public StringModifier roomNameModifier;
    
    private void Awake() {
        PunManager.instance.currentLobby = this;
    }

    private void Start() {
        OnRoomListUpdate(PunManager.instance.currentRoomList);
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

    public List<RoomInfo> RoomList {
        get {
            return PunManager.instance.currentRoomList;
        }
        private set {}
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        onRoomListUpdate.Invoke();
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room");
        onJoinedRoom.Invoke();
    }

    public class JoinRoomHandler {
        RoomInfo room;
        public JoinRoomHandler(RoomInfo _room) {
            room = _room;
        }

        public void StartProcess() {
            if (PhotonNetwork.InRoom) {
                Debug.LogError("Joining room failed. Already in a room.");
                return;
            }
            if (room != null) {
                if (room.PlayerCount < room.MaxPlayers) {
                    if ((bool)room.CustomProperties["hasPassword"]) {
                        UserInquirer.instance.InquirString(JoinWithPassword, "Enter password");
                    } else {
                        UserInquirer.instance.InquirBool(Join, $"Join {room.Name}?");
                    }
                } else {
                    Debug.LogError("The room is full. " + room.Name);
                    return;
                }
            } 
        }

        private void JoinWithPassword(string password) {
            if (password == (string)room.CustomProperties["password"]) {
                Join(true);
            } else {
                Debug.LogError("Wrong password entered.");
            }
        }

        private void Join(bool confirm) {
            if (!confirm) return;
            if (PhotonNetwork.JoinRoom(room.Name)) {

            } else {
                Debug.LogError("Joining room failed.");
            }
        }
    }
}
