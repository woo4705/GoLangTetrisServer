using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace GameNetwork
{

    using System;

    public class LoginReqPacket
    {
        Int16 IDLen;
        Int16 PWLen;
        string UserID = "";
        string UserPW = ""; 
        
        public void SetValue(string userID, string userPW)
        {
            IDLen = (Int16)userID.Length;
            PWLen = (Int16)userPW.Length;
            UserID = userID;
            UserPW = userPW;
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
        
            dataSource.AddRange(BitConverter.GetBytes(IDLen));
            dataSource.AddRange(BitConverter.GetBytes(PWLen));
            dataSource.AddRange(Encoding.UTF8.GetBytes(UserID));
            dataSource.AddRange(Encoding.UTF8.GetBytes(UserPW));
            
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
        public List<UInt64> UserUniqueIdList = new List<UInt64>();
        public List<Int16> UserStatusList = new List<Int16>();
        public List<string> UserIDList = new List<string>();
        

        public bool FromBytes(byte[] bodyData)
        {
            var readPos = 0;
            var userCount = (SByte)bodyData[readPos];
            ++readPos;

            for (int i = 0; i < userCount; ++i)
            {
                var userDataSize = BitConverter.ToInt16(bodyData, readPos);
                readPos += 2;
                Debug.Log("getuserDataSize = "+userDataSize);
                
                
                var uniqueRoomId = BitConverter.ToUInt64(bodyData, readPos);
                readPos += 8;
                Debug.Log("uniqueRoomId");

                var status = BitConverter.ToInt16(bodyData, readPos);
                readPos += 2;
                Debug.Log("userStatus");
               
                var userId = Encoding.UTF8.GetString(bodyData, readPos, userDataSize-12 );
                readPos += userDataSize-10;
                Debug.Log("userID");
                

                UserUniqueIdList.Add(uniqueRoomId);
                UserStatusList.Add(status);
                UserIDList.Add(userId);
                
                
            }

            UserCount = userCount;
            return true;
        }
    }



    public class RoomNewUserNotifyPacket
    {
        public UInt64 UserUniqueId;
        public Int16 UserStatus;
        public string UserID;
        

        public bool FromBytes(byte[] bodyData)
        {
            var readPos = 0;

            UserUniqueId = BitConverter.ToUInt64(bodyData, readPos);
            readPos += 8;
            
            
            UserStatus = BitConverter.ToInt16(bodyData, readPos);
            readPos += 2;
            
            UserID = Encoding.UTF8.GetString(bodyData, readPos, bodyData.Length-readPos);
            

            return true;
        }
    }
    
    
    



    public class RoomLeaveReqPacket
    {
        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            return dataSource.ToArray();
        }

    }

    public class RoomLeaveResPacket
    {
        public ERROR_CODE Result = ERROR_CODE.DUMMY_CODE;

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