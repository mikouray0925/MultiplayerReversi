using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PunReversiManager : MonoBehaviourPunCallbacks
{
    public PunRoomManager inRoom;

    public enum GameState {
        Paused,
        BlackRound,
        WhiteRound,
        Waiting,
        End
    }

    public void Initialize() {
        PhotonHashtable propNeedToChange = new PhotonHashtable();
        propNeedToChange["gameState"] = GameState.Paused;
        propNeedToChange["blackActId"] = PhotonNetwork.LocalPlayer.ActorNumber;
        propNeedToChange["whiteActId"] = -1;

        for (int row = 1; row <= 8; row++) {
            for (char col = 'A'; col <= 'H'; col++) {
                propNeedToChange[row.ToString() + col] = ReversiChess.State.Unuse;
            }
        }

        inRoom.ChangeCustomProperties(propNeedToChange);
    }

    public Player BlackPlayer {
        get {
            if (PhotonNetwork.InRoom && inRoom) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"];
                if (actorId != -1) {
                    return inRoom.players[actorId];
                } else return null;
            } else return null;
        }
    }

    public Player WhitePlayer {
        get {
            if (PhotonNetwork.InRoom && inRoom) {
                int actorId = (int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"];
                if (actorId != -1) {
                    return inRoom.players[actorId];
                } else return null;
            } else return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player player) {
        if (PhotonNetwork.InRoom && inRoom && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == -1) {
                propNeedToChange["blackActId"] = player.ActorNumber;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }

            else if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == -1) {
                propNeedToChange["whiteActId"] = player.ActorNumber;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player player) {
        if (PhotonNetwork.InRoom && inRoom && PhotonNetwork.IsMasterClient) {
            PhotonHashtable propNeedToChange = new PhotonHashtable();

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["blackActId"] == player.ActorNumber) {
                propNeedToChange["blackActId"] = -1;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["whiteActId"] == player.ActorNumber) {
                propNeedToChange["whiteActId"] = -1;
                inRoom.ChangeCustomProperties(propNeedToChange);
            }
        }
    }
}
