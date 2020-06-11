
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using GameNetwork;
using ERROR_CODE = GameNetwork.ERROR_CODE;

public class LoginSceneManager : MonoBehaviour
{

    private bool isLoginReqPktSended = false;
    private GameNetworkServer gameServer;
    private ErrorMsgBox errorMsgBox;
    public static Int16 loginResult { get; set; } = (Int16)ERROR_CODE.DUMMY_CODE;


    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1366,768,false);
        gameServer = GameNetworkServer.Instance;
        errorMsgBox = gameObject.AddComponent<ErrorMsgBox>();

        (GameObject.Find("input_ip_addr_field")).GetComponent<InputField>().text = "127.0.0.1";

        if (errorMsgBox != null)
        {
            Debug.Log("errorMsgBox Init");
            errorMsgBox.Init();
        }
        else
        {
            Debug.LogWarning("errorMsgBox is null");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isLoginReqPktSended == true )
        {
            
            if ( loginResult == (Int16) ERROR_CODE.NONE )
            {
                GameNetworkServer.Instance.ClientStatus = GameNetworkServer.CLIENT_STATUS.LOGIN;
                SceneManager.LoadScene("Lobby");
            }
            else if(loginResult != (Int16) ERROR_CODE.DUMMY_CODE )
            {
                
                errorMsgBox.PopUpErrorMessage("[로그인 오류] 오류코드:"+ loginResult);
                isLoginReqPktSended = false;

            }
            
        }

    }


    
    //아래는 Login버튼을 클릭하였을떄 호출되는 함수
    public void SendLoginRequest()
    {
        var InputID = (GameObject.Find("input_id_field")).GetComponent<InputField>().text;
       // var InputPW = (GameObject.Find("input_pw_field")).GetComponent<InputField>().text;
        var InputIPAddr = (GameObject.Find("input_ip_addr_field")).GetComponent<InputField>().text;

        GameNetworkServer.Instance.ipAddr = InputIPAddr;
        var request = new LoginReqPacket();
        request.SetValue(InputID, InputID);
        var bodyData = request.ToBytes();

        if (gameServer.ClientStatus == GameNetworkServer.CLIENT_STATUS.NONE)
        {

            if (gameServer.GetIsConnected() == false)
            {
                GameNetworkServer.Instance.ConnectToServer();
            }

            GameNetworkServer.Instance.RequestLogin(InputID, InputID);
        }

        isLoginReqPktSended = true;
        Debug.Log("LoginReqPacket sended");
        
    }


    public void CloseErrorMsg()
    {
        errorMsgBox.SetInactive();
       
        //사용자가 오류를 확인했으므로 에러창을 띄우게 하는 요인을 초기화시켜줌 
        loginResult = (Int16)ERROR_CODE.DUMMY_CODE;
    } 
    
    
}
