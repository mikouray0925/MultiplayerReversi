using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomInfoUI : MonoBehaviour
{
    [Header ("Display")]    
    [SerializeField] Image icon;
    [SerializeField] Text roomName;
    [SerializeField] Text playerNum;
    [SerializeField] Image lockIcon;

    private RoomInfo currentRoom;

    public RoomInfo CurrentRoom {
        set {
            if (value != null) {
                roomName.text = value.Name;
                playerNum.text = $"({value.PlayerCount}/{value.MaxPlayers})";
                lockIcon.enabled = (bool)value.CustomProperties["hasPassword"];
                currentRoom = value;
            }
        }
        get {
            return currentRoom;
        }
    }

    public void StartJoinRoomInteraction() {
        PunLobbyManager.JoinRoomHandler joinRoomHandler = new PunLobbyManager.JoinRoomHandler(currentRoom);
        joinRoomHandler.StartProcess();
    }
}
