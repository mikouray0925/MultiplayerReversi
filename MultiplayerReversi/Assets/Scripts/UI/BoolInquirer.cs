using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BoolInquirer : MonoBehaviour
{
    [Header ("UI")]
    [SerializeField] private Text infoText;

    [Header ("Event")]
    [SerializeField] private UnityEvent beforeInquir;
    [SerializeField] private UnityEvent afterSubmit;

    bool isInquiring = false;

    public delegate void Callback(bool b);
    Callback callback;
    public delegate bool BoolGetter();
    public BoolGetter getterToReplace;

    public UnityAction<GameObject> beforeInquirAction;
    public UnityAction<GameObject> afterSubmitAction;

    public void Inquir(Callback _callback, string info = "") {
        beforeInquir.Invoke();
        if (beforeInquirAction != null) beforeInquirAction.Invoke(gameObject);
        if (infoText) infoText.text = info;
        callback = _callback;
        isInquiring = true;
    }   

    public void Submit(bool val) {
        if (isInquiring) {
            if (callback != null) {
                if (getterToReplace != null) {
                    callback(getterToReplace());
                }
                else {
                    callback(val);
                }
                callback = null;
            } 
            isInquiring = false;
            afterSubmit.Invoke();
            if (afterSubmitAction != null) afterSubmitAction.Invoke(gameObject);
        }
    }
}
