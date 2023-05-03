using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PunSceneController : MonoBehaviourPunCallbacks
{
    [SerializeField] PunRoomManager room;
    private PhotonView pv;

    private void Awake() {
        pv = GetComponent<PhotonView>();
    }

    public void UpdateMasterSceneName() {
        if (room.isMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            propNeedToChange["masterSceneName"] = SceneManager.GetActiveScene().name;
            room.ChangeCustomProperties(propNeedToChange);
        }
    }

    public void CallChangeSceneToAll(string toSceneName) {
        pv.RPC("RpcChangeScene", RpcTarget.All, toSceneName);
    }

    [PunRPC]
    public void RpcChangeScene(string toSceneName, PhotonMessageInfo msg) {
        SceneController.instance.ChangeScene(toSceneName, UpdateMasterSceneName);
    }

    public void SyncSceneToMaster() {
        if (!room.isMasterClient) {
            string masterSceneName = (string)PhotonNetwork.CurrentRoom.CustomProperties["masterSceneName"];
            if (SceneManager.GetActiveScene().name != masterSceneName) {
                Debug.Log("Try to change to master scene: " + masterSceneName);
                SceneController.instance.ChangeScene(masterSceneName);
            }
        }   
    }
}
