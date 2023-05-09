using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
public class Debugger : MonoBehaviour
{
    // Start is called before the first frame update
    public static Debugger instance {get; private set;}
    public bool isDebugging = false;
    public bool isCanvasOn = false;
    public GameObject canvas;

    public PunRoomManager roomManager;
    public PunReversiManager punReversiManager;
    public ReversiManager reversiManager;
    [SerializeField] private Text debugText;
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //when press F1, open canvas
        if(Input.GetKeyDown(KeyCode.F1)){
            isCanvasOn = !isCanvasOn;
            debugText.gameObject.SetActive(isCanvasOn);
        }
        if(isDebugging) {
            Debug.Log(getDebugMsg());
            isDebugging = false;
        }
        else if(isCanvasOn){
            debugText.text = getDebugMsg();
        }
    } 

    public string getDebugMsg(){
            //Print whole board
            StringBuilder sb = new StringBuilder();

            sb = sb.Append("Debug Message: \n");
            sb = sb.Append("PhotonNetwork.InRoom: ").Append(PhotonNetwork.InRoom).Append("\n");
            sb = sb.Append("PhotonNetwork.IsMasterClient: ").Append(PhotonNetwork.IsMasterClient).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.MasterClientId: ").Append(PhotonNetwork.CurrentRoom.MasterClientId).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom: ").Append(PhotonNetwork.CurrentRoom).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.CustomProperties: ").Append(PhotonNetwork.CurrentRoom.CustomProperties).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.Name: ").Append(PhotonNetwork.CurrentRoom.Name).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.PlayerCount: ").Append(PhotonNetwork.CurrentRoom.PlayerCount).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.MaxPlayers: ").Append(PhotonNetwork.CurrentRoom.MaxPlayers).Append("\n");
            sb = sb.Append("PhotonNetwork.CurrentRoom.IsOpen: ").Append(PhotonNetwork.CurrentRoom.IsOpen).Append("\n");

            if(roomManager.currentState != PunRoomManager.State.Playing) return sb.ToString();
            
            sb = sb.Append("\nBoard: \n");
            sb.Append("Board in prop:\n");
            for (int row = 1; row <= 8; row++)
            {
                for (char col = 'A'; col <= 'H'; col++)
                {
                    sb.Append((ReversiChess.State)PhotonNetwork.CurrentRoom.CustomProperties[row.ToString() + col]);
                    sb.Append(" ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            sb.Append("Board in local:\n");
            for (int row = 1; row <= 8; row++)
            {
                for (char col = 'A'; col <= 'H'; col++)
                {
                    sb.Append((ReversiChess.State)reversiManager.chessesOnBoard[row.ToString() + col].CurrentState);
                    sb.Append(" ");
                }
                sb.Append("\n");
            }
            sb = sb.Append("\nStates: \n");
            sb.Append("\n").Append("CurrentSide: ").Append(reversiManager.currentSide).Append("\n");
            sb.Append("BlackPlayer: ").Append(punReversiManager.BlackPlayer).Append("\n");
            sb.Append("WhitePlayer: ").Append(punReversiManager.WhitePlayer).Append("\n");
            sb.Append("isMasterClient: ").Append(PhotonNetwork.IsMasterClient).Append("\n");
            sb.Append("Master Client ID:").Append(PhotonNetwork.MasterClient).Append("\n");

            //get this client's side
            sb.Append("MySide: ").Append(PhotonNetwork.LocalPlayer).Append("\n");
            sb = sb.Append("Room State: ").Append((PunRoomManager.State)PhotonNetwork.CurrentRoom.CustomProperties["roomState"]).Append("\n");
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameState",out var s))
            sb = sb.Append("Game State: ").Append((PunRoomManager.GameState)s).Append("\n");
            sb = sb.Append("Black Ready: ").Append(punReversiManager.blackReady).Append("\n");
            sb = sb.Append("White Ready: ").Append(punReversiManager.whiteReady).Append("\n");


            sb = sb.Append("Place Chess Ack Received: ").Append(punReversiManager.placeChessAckReceived).Append("\n");
            sb = sb.Append("No Chess is Flipping: ").Append(reversiManager.NoChessIsFlipping()).Append("\n");
            //_ReversiManager.syncBoardwithLocalData();
            return sb.ToString();
    }
}
