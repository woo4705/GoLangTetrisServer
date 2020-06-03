using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using GameNetwork;

public class GameSceneManager : MonoBehaviour
{
    private InputField chatMsgInputField;
    private Text chattingLog;

    public static bool isGameCanStart { get; set; } = false;

    private GameNetworkServer gameServer;
    private ErrorMsgBox errorMsgBox;


    bool isGameStart = false;
    // Start is called before the first frame update
    void Start()
    {
        chatMsgInputField = GameObject.Find("ChatMsgInputField").GetComponent<InputField>();
        chattingLog = GameObject.Find("ChattingLog").GetComponent<Text>();
        chattingLog.text = "";

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

        if(isGameStart == false && GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.GAME )
        {
            GameObject.Find("GameStartButton").GetComponent<Button>().interactable = false;
            isGameStart = true;
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


    public void OnClickGameStartBtn()
    {

        if(GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.ROOM)
        {
            GameNetworkServer.Instance.SendGameStartPacket(new GameStartRequestPacket());
        }
        else
        {
            errorMsgBox.PopUpErrorMessage("게임시작 요청을 보낼 수 있는 상태가 아닙니다.");
        }
        
    }




}
