using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameNetwork;

public class LobbySceneManager : MonoBehaviour
{
    public static bool isWatingEnterRoomRes { get; set; } = false;
    public static RoomEnterResPacket roomEnterRes { get; set; }

    private GameNetworkServer gameServer;
    private ErrorMsgBox errorMsgBox;

    // Start is called before the first frame update
    void Start()
    {
        
        gameServer = GameNetworkServer.Instance;
        errorMsgBox = gameObject.AddComponent<ErrorMsgBox>();
        Text userIDText = GameObject.Find("id_txt").GetComponent<Text>();
        
        roomEnterRes = new RoomEnterResPacket();
        userIDText.text = GameNetworkServer.Instance.LocalUserID;
        
        
        if (errorMsgBox != null)
        {
            errorMsgBox.Init();
        }
        else
        {
            Debug.LogWarning("errorMsgBox is null");
        }
        
        Debug.Log("start Lobby Scene");
    }


    // Update is called once per frame
    void Update()
    {

        if(GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.LOGIN && isWatingEnterRoomRes==true)
        {
            if (roomEnterRes.Result == ERROR_CODE.DUMMY_CODE)
            {
                return;
            }

            if (roomEnterRes.Result == ERROR_CODE.NONE)
            {
                GameNetworkServer.Instance.ClientStatus = GameNetworkServer.CLIENT_STATUS.ROOM;
                SceneManager.LoadScene("Game");
            }
            else
            {
                errorMsgBox.PopUpErrorMessage("[방 입장요청 오류]"+roomEnterRes.Result);
            }
        }

    }


    public void SendRoomEnterReqPacket()
    {
        Debug.Log("RoomEnterReqPacket called");
        if (isWatingEnterRoomRes == true)
        {
            return;
        }

        var request = new RoomEnterReqPacket();
        var bodyData = request.ToBytes();

        if (gameServer.ClientStatus == GameNetworkServer.CLIENT_STATUS.LOGIN)
        {

            if (gameServer.GetIsConnected() == false)
            {
               errorMsgBox.PopUpErrorMessage("네트워크와의 접속이 끊어졌습니다");
            }

            GameNetworkServer.Instance.RequestRoomEnter();
        }

        isWatingEnterRoomRes = true;
        Debug.Log("RoomEnterReqPacket sended");
       
    }

    
}
