﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace GameNetwork
{

    public enum PACKET_ID : UInt16
    {
        INVALID = 0,

        SYSTEM_CLIENT_CONNECT = 11,
        SYSTEM_CLIENT_DISCONNECTD = 12,

        LOGIN_REQ = 701,
        LOGIN_RES = 702,
                

        PACKET_ID_ROOM_ENTER_REQ = 721,
        PACKET_ID_ROOM_ENTER_RES = 722,
        PACKET_ID_ROOM_USER_LIST_NTF = 723,
        PACKET_ID_ROOM_NEW_USER_NTF = 724,


        NewRoomReq = 203,
        NewRoomRes = 204,

        EnterRoomReq = 206,
        EnterRoomRes = 207,

        LeaveRoomReq = 209,
        LeaveRoomRes = 210,

        ChatRoomReq = 214,
        ChatRoomRes = 215,
        ChatRoomNtf = 216,

        RivalUserInfoNtf = 220,

        GameStartReqPkt = 301,
        GameStartResPkt = 302,
        GameStartNtfPkt = 303,

        GameSyncReqPkt = 304,
        GameSyncNtfPkt = 305,

        GameEndReqPkt = 311,
        GameEndResPkt = 312,
        GameEndNtfPkt = 313,

    }

    public enum ERROR_CODE : Int16
    {
        DUMMY_CODE = -1,
        
        NONE = 1,

        USER_MGR_INVALID_USER_INDEX = 11,
        USER_MGR_INVALID_USER_UNIQUEID = 12,

        LOGIN_USER_ALREADY = 31,
        LOGIN_USER_USED_ALL_OBJ = 32,

        NEW_ROOM_USED_ALL_OBJ = 41,
        NEW_ROOM_FAIL_ENTER = 42,

        ENTER_ROOM_NOT_FINDE_USER = 51,
        ENTER_ROOM_INVALID_USER_STATUS = 52,
        ENTER_ROOM_NOT_USED_STATUS = 53,
        ENTER_ROOM_FULL_USER = 54,

        ROOM_INVALID_INDEX = 61,
        ROOM_NOT_USED = 62,
        ROOM_TOO_MANY_PACKET = 63,

        LEAVE_ROOM_INVALID_ROOM_INDEX = 71,

        CHAT_ROOM_INVALID_ROOM_INDEX = 81,
    }

    public enum EVENT_TYPE : Int16  {
        NONE = -1,
        SPAWN_GROUP_I = 0,
        SPAWN_GROUP_J = 1,
        SPAWN_GROUP_L = 2,
        SPAWN_GROUP_O = 3,
        SPAWN_GROUP_S = 4,
        SPAWN_GROUP_T = 5,
        SPAWN_GROUP_Z = 6,
        MOVE_LEFT = 7,
        MOVE_RIGHT = 8,
        MOVE_DOWN = 9,
        ROTATE = 10,
        DELETE_ROW = 11,

    }



    public class PacketDataValue
    {
        public const int USER_ID_LENGTH = 20;
        public const int USER_PW_LENGTH = 20;
        public const int MAX_CHAT_SIZE = 257;
    }

    struct PacketData
    {
        public Int16 DataSize;
        public Int16 PacketID;
        public SByte Type;
        public byte[] BodyData;
    }

    
    public class RoomEnterResPacket
    {
        public ERROR_CODE Result;
        public string RivalUserID;
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            RivalUserID = Encoding.ASCII.GetString(bodyData, 2, 21).Split('\0')[0];
            Debug.Log("RivalUserID: "+ RivalUserID);

            return true;
        }
    }


    public class RoomLeaveReqPacket
    {
    }

    public class RoomLeaveResPacket
    {
        public ERROR_CODE Result;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }


    public class RoomChatReqPacket
    {
        public string Message;

        public byte[] ToBytes()
        {
            var message = new byte[PacketDataValue.MAX_CHAT_SIZE];
            Encoding.Unicode.GetBytes(Message).CopyTo(message, 0);

            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(message);

            return dataSource.ToArray();
        }
    }


    public class RoomChatResPacket
    {
        public ERROR_CODE Result;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class RoomChatNotPacket
    {
        public string UserID;
        public string Message;

        public bool FromBytes(byte[] bodyData)
        {
            UserID = Encoding.ASCII.GetString(bodyData, 0, 21).Split('\0')[0];
            Message = Encoding.Unicode.GetString(bodyData, 21, 257).Split('\0')[0];

            return true;
        }
    }


    public class GameStartRequestPacket {
    }

    public class GameStartResponsePacket  {
        public ERROR_CODE Result;
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class GameStartNotifyPacket {

    }



    public class GameSynchronizePacket
    {
        public Int16[] EventRecordArr;
        public Int32 Score;
        public Int32 Line;
        public Int32 Level;

        public GameSynchronizePacket()
        {
            EventRecordArr = new Int16[6];

            for (int i=0; i<6; i++) {
             EventRecordArr[i] = (Int16)EVENT_TYPE.NONE;
            }
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();

            foreach (Int16 Record in EventRecordArr) {
                dataSource.AddRange(BitConverter.GetBytes(Record));
            }
            dataSource.AddRange(BitConverter.GetBytes(Score));
            dataSource.AddRange(BitConverter.GetBytes(Line));
            dataSource.AddRange(BitConverter.GetBytes(Level));

            return dataSource.ToArray();
        }
    }


    public class GameSynchronizeNotifyPacket
    {
        public Int16[] EventRecordArr =  new Int16[6];
        public Int32 Score;
        public Int32 Line;
        public Int32 Level;

        public bool FromBytes(byte[] bodyData)
        {
            String DebugString = "";

            for (int i = 0; i < 6; i++)
            {
                EventRecordArr[i] = BitConverter.ToInt16(bodyData, 0 + i * 2);
                DebugString += "{" + EventRecordArr[i] + "}  ";
            }

            Score = BitConverter.ToInt32(bodyData, 12);
            Line = BitConverter.ToInt32(bodyData, 16);
            Level = BitConverter.ToInt32(bodyData, 20);

            return true;
        }
    }




    public class GameEndRequestPacket
    {
    }

    public class GameEndResponsePacket
    {
        public ERROR_CODE Result;
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class GameEndNotifyPacket {

    }


}