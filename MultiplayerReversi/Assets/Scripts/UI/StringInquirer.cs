using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StringInquirer : MonoBehaviour
{
    [SerializeField] private UnityEvent beforeInquir;
    [SerializeField] private UnityEvent afterSubmit;

    [SerializeField] private Text infoText;
    [SerializeField] private InputField input;
    
    bool isInquiring = false;

    public delegate void StringCallback(string s);
    StringCallback callback;
    public delegate string StringGetter();
    public StringGetter getterToReplace;

    public void Inquir(StringCallback _callback, string info = "") {
        beforeInquir.Invoke();
        if (infoText) infoText.text = info;
        callback = _callback;
    }   

    public void Submit() {
        if (isInquiring) {
            if (callback != null) {
                if (getterToReplace != null) {
                    callback(getterToReplace());
                }
                else if (input) {
                    callback(input.text);
                }
                callback = null;
            } 
            isInquiring = false;
        }
        afterSubmit.Invoke();
    }
}
