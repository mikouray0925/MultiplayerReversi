using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInquirer : MonoBehaviour
{
    public static UserInquirer instance {get; private set;}
    [SerializeField] StringInquirer stringInquirer;

    private void Awake() {
        instance = this;
    }
}
