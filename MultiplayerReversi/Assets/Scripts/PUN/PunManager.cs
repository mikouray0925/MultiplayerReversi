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
}
