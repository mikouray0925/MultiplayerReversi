using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PunRoomChatManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject chatTextbox;
    public GameObject chatMsgPrefab;
    public RectTransform chatMsgContent;
    public VerticalLayoutGroup chatMsgContentLayout;
    public LayoutElement chatMsgContentLayoutElement;
    public InputField chatInput;
    public Scrollbar chatScrollbar;
    private bool autoScroll = false;
    public bool isTextboxOpen = false;
    private double lastMsgSendTime = 0;
    private LinkedList<GameObject> chatMsgList = new LinkedList<GameObject>();
    public PhotonView pv;
    public static PunRoomChatManager chatManager;

    // Start is called before the first frame update
    private void Awake(){
        chatManager = this;
        //If android, default open
        #if UNITY_ANDROID
        isTextboxOpen = true;
        chatTextbox.SetActive(true);
        #endif
    }
    private void Update(){
        if(Input.GetKeyDown(KeyCode.T) && !chatInput.isFocused){
            isTextboxOpen = !isTextboxOpen;
            chatTextbox.SetActive(isTextboxOpen);
        }
    }
    private void FixedUpdate(){
        if(chatMsgContentLayout.preferredHeight > 240 && 
           (chatMsgContentLayout.preferredHeight + 5) != chatMsgContentLayoutElement.minHeight){
            chatMsgContentLayoutElement.minHeight = chatMsgContentLayout.preferredHeight + 5;
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatMsgContent);
            if(autoScroll) {
            chatScrollbar.value = -0.1f;
            autoScroll = false;
            }   
            Debug.Log("Rebuild layout");
        }
    }

    public void OnInputSubmit(string msg){
        if(chatInput.text != ""){
            //cooldown 250ms to prevent spam
            try{
                if(chatInput.text[0] == '/') {
                    string command = chatInput.text.Substring(1);
                    if(command == "ClearAllFuckingPlayerPrefs") AchievementManager.Instance.deleteAllPlayerPrefs();
                    chatInput.text = "";
                    return;
                }
                if(chatInput.text.Length > 256) throw new Exception("Message too long, please keep it under 256 characters\nOnly you can see this message");
                if(PhotonNetwork.Time - lastMsgSendTime < 0.25f) throw new Exception("You're sending messages too fast, please wait a bit\nOnly you can see this message");
                SendMsgToAll(chatInput.text);
                chatInput.text = "";
                lastMsgSendTime = PhotonNetwork.Time;
            }
            catch(Exception e){
                Text msgText = Instantiate(chatMsgPrefab, chatMsgContent).GetComponent<Text>();
                msgText.text = e.Message;
                chatMsgList.AddLast(msgText.gameObject);
                StartCoroutine(deleteWarningMessage(msgText.gameObject));
                while (e is not null) {
                    Debug.Log($"{e.Message} ({e.GetType().Name})");
                    e = e.InnerException;
                }
            }
        }
    }

    public void systemMessage(string msg){
        Text msgText = Instantiate(chatMsgPrefab, chatMsgContent).GetComponent<Text>();
        msgText.text = msg;
        chatMsgList.AddLast(msgText.gameObject);
        isTextboxOpen = true;
    }

    private IEnumerator deleteWarningMessage(GameObject msg){
        yield return new WaitForSeconds(5);
        chatMsgList.Remove(msg);
        Destroy(msg);
    }

    private void SendMsgToAll(string msg){
        pv.RPC("RpcSendMsg", RpcTarget.All, msg);
    }

    [PunRPC]
    private void RpcSendMsg(string msg, PhotonMessageInfo info){
        Debug.Log("Received msg from " + info.Sender.NickName + ": " + msg);
        autoScroll = (chatScrollbar.value < 0.1f);
        Text msgText = Instantiate(chatMsgPrefab, chatMsgContent).GetComponent<Text>();
        if(info.Sender.NickName != "") msgText.text = info.Sender.NickName + " : " + msg;
        else msgText.text = "Player " + info.Sender.ActorNumber + " : " + msg;
        chatMsgList.AddLast(msgText.gameObject);
        UpdateHeight();
    }

    private void UpdateHeight(){
        float height = 0;
        foreach(GameObject msg in chatMsgList){
            Rect _rect = msg.GetComponent<RectTransform>().rect;
            _rect.height = msg.GetComponent<LayoutElement>().preferredHeight;
            height += _rect.height;
            msg.GetComponent<RectTransform>().rect.Set(_rect.x, _rect.y, _rect.width, _rect.height);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatMsgContent);
    }
}
