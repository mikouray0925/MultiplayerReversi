using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BoolInquirer : MonoBehaviour
{
    [Header ("UI")]
    [SerializeField] private Text infoText;
    [SerializeField] private GameObject cancelButton;

    [Header ("Settings")]
    public bool deactivateAfterSubmit = false;
    public bool destroyAfterSubmit = false;

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

    public void Inquir(Callback _callback, string info = "", bool cancellable = true) {
        beforeInquir.Invoke();
        if (beforeInquirAction != null) beforeInquirAction.Invoke(gameObject);
        callback = _callback;
        if (infoText) infoText.text = info;
        if (cancelButton) cancelButton.SetActive(cancellable);
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
            if (deactivateAfterSubmit) gameObject.SetActive(false);
            if (destroyAfterSubmit) Destroy(gameObject);
        }
    }
}
