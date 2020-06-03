using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMsgBox : MonoBehaviour
{
    GameObject ErrorMsgPopUp;
    

    public void Init()
    {
        ErrorMsgPopUp = GameObject.Find("ErrorMsgPanel");

        if (ErrorMsgPopUp == null)
        {
            Debug.LogError("Panel Not Found!");
        }
        
        ErrorMsgPopUp.SetActive(false);

    }

    public void PopUpErrorMessage(string message)
    {
        ErrorMsgPopUp.SetActive(true);
        Text ErrorMessage = GameObject.Find("ErrorMsgText").GetComponent<Text>();
        ErrorMessage.text = message;

    }


    public void SetInactive()
    {
       ErrorMsgPopUp.SetActive(false);
    }

}
