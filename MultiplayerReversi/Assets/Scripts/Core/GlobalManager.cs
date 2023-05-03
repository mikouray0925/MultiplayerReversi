using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance {get; private set;}
        
    void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        Debug.LogError("Global manager is desroyed!!!");
    }
}


