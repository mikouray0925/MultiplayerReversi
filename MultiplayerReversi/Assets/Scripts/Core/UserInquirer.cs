using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInquirer : MonoBehaviour
{
    public static UserInquirer instance {get; private set;}
    [SerializeField] RectTransform inquirerCanvas;
    [SerializeField] GameObject stringInquirerPrefab;
    [SerializeField] GameObject boolInquirerPrefab;

    private void Awake() {
        instance = this;
    }

    public bool InquirString(StringInquirer.Callback callback, string info = "") {
        StringInquirer inquirer = Instantiate(stringInquirerPrefab, inquirerCanvas).GetComponent<StringInquirer>();
        if (inquirer) {
            inquirer.afterSubmitAction += Destroy;
            inquirer.Inquir(callback, info);
            return true;
        } else {
            return false;
        }
    }

    public bool InquirBool(BoolInquirer.Callback callback, string info = "") {
        BoolInquirer inquirer = Instantiate(boolInquirerPrefab, inquirerCanvas).GetComponent<BoolInquirer>();
        if (inquirer) {
            inquirer.afterSubmitAction += Destroy;
            inquirer.Inquir(callback, info);
            return true;
        } else {
            return false;
        }
    }
}
