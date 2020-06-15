using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;
using ServerCommon;
using Unity.Collections.LowLevel.Unsafe;

namespace GameNetwork
{
    public class GameNetworkServer : MonoBehaviour
    {
        
        //게임서버 로직에 필요한 변수들을 정의한 부분
        private static GameNetworkServer instance ;

        public string ipAddr { get; set; } = "52.141.58.88";
        private int port = Convert.ToInt32("11031");
        const int PacketHeaderSize = 5;

        ClientSimpleTcp Network = new ClientSimpleTcp();
        PacketBufferManager PacketBuffer = new PacketBufferManager();

        Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();
        Queue<byte[]> SendPacketQueue = new Queue<byte[]>();
        public Queue<RoomChatNotPacket> ChatMsgQueue { get; set; } = new Queue<RoomChatNotPacket>();

        bool IsNetworkThreadRunning = false;

        public bool GetIsConnected() { return Network.IsConnected(); }
        public void Disconnect() { Network.Close(); }

        System.Threading.Thread NetworkReadThread = null;
        System.Threading.Thread NetworkSendThread = null;
        System.Threading.Thread ProcessReceivedPacketThread = null;

        public CLIENT_STATUS ClientStatus { get; set; } = new CLIENT_STATUS();
        public UInt64 Local_RoomUserUniqueID ;
        public String LocalUserID;

        public struct UserData
        {
            public string ID;
            public Int16 Status;
        }

        public Dictionary<UInt64, UserData> RoomUserInfo;
        
        public Single SyncPacketInterval { get; set; } = 0.1f;


        public enum CLIENT_STATUS
        {
            NONE = 0,
            LOGIN = 1,
            ROOM = 2,
            GAME = 3,
        }


        
        //게임서버 싱글톤 패턴을 위한 함수들 정의한 부분
        public static GameNetworkServer Instance {
            get {
                return instance;
            }
        }

        void Awake() {
            if (instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
        }

        

        // 게임서버 멀티스레딩 관련 부분
        void Init() {
            ClientStatus = CLIENT_STATUS.NONE;
            

            PacketBuffer.Init((8096 * 10), PacketHeaderSize, 1024);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();
            ProcessReceivedPacketThread = new System.Threading.Thread(this.ProcessReceivedPacket);
            ProcessReceivedPacketThread.Start();
        }

        public void StopAllNetWorkThread()
        {
            NetworkReadThread.Join();
            NetworkSendThread.Join();
            ProcessReceivedPacketThread.Join();
        }

        public void StartAllNetWorkThread()
        {
            NetworkReadThread.Start();
            NetworkSendThread.Start();
            ProcessReceivedPacketThread.Start();
        }
        
        
        
        
        
          
        // 게임룸 관련 설정하기
        public void InitRoomUserInfo()
        {
            RoomUserInfo = new Dictionary<UInt64, UserData>(2);
        }



        public void AddUserInfo(UInt64 UniqueSessionID, String UserID, Int16 UserStatus)
        {
            UserData userData = new UserData();
            userData.ID = UserID;
            userData.Status = UserStatus;

            RoomUserInfo.Add(UniqueSessionID, userData);

        }


        public void DeleteUserInfo(UInt64 UniqueSessionID)
        {
            bool result = RoomUserInfo.Remove(UniqueSessionID);
            if (result != true)
            {
                Debug.LogError("[GameNetworkServer - DeleteUserInfo] 오류발생");
            }
        }


        //TODO: 해당함수 손보기(원격 접속된 유저없을때 아래 구조인데 메모리릭 없는지 확인
        public UserData GetRemoteUserInfo()
        {
            foreach (var roomUser in RoomUserInfo)
            {
                if (roomUser.Key != Local_RoomUserUniqueID)
                {
                    return roomUser.Value;
                }
            }
            
            UserData emptyUser = new UserData();
            emptyUser.ID = "[]";
            emptyUser.Status = (Int16)CLIENT_STATUS.NONE;

            return emptyUser;
        }
        
        
        
        

        //게임서버 네트워크 부분
        public bool ConnectToServer()
        {
            if (Network.Connect(ipAddr, port))
            {
                Debug.Log("접속성공!");
                return true;
            }
            else {
                Debug.Log("접속실패");
                return false;
            }
        }

        public void RequestLogin(string loginID, string loginPW) {
            var request = new LoginReqPacket();
            request.SetValue(loginID, loginPW);
            var bodyData = request.ToBytes();
            LocalUserID = loginID;
            PostSendPacket(PACKET_ID.LOGIN_REQ, bodyData);
            
            
        }


        public void RequestRoomEnter()
        {
            if (ClientStatus == CLIENT_STATUS.LOGIN)
            {
                var request = new RoomEnterReqPacket();
                var bodyData = request.ToBytes();
                PostSendPacket(PACKET_ID.ROOM_ENTER_REQ, bodyData);
            }
            else
            {
               Debug.LogError("로그인 상태가 아닙니다.");
            }
        }


        public void RequestChatMsg(string msg)
        {
            var request = new RoomChatReqPacket();
            request.SetValue(msg);
            var bodyData = request.ToBytes();
            PostSendPacket(PACKET_ID.ROOM_CHAT_REQ, bodyData);
        }
        
        
        public void RequestRoomLeave()
        {
            if (ClientStatus == CLIENT_STATUS.ROOM )
            {
                var request = new RoomLeaveReqPacket();
                var bodyData = request.ToBytes();
                PostSendPacket(PACKET_ID.ROOM_LEAVE_REQ, bodyData);
            }
            else
            {
                Debug.LogError("방에 입장한 상태가 아니거나, 게임이 진행중입니다.");
            }   
        }

        


        // 게임플레이 네트워크 부분
        public void SendGameReadyPacket()
        {
            PostSendPacket(PACKET_ID.GAME_READY_REQ, null);
        }

        public void SendSynchronizePacket(GameSynchronizePacket packet)
        {
            var request = packet;
            var bodyData = request.ToBytes();
            PostSendPacket(PACKET_ID.GAME_SYNC_REQ, bodyData);
         }

        public void SendGameEndPacket(GameEndRequestPacket packet)
        {
            var request = packet;
            PostSendPacket(PACKET_ID.GAME_END_REQ, null);
        }



        
        
        
        //네트워크 Read/Send 스레드 부분
       public void PostSendPacket(PACKET_ID packetID, byte[] bodyData)
        {
            if (Network.IsConnected() == false)
            {
                Debug.LogWarning("서버에 접속하지 않았습니다");
                return;
            }

            List<byte> dataSource = new List<byte>();
            var packetSize = 0;

            if (bodyData != null)
            {
                packetSize = (Int16)(bodyData.Length + PacketHeaderSize);
            }
            else
            {
                packetSize = (Int16)(PacketHeaderSize);
            }

            dataSource.AddRange(BitConverter.GetBytes((Int16)packetSize));
            dataSource.AddRange(BitConverter.GetBytes((Int16)packetID));
            dataSource.AddRange(new byte[] { (byte)0 });
            if (bodyData != null)
            {
                dataSource.AddRange(bodyData);
            }
            SendPacketQueue.Enqueue(dataSource.ToArray());
        }
       
        
        void NetworkReadProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(32);

                if (Network.IsConnected() == false)
                {
                    continue;
                }

                var recvData = Network.Receive();

                if (recvData.Count > 0)
                {
                    PacketBuffer.Write(recvData.Array, recvData.Offset, recvData.Count);

                    while (true)
                    {
                        var data = PacketBuffer.Read();
                        if (data.Count < 1)
                        {
                            break;
                        }

                        var packet = new PacketData();
                        packet.DataSize = (short)(data.Count - PacketHeaderSize);
                        packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset + 2);
                        packet.BodyData = new byte[packet.DataSize];
                        Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));

                        lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                        {
                            RecvPacketQueue.Enqueue(packet);
                        }
                    }
                }
                else
                {
                    var packet = new PacketData();
                    packet.PacketID = (short)PACKET_ID.SYSTEM_CLIENT_DISCONNECTD;
                    packet.DataSize = 0;

                    lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                    {
                        RecvPacketQueue.Enqueue(packet);
                    }
                }
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(32);

                if (Network.IsConnected() == false)
                {
                    continue;
                }

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (SendPacketQueue.Count > 0)
                    {
                        var packet = SendPacketQueue.Dequeue();
                    //    Debug.Log("SendPacket Packet ID=" + packet.ToString());
                        Network.Send(packet);
                    }
                }
            }
        }


        void ProcessReceivedPacket()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(32);
                ReadPacketQueueProcess();
            }
        }


        void ReadPacketQueueProcess()
        {
            try
            {
                PacketData packet = new PacketData();
                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {

                    if (RecvPacketQueue.Count() > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet.PacketID != 0)
                {
                    GameServerPacketHandler.Process(packet);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

    }






}