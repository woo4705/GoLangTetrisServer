using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace GameNetwork
{

   

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

    public class GameReadyRequestPacket {
    }

    public class GameStartResponsePacket  {
        public ERROR_CODE Result;
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    
    public class GameUserStatusNotifyPacket {
        public UInt64 RoomUserUniqueID;
        public Int16 UserStatus;


        public bool FromBytes(byte[] bodyData)
        {
            RoomUserUniqueID = BitConverter.ToUInt64(bodyData, 0);
            UserStatus = BitConverter.ToInt16(bodyData, 8);
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

    public class GameEndNotifyPacket
    {
        public Int16 GameResult;

        public bool FromBytes(byte[] bodyData)
        {
            GameResult = BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }


}
