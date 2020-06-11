using System;

namespace GameNetwork
{
    public enum PACKET_ID : UInt16
    {
        INVALID = 0,

        SYSTEM_CLIENT_CONNECT = 11,
        SYSTEM_CLIENT_DISCONNECTD = 12,

        LOGIN_REQ = 701,
        LOGIN_RES = 702,
                

        ROOM_ENTER_REQ = 721,
        ROOM_ENTER_RES = 722,
        ROOM_USER_LIST_NTF = 723,
        ROOM_NEW_USER_NTF = 724,

        
        ROOM_LEAVE_REQ = 726,
        ROOM_LEAVE_RES = 727,
        ROOM_LEAVE_NTF = 728,
        
        ROOM_CHAT_REQ = 731,
        ROOM_CHAT_NTF = 733,


        GAME_READY_REQ = 751,
        USER_STATUS_NTF = 752,

        GAME_START_NTF = 762,

        GAME_SYNC_REQ = 304,
        GAME_SYNC_NTF = 305,

        GAME_END_REQ = 311,
        GAME_END_RES = 312,
        GAME_END_NTF = 313,

    }

    public enum ERROR_CODE : Int16
    {
        DUMMY_CODE = -1,
        
        NONE = 1,
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


    public enum DUMMY_ROOM_USER_ID : UInt64
    {
        VALUE = 0
    }
    
    
    public enum GAME_USER_STATUS : Int16
    {
        NONE = 1,
        READY = 2,
        GAME = 3,
    }

    public enum GAME_RESULT : Int16
    {
        WIN = 1,
        LOSE =0
    }


}