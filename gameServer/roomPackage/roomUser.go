package roomPackage

import (
	"chatServer/connectedSession"
	"chatServer/protocol"
)

type RoomConfig struct {
	StartRoomNumber	int32
	MaxRoomCount	int32
	MaxUserCount	int32
}

const (
	USER_STATUS_NONE = 1
	USER_STATUS_READY = 2
	USER_STATUS_GAME = 3
	USER_STATUS_GAMEOVER = 4
)


type RoomUser struct {
	NetSessionIndex		int32
	NetSessionUniqueID	uint64

	RoomUniqueID		uint64
	ID					string
	Status				int16

	UserPacketDataSize		int16
}



type AddRoomUserInfo struct {
	userID string

	NetSessionIndex		int32
	NetSessionUniqueID	uint64
}

func (user *RoomUser)Init (userID string, uniqueID uint64) {

	user.ID = userID
	user.RoomUniqueID = uniqueID
	user.Status = USER_STATUS_NONE
}


func (user *RoomUser) SetNetworkInfo(sessionIndex int32, sessionUniqueID uint64) {
	user.NetSessionIndex = sessionIndex
	user.NetSessionUniqueID = sessionUniqueID
}

func (user *RoomUser) GetUserPacketDataSize() int16 {
	return int16(len(user.ID)) + 8 + 2
	//유저의 정보를 보낼떄는 1.UserID | 2.해당 유저가 접속한 세션의 고유번호 | 3.유저의 상태, 정보를 패킷으로 보낸다.
}

func MakeRoomUserDataToAdd(sessionIndex int32, sessionUniqueID uint64) (userInfo AddRoomUserInfo,result int16) {
	userID, ok := connectedSession.GetUserID(sessionIndex)

	if ok == false {
		SendRoomEnterResult(sessionIndex, sessionUniqueID, -1,0, protocol.ERROR_CODE_ENTER_ROOM_INVALID_USER_ID)
		return userInfo, protocol.ERROR_CODE_ENTER_ROOM_INVALID_USER_ID
	}

	userInfo = AddRoomUserInfo{
		userID,
		sessionIndex,
		sessionUniqueID,
	}

	return userInfo, protocol.ERROR_CODE_NONE

}