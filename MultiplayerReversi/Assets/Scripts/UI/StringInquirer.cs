using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StringInquirer : MonoBehaviour
{
    [Header ("UI")]
    [SerializeField] private Text infoText;
    [SerializeField] private InputField input;

    [Header ("Event")]
    [SerializeField] private UnityEvent beforeInquir;
    [SerializeField] private UnityEvent afterSubmit;

    bool isInquiring = false;

    public delegate void Callback(string s);
    Callback callback;
    public delegate string StringGetter();
    public StringGetter getterToReplace;

    public UnityAction<GameObject> beforeInquirAction;
    public UnityAction<GameObject> afterSubmitAction;

    public void Inquir(Callback _callback, string info = "") {
        beforeInquir.Invoke();
        if (beforeInquirAction != null) beforeInquirAction.Invoke(gameObject);
        if (infoText) infoText.text = info;
        callback = _callback;
        isInquiring = true;
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
            afterSubmit.Invoke();
            if (afterSubmitAction != null) afterSubmitAction.Invoke(gameObject);
        }
    }
}
