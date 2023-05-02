using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RoomCreatorUI : MonoBehaviour
{    
    public UnityEvent onCreateSuccess;
    public UnityEvent onCreateFailed;
    
    public InputField roomName;
    public Toggle hasPassword;
    public InputField password;

    public void Create() {
        PunLobbyManager.CreateRoomInfo info = new PunLobbyManager.CreateRoomInfo();
        info.roomName = roomName.text;
        info.hasPassword = hasPassword.isOn;
        info.password = password.text;
        info.maxPlayerNum = 2;
        if (PunManager.instance.currentLobby.CreateRoom(info)) {
            onCreateSuccess.Invoke();
        } else {
            onCreateFailed.Invoke();
        }
    }
}
