package roomPackage

type RoomConfig struct {
	StartRoomNumber	int32
	MaxRoomCount	int32
	MaxUserCount	int32
}

const (
	USER_STATUS_NONE = 1
	USER_STATUS_READY = 2
	USER_STATUS_GAME = 3
)


type RoomUser struct {
	NetSessionIndex		int32
	NetSessionUniqueID	uint64

	RoomUniqueID		uint64
	ID					string
	Status				int16

	packetDataSize		int16
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

func (user *RoomUser) PacketDataSize() int16 {
	return int16(len(user.ID)) + 8 + 2
}

type AddRoomUserInfo struct {
	userID string

	NetSessionIndex		int32
	NetSessionUniqueID	uint64
}