using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class PunManager : MonoBehaviourPunCallbacks
{
    public static PunManager instance;

    public UnityEvent onStartConnecting;
    public UnityEvent onConnectedToMaster;
    public UnityEvent onJoinedLobby;
    
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
}
