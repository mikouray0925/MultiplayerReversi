using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class RoomListUI : MonoBehaviour
{
    [SerializeField] GameObject roomInfoPrefab;
    [SerializeField] RectTransform contentRect;
    [SerializeField] float heightPerRoom;

    LinkedList<RoomInfoUI> roomInfoList = new LinkedList<RoomInfoUI>();
    
    public void UpdateRoomInfoList() {
        List<RoomInfo> roomList = PunManager.instance.currentRoomList;
        while (roomInfoList.Count < roomList.Count) {
            roomInfoList.AddLast(Instantiate(roomInfoPrefab, contentRect).GetComponent<RoomInfoUI>());
        }
        while (roomInfoList.Count > roomList.Count) {
            RoomInfoUI removedRoomInfoUI = roomInfoList.Last.Value;
            roomInfoList.RemoveLast();
            Destroy(removedRoomInfoUI.gameObject);
        }

        int i = 0;
        foreach (var roomInfoUI in roomInfoList) {
            roomInfoUI.CurrentRoom = roomList[i];
            i++;
        }

        if (contentRect) {
            Rect rect = contentRect.rect;
            rect.height = roomList.Count * heightPerRoom;
            contentRect.sizeDelta = rect.size;
        }
    }
}
