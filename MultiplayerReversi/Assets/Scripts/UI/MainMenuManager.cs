using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MikouPunBehavior
{
    public void PlayMultiplayerMode() {
        punManager.Connect();
    }
}
