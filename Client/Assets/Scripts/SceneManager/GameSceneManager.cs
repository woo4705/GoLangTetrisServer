using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using GameNetwork;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private InputField chatMsgInputField;
    private Text chattingLog;

    public static bool isRemoteUserInRoom { get; set; } = false;
    public static bool isLocalUserInfoNeedUpdate { get; set; } = false;
    public static bool isRemoteUserInfoNeedUpdate { get;set;} =  false;
    
    public static bool isLocalReadyON_MsgArrived { get;set;} =  false;
    public static bool isRemoteReadyON_MsgArrived { get;set;} =  false;
    
    public static bool isLocalReadyOFF_MsgArrived { get;set;} =  false;
    public static bool isRemoteReadyOFF_MsgArrived { get;set;} =  false;
    public static RoomLeaveResPacket roomLeaveResPkt;
    

    private GameNetworkServer gameServer;
    private ErrorMsgBox errorMsgBox;


    bool isGameStart = false;
    // Start is called before the first frame update
    void Start()
    {
        chatMsgInputField = GameObject.Find("ChatMsgInputField").GetComponent<InputField>();
        chattingLog = GameObject.Find("ChattingLog").GetComponent<Text>();
        chattingLog.text = "";
        
        roomLeaveResPkt = new RoomLeaveResPacket();

        errorMsgBox = gameObject.AddComponent<ErrorMsgBox>();
        if (errorMsgBox != null)
        {
            errorMsgBox.Init();
        }
        else
        {
            Debug.LogWarning("errorMsgBox is null");
        }
        
        
        Debug.Log("start Game Scene");
        isLocalUserInfoNeedUpdate = true;

        UI_IsReadyLocalPlayer(false);
        UI_IsReadyRemotePlayer(false);

    }


    // Update is called once per frame
    void Update()
    {
        //채팅메세지 확인.
        if (GameNetworkServer.Instance.ChatMsgQueue.Count > 0)
        {
            RoomChatNotPacket recvMsg = GameNetworkServer.Instance.ChatMsgQueue.Dequeue();
            chattingLog.text += "[" + recvMsg.UserUniqueId + "] " + recvMsg.Message + "\n";
        }


        if (roomLeaveResPkt.Result != ERROR_CODE.DUMMY_CODE)
        {
            if (roomLeaveResPkt.Result == ERROR_CODE.NONE)
            {
                GameNetworkServer.Instance.ClientStatus = GameNetworkServer.CLIENT_STATUS.LOGIN;
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                errorMsgBox.PopUpErrorMessage("[방 나가기 요청 오류]"+roomLeaveResPkt.Result);
            }
        }
        
        

        if (isRemoteUserInRoom == false)
        {
            if (GameNetworkServer.Instance.RoomUserInfo.Count > 1)
            {
                Debug.Log("Room Full");
                isRemoteUserInfoNeedUpdate = true;
                isRemoteUserInRoom = true;
            }
        }
        
        if (isLocalUserInfoNeedUpdate )
        {
            GameNetworkServer.UserData userData=
                GameNetworkServer.Instance.RoomUserInfo[GameNetworkServer.Instance.Local_RoomUserUniqueID];
            UI_SetLocalPlayerInfo(userData.ID);
            isLocalUserInfoNeedUpdate = false;
        }
        
        if (isRemoteUserInfoNeedUpdate )
        {
            GameNetworkServer.UserData userData =
                GameNetworkServer.Instance.GetRemoteUserInfo();
            UI_SetRemotePlayerInfo(userData.ID);
            isRemoteUserInfoNeedUpdate = false;
        }


        if (isLocalReadyON_MsgArrived)
        {
            UI_IsReadyLocalPlayer(true);
            isLocalReadyON_MsgArrived = false;
        }
        if (isLocalReadyOFF_MsgArrived)
        {
            UI_IsReadyLocalPlayer(false);
            isLocalReadyOFF_MsgArrived = false;
        }
        
        if (isRemoteReadyON_MsgArrived)
        {
            UI_IsReadyRemotePlayer(true);
            isRemoteReadyON_MsgArrived = false;
        }
        if (isRemoteReadyOFF_MsgArrived)
        {
            UI_IsReadyRemotePlayer(false);
            isRemoteReadyOFF_MsgArrived = false;
        }
        

        if(isGameStart == false && GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.GAME )
        {
            GameObject.Find("ReadyButton").GetComponent<Button>().interactable = false;
            GameObject.Find("LeaveButton").GetComponent<Button>().interactable = false;
            UI_IsReadyLocalPlayer(false);
            UI_IsReadyRemotePlayer(false);
            
            isGameStart = true;
        }

    }
    
    
    

    
    
    public void UI_SetLocalPlayerInfo(string UserID)
    {
 
        GameObject[] LocalNameLabels = GameObject.FindGameObjectsWithTag("LocalPlayerName");

        foreach (GameObject TextLabelObject in  LocalNameLabels)
        {
            Text PlayerText = TextLabelObject.GetComponent<Text>();
            PlayerText.text = UserID;
        }
        
        Debug.Log("Player Name set to: " + UserID);
    }

    public void UI_SetRemotePlayerInfo(string UserID)
    {
       
       GameObject[] RemoteNameLabels = GameObject.FindGameObjectsWithTag("RemotePlayerName");
        foreach (GameObject TextLabelObject in RemoteNameLabels)
        {
            Text PlayerText = TextLabelObject.GetComponent<Text>();
            PlayerText.text = UserID;
        }
    }

    


     void UI_IsReadyLocalPlayer(bool is_on)
     {
         if (is_on)
         {
             (GameObject.FindGameObjectWithTag("LocalReadyLabel")).GetComponent<Text>().text = "READY";
         }
         else
         {
             (GameObject.FindGameObjectWithTag("LocalReadyLabel")).GetComponent<Text>().text = "";
         }
         
     }
     void UI_IsReadyRemotePlayer(bool is_on)
     {
         if (is_on)
         {
             (GameObject.FindGameObjectWithTag("RemoteReadyLabel")).GetComponent<Text>().text = "READY";
         }
         else
         {
             (GameObject.FindGameObjectWithTag("RemoteReadyLabel")).GetComponent<Text>().text = "";
         }
     }
    
     
    

     
     
     
    public void OnClickMsgSendButton()
    {
        string message="";
        if (chatMsgInputField != null) {
            message = chatMsgInputField.text;
        }
        if(message.Length <=0) {
            return;
        }

        if(message.Length > PacketDataValue.MAX_CHAT_SIZE)
        {
            message = message.Substring(PacketDataValue.MAX_CHAT_SIZE - 1);
        }
        GameNetworkServer.Instance.RequestChatMsg(message);
    }


    public void SendReadyRequest()
    {

        var request = new GameReadyRequestPacket();

        if (GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.ROOM)
        {

            if (GameNetworkServer.Instance.GetIsConnected() == false)
            {
                GameNetworkServer.Instance.ConnectToServer();
            }

            GameNetworkServer.Instance.SendGameReadyPacket(request);
        }

    }



    public void SendLeaveRequest()
    {

        if (GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.ROOM)
        {
            GameNetworkServer.Instance.RequestRoomLeave();

        }
    }



}
