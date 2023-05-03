using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunRoomManager : MonoBehaviourPunCallbacks
{
    private RoomOptions roomOptions;

    [Header ("Canvas")]
    [SerializeField] private GameObject roomInfoCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI ruleTimeText;
    private string[] ruleTimeOptions = {"5s","10s","30s","60s","Unlimited"}; 

    // Start is called before the first frame update
    void Start()
    {

        //get the room options of the current room scene
        PunManager.instance.currentRoom = this;
        if(PhotonNetwork.CurrentRoom == null){
            Debug.LogError("No room found");
        }
        roomOptions = new RoomOptions();
        roomOptions.CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        LoadPropertiesIntoCanvas();
    }

    private void LoadPropertiesIntoCanvas(){
        //load the room properties into the canvas
        ruleTimeText.text = "Time Limit:\n" + roomOptions.CustomRoomProperties["timeLimit"].ToString();
    }
    private void UpdatePropertiesToPun(){
        //update the room properties to pun
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomOptions.CustomRoomProperties);
    }
    public void TimeButtonCallback(bool isAdd){
        //get the current rule time
        int currentRuleTime = System.Array.IndexOf(ruleTimeOptions, roomOptions.CustomRoomProperties["timeLimit"].ToString());
        //if the button is add, add 1 to the current rule time
        if(isAdd){
            currentRuleTime++;
            //if the current rule time is out of range, set it to 0
            if(currentRuleTime >= ruleTimeOptions.Length){
                currentRuleTime = 0;
            }
        }
        //if the button is subtract, subtract 1 to the current rule time
        else{
            currentRuleTime--;
            //if the current rule time is out of range, set it to the last element
            if(currentRuleTime < 0){
                currentRuleTime = ruleTimeOptions.Length - 1;
            }
        }
        //set the rule time to the current rule time
        roomOptions.CustomRoomProperties["timeLimit"] = ruleTimeOptions[currentRuleTime];
        UpdatePropertiesToPun();
        LoadPropertiesIntoCanvas();
    }
}
