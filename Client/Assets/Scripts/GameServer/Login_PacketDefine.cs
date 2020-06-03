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


    public class RoomNewReqPacket
    {
    }

    public class RoomNewResPacket
    {
        public ERROR_CODE Result;
        public int RoomNumber;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            RoomNumber = BitConverter.ToInt32(bodyData, 2);
            return true;
        }
    }


    public class RoomEnterReqPacket
    {
        public int RoomNumber;

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(RoomNumber));
            return dataSource.ToArray();
        }
    }



    
}