using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void PlayMultiplayerMode() {
        PunManager.instance.Connect();
    }
}