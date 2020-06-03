using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameNetwork;

public class LobbySceneManager : MonoBehaviour
{
    public static bool isMatchingResArrived { get; set; } = false;
    public static bool isMatchingNtfArrived { get; set; } = false;
    public static bool isWatingEnterRoomRes { get; set; } = false;
    //private static PKTNtfLobbyMatch matchInfo;
    public static RoomEnterResPacket roomEnterRes { get; set; }
    
    private GameNetworkServer gameServer;

    // Start is called before the first frame update
    void Start()
    {
        
        gameServer = GameNetworkServer.Instance;
        roomEnterRes = new RoomEnterResPacket();
        // matchInfo = new PKTNtfLobbyMatch();
        
    }


    // Update is called once per frame
    void Update()
    {
    
       
    

        if (isMatchingNtfArrived == true)
        {
            ProcessMatchingNotify();
            isMatchingNtfArrived = false;
        }

        if(GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.LOGIN && isWatingEnterRoomRes==false)
        {
        //    GameNetworkServer.Instance.RequestRoomEnter(matchInfo.RoomNumber);
            isWatingEnterRoomRes = true;
        }
        else if(GameNetworkServer.Instance.ClientStatus == GameNetworkServer.CLIENT_STATUS.ROOM)
        {
            SceneManager.LoadScene("Game");
        }
        
    }


  

    public void SendRoomEnterReqPacket()
    {
     //gameServer.Send  
    }



    void ProcessMatchingNotify()
    {
      
        if(GameNetworkServer.Instance.GetIsConnected() == true)
        {
            //RoomEnter

        }
    }


    public static bool FillMatchInfo(string ip_addr, ushort port, int room_idx)
    {
       // matchInfo.GameServerIP = ip_addr;
       // matchInfo.GameServerPort = port;
      //  matchInfo.RoomNumber = room_idx;

        Debug.Log("[FillMatchInfo] ip:"+ip_addr+"  port:"+port+"  roomNum:"+room_idx);
        return true;
    }

    public static int GetMatchedRooom()
    {
       // return matchInfo.RoomNumber;
       return 0;
    }
}
