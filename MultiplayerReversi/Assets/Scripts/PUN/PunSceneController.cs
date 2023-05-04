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
        if (TryGetComponent<PhotonView>(out PhotonView _pv)) {
            pv = _pv;
        } else {
            pv = gameObject.AddComponent<PhotonView>();
        }
    }

    public void UpdateMasterSceneName() {
        if (room.isMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();
            string masterScenename = SceneManager.GetActiveScene().name;
            propNeedToChange["masterSceneName"] = masterScenename;
            room.ChangeCustomProperties(propNeedToChange);
        }
    }

    public void CallChangeSceneToAll(string toSceneName) {
        pv.RPC("RpcChangeScene", RpcTarget.All, toSceneName, null);
    }

    public void CallChangeSceneToAll(string toSceneName, SceneController.Callback onSceneChanged) {
        pv.RPC("RpcChangeScene", RpcTarget.All, toSceneName, onSceneChanged);
    }

    [PunRPC]
    public void RpcChangeScene(string toSceneName, SceneController.Callback onSceneChanged, PhotonMessageInfo msg) {
        SceneController.Callback callback = UpdateMasterSceneName;
        if (onSceneChanged != null) callback += onSceneChanged;
        SceneController.instance.ChangeScene(toSceneName, callback);
    }

    public void SyncSceneToMaster() {
        if (!room.isMasterClient) {
            string masterSceneName = (string)PhotonNetwork.CurrentRoom.CustomProperties["masterSceneName"];
            if (SceneManager.GetActiveScene().name != masterSceneName) {
                Debug.Log("Try to change to master scene: " + masterSceneName);
                SceneController.instance.ChangeScene(masterSceneName);
            } else {
                Debug.Log("Already in master scene: " + masterSceneName);
            }
        }   
    }
}
