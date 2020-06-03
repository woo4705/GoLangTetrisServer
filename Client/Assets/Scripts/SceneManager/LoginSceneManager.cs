
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
    public static Int16 loginResult { get; set; } = (Int16)ERROR_CODE.DUMMY_CODE;
    GameObject ErrorMsgPopUp;


    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1920,1080,false);
        gameServer = GameNetworkServer.Instance;
        
        ErrorMsgPopUp = GameObject.Find("ErrorMsgPanel");
        
        
        if (ErrorMsgPopUp == null)
        {
            Debug.LogError("Panel Not Found!");
        }

        
        ErrorMsgPopUp.SetActive(false);

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
                PopUpErrorMessage("[로그인 오류] 오류코드:"+ loginResult);
                isLoginReqPktSended = false;
                
            }

        }

    }


    
    //아래는 Login버튼을 클릭하였을떄 호출되는 함수
    public void SendLoginRequest()
    {
        var InputID = (GameObject.Find("input_id_field")).GetComponent<InputField>().text;
        var InputPW = (GameObject.Find("input_pw_field")).GetComponent<InputField>().text;
        
        
        var request = new LoginReqPacket();
        request.SetValue(InputID, InputPW);
        var bodyData = request.ToBytes();

        if (gameServer.ClientStatus == GameNetworkServer.CLIENT_STATUS.NONE)
        {

            if (gameServer.GetIsConnected() == false)
            {
                GameNetworkServer.Instance.ConnectToServer();
            }

            GameNetworkServer.Instance.RequestLogin(InputID, InputPW); //PW를 dummy데이터로 설정하였음

            gameServer.PostSendPacket(PACKET_ID.LOGIN_REQ, bodyData);
        }

        isLoginReqPktSended = true;
        Debug.Log("LoginReqPacket sended");
        
        

    }
    
    public void PopUpErrorMessage(string message)
    {
        ErrorMsgPopUp.SetActive(true);
        Text ErrorMessage = GameObject.Find("ErrorMsgText").GetComponent<Text>();
        ErrorMessage.text = message;

    }
    
}
