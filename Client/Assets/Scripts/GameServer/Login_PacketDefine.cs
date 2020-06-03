using System.Collections.Generic;
using System.Text;

namespace GameNetwork

{
   
    
    using System;
    using UnityEngine;
    using ServerCommon;

    public class LoginReqPacket
    {
        byte[] UserID = new byte[PacketDataValue.USER_ID_LENGTH];
        byte[] UserPW = new byte[PacketDataValue.USER_PW_LENGTH];

        public void SetValue(string userID, string userPW)
        {
            Encoding.UTF8.GetBytes(userID).CopyTo(UserID, 0);
            Encoding.UTF8.GetBytes(userPW).CopyTo(UserPW, 0);
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(UserID);
            dataSource.AddRange(UserPW);
            return dataSource.ToArray();
        }
    }

    public class LoginResPacket
    {
        public ERROR_CODE Result;
        

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }
    
    
    public class RoomEnterReqPacket
    {

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            return dataSource.ToArray();
        }
    }


    public class RoomEnterResPacket
    {
        public ERROR_CODE Result;
        public Int32 RoomNumber;
        public UInt64 RoomUserUniqueID;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            RoomNumber = BitConverter.ToInt32(bodyData, 2);
            RoomUserUniqueID = BitConverter.ToUInt64(bodyData, 6);
            return true;
        }
    }


    


    public class RoomUserListNotifyPacket
    {
        public int UserCount = 0;
        public List<Int64> UserUniqueIdList = new List<Int64>();
        public List<string> UserIDList = new List<string>();

        public bool FromBytes(byte[] bodyData)
        {
            var readPos = 0;
            var userCount = (SByte)bodyData[readPos];
            ++readPos;

            for (int i = 0; i < userCount; ++i)
            {
                var uniqeudId = BitConverter.ToInt64(bodyData, readPos);
                readPos += 8;

                var idlen = (SByte)bodyData[readPos];
                ++readPos;

                var id = Encoding.UTF8.GetString(bodyData, readPos, idlen);
                readPos += idlen;

                UserUniqueIdList.Add(uniqeudId);
                UserIDList.Add(id);
            }

            UserCount = userCount;
            return true;
        }
    }



    public class RoomNewUserNotifyPacket
    {
        public Int64 UserUniqueId;
        public string UserID;

        public bool FromBytes(byte[] bodyData)
        {
            UserUniqueId = BitConverter.ToInt64(bodyData, 0);
            
            var idlen = (SByte)bodyData[8];
 
            UserID = Encoding.UTF8.GetString(bodyData, 9, idlen);

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
        Int16 MsgLen;
        byte[] Message;

        public void SetValue(string message)
        {
            Message = Encoding.UTF8.GetBytes(message);
            MsgLen = (Int16)Message.Length;
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(MsgLen));
            dataSource.AddRange(Message);
            return dataSource.ToArray();
        }
    }


    public class RoomChatResPacket
    {
        public Int16 Result;
        
        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    
    }

    public class RoomChatNotPacket
    {
        public Int64 UserUniqueId;
        public string Message;

        public bool FromBytes(byte[] bodyData)
        {
            UserUniqueId = BitConverter.ToInt64(bodyData, 0);

            var msgLen = BitConverter.ToInt16(bodyData, 8);
            byte[] messageTemp = new byte[msgLen];
            Buffer.BlockCopy(bodyData, 8 + 2, messageTemp, 0, msgLen);
            Message = Encoding.UTF8.GetString(messageTemp);
            return true;
        }
    }



}