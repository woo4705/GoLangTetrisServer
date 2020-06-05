
using System;
using UnityEngine;

namespace GameNetwork
{
    class GameServerPacketHandler
    {
        public static void Process(PacketData packet)
        {
            var packetType = (PACKET_ID)packet.PacketID;

            switch (packetType)  {
                case PACKET_ID.LOGIN_RES:
                    {
                        ProcessLoginResponse(packet);
                        break;
                    }

                case PACKET_ID.ROOM_ENTER_RES:
                    {
                        ProcessEnterRoomResponse(packet);
                        break;
                    }

                case PACKET_ID.ROOM_USER_LIST_NTF:
                    {
                        ProcessRoomUserListNotify(packet);
                        break;
                    }
                
                case PACKET_ID.ROOM_NEW_USER_NTF:
                    {
                        ProcessRoomNewUserNotify(packet);
                        break;
                    }

                case PACKET_ID.ROOM_CHAT_NTF:
                    {
                        ProcessChatRoomNotify(packet);
                        break;
                    }

                case PACKET_ID.USER_STATUS_NTF:
                    {
                        ProcessGameUserStatusNotify(packet);
                        break;
                    }

                case PACKET_ID.GAME_START_NTF:
                    {
                        ProcessGameStartNotify(packet);
                        break;
                    }

                case PACKET_ID.GAME_SYNC_NTF:
                    {
                        ProcessGameSyncNotify(packet);
                        break;
                    }

                case PACKET_ID.GameEndResPkt:
                    {
                        ProcessGameEndResponse(packet);
                        break;
                    }

                case PACKET_ID.GameEndNtfPkt:
                    {
                        ProcessGameEndNotify(packet);
                        break;  
                    }
            }
        }



        static void ProcessLoginResponse(PacketData packet)
        {
            var response = new LoginResPacket();
            response.FromBytes(packet.BodyData);

            if (response.Result == ERROR_CODE.NONE)
            {
                Debug.Log("로그인성공");
            }
            else
            {
                GameNetworkServer.Instance.LocalUserID = "";
            }

            LoginSceneManager.loginResult = (Int16)response.Result;

        }

        
        
        static void ProcessEnterRoomResponse(PacketData packet)
        {
            var response = LobbySceneManager.roomEnterRes;
            response.FromBytes(packet.BodyData);

            GameNetworkServer.Instance.Local_RoomUserUniqueID = response.RoomUserUniqueID;
            string userID = GameNetworkServer.Instance.LocalUserID;
            
            GameNetworkServer.Instance.InitRoomUserInfo();
            GameNetworkServer.Instance.AddUserInfo(response.RoomUserUniqueID, userID, 0);
           
            
        }
        
        
        
        static void ProcessRoomUserListNotify(PacketData packet)
        {
            var notify = new RoomUserListNotifyPacket();
            notify.FromBytes(packet.BodyData);
            
            // 테트리스 대전방에는 2명을 최대로 고정하는것 생각했기에, 하드코딩이 들어가있다.
            Debug.Log("[ProcessRoomUserListNotify] roomUserCnt="+notify.UserCount);
            Debug.Log("[ProcessRoomUserListNotify] UserIDList[0]="+notify.UserIDList[0]);
            Debug.Log("[ProcessRoomUserListNotify] UserUniqueIdList[0]="+notify.UserUniqueIdList[0]);
            Debug.Log("[ProcessRoomUserListNotify] UserUniqueIdList[0]="+notify.UserStatusList[0]);
            
            
            //GameNetworkServer.Instance
            GameNetworkServer.Instance.AddUserInfo(notify.UserUniqueIdList[0], notify.UserIDList[0], notify.UserStatusList[0]);
            
            //GameSceneManager.isRemoteUserInRoom = true;
            //GameSceneManager.isRemoteUserInfoNeedUpdate = true;
        }


        static void ProcessRoomNewUserNotify(PacketData packet)
        {
            var notify = new RoomNewUserNotifyPacket();
            notify.FromBytes(packet.BodyData);

            Debug.Log("[ProcessRoomUserListNotify] UserStatus="+notify.UserStatus);
            Debug.Log("[ProcessRoomUserListNotify] UserID="+notify.UserID);
            Debug.Log("[ProcessRoomUserListNotify] UserUniqueId="+notify.UserUniqueId);
            
            GameNetworkServer.Instance.AddUserInfo(notify.UserUniqueId, notify.UserID, notify.UserStatus);
            GameSceneManager.isRemoteUserInRoom = true;
            GameSceneManager.isRemoteUserInfoNeedUpdate = true;
            
        }
        

        static void ProcessChatRoomNotify(PacketData packet)
        {
            var response = new RoomChatNotPacket();
            response.FromBytes(packet.BodyData);
            GameNetworkServer.Instance.ChatMsgQueue.Enqueue(response);
        }


        static void ProcessGameUserStatusNotify(PacketData packet)
        {
            var notify = new GameUserStatusNotifyPacket();
            notify.FromBytes(packet.BodyData);

            if (notify.RoomUserUniqueID == GameNetworkServer.Instance.Local_RoomUserUniqueID)
            {
                if (notify.UserStatus == 2)
                {
                    Debug.Log("UserStatusReady");
                    GameSceneManager.isLocalReadyON_MsgArrived = true;
                }
                else
                {
                    Debug.Log("UserStatus:"+notify.UserStatus);
                    GameSceneManager.isLocalReadyOFF_MsgArrived = true;
                }
            }
            else
            {
                if (notify.UserStatus == 2)
                {
                    Debug.Log("UserStatusReady");
                    GameSceneManager.isRemoteReadyON_MsgArrived = true;
                }
                else
                {
                    Debug.Log("UserStatus:"+notify.UserStatus);
                    GameSceneManager.isRemoteReadyOFF_MsgArrived = true;
                }
            }
        }

        
        
        
        static void ProcessGameStartNotify(PacketData packet)
        {
           var response = new GameStartNotifyPacket();
            GameNetworkServer.Instance.ClientStatus = GameNetworkServer.CLIENT_STATUS.GAME;
            Spawner.isGameStart = true;
        }

        static void ProcessGameSyncNotify(PacketData packet)
        {
            var response = new GameSynchronizeNotifyPacket();
            response.FromBytes(packet.BodyData);
            
            if (ShadowGrid.RecvSyncPacketQueue != null)
            {
                ShadowGrid.RecvSyncPacketQueue.Enqueue(response);
            }
        }

        static void ProcessGameEndResponse(PacketData packet)
        {
            var response = new GameEndResponsePacket();
            response.FromBytes(packet.BodyData);
            //TODO Result에 따른 처리 구현하기
        }

        static void ProcessGameEndNotify(PacketData packet)
        {
            Spawner.isGameEndPacketArrived = true;
            GameNetworkServer.Instance.Disconnect();
        }
    }
}
