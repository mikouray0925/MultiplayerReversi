using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void PlayMultiplayerMode() {
        UserInquirer.instance.InquirString(PunManager.instance.Connect, "Enter name:");
    }
}
