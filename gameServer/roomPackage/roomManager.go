package roomPackage

import (
	"chatServer/protocol"
	"go.uber.org/zap"
	NetLib "gohipernetFake"
)

type RoomManager struct {
	RoomStartNum	int32
	MaxRoomCount	int32
	RoomCountList	[]int16
	RoomList		[]BaseRoom
}

func NewRoomManager(config RoomConfig) *RoomManager {
	roomManager := new(RoomManager)
	roomManager.Initialize(config)
	return roomManager
}


func (roomMgr *RoomManager) Initialize(config RoomConfig) {
	roomMgr.RoomStartNum = config.StartRoomNumber
	roomMgr.MaxRoomCount = config.MaxRoomCount
	roomMgr.RoomCountList = make([]int16, config.MaxRoomCount)
	roomMgr.RoomList = make([]BaseRoom, config.MaxRoomCount)

	for i:= int32(0); i < roomMgr.MaxRoomCount; i++ {
		roomMgr.RoomList[i].Initialize(i, config)
		roomMgr.RoomList[i].SettingPacketFunction()
	}

	LogStartRoomPacketProcess(config.MaxRoomCount, config)

	NetLib.NTELIB_LOG_INFO("[RoomManager Initialize]", zap.Int32("MaxRoomCount",roomMgr.MaxRoomCount))

}


func (roomMgr *RoomManager) GetAllChannelUserCount() []int16 {
	maxRoomCount := roomMgr.MaxRoomCount

	for i := int32(0); i<maxRoomCount; i++ {
		roomMgr.RoomCountList[i] = int16 (roomMgr.GetRoomUserCount(i))
	}


	return roomMgr.RoomCountList
}


func (roomMgr *RoomManager) GetRoomByNumber(roomNumber int32) *BaseRoom {
	roomIndex := roomNumber - roomMgr.RoomStartNum

	if roomIndex < 0 || roomIndex >= roomMgr.MaxRoomCount {
		return nil
	}

	return &roomMgr.RoomList[roomIndex]
}


func (roomMgr *RoomManager) GetRoomUserCount(roomID int32) int32 {
	return roomMgr.RoomList[roomID].GetCurrentUserCount()
}


func (roomMgr *RoomManager) PacketProcess(roomNumber int32, packet protocol.Packet){
	NetLib.NTELIB_LOG_DEBUG("[RoomManager - PacketProcess]", zap.Int16("PacketID", packet.ID))

	room := roomMgr.GetRoomByNumber(roomNumber)
	NetLib.NTELIB_LOG_DEBUG("[RoomManager - PacketProcess]",zap.Int32("roomNumber",roomNumber), zap.Int32("room Struct's number",room.GetNumber()))


	if room == nil {
		protocol.NotifyErrorPacket(packet.RequestSessionIndex, packet.RequestSessionUniqueID, protocol.ERROR_CODE_ROOM_INVALID_NUMBER )
		return
	}

	user := room.GetUser(packet.RequestSessionUniqueID)
	if user == nil {
		protocol.NotifyErrorPacket(packet.RequestSessionIndex, packet.RequestSessionUniqueID, protocol.ERROR_CODE_ROOM_NOT_IN_USER)
		return
	}

	funcCount := len(room.FuncPacketIDList)

	for i:=0; i<funcCount; i++ {
		if room.FuncPacketIDList[i] != packet.ID {
			continue
		}

		result := room.FuncList[i] (user, packet)

		if result != protocol.ERROR_CODE_NONE {
			NetLib.NTELIB_LOG_DEBUG("[Room - PacketProcess Fail]",
			zap.Int16("PacketID",packet.ID), zap.Int16("Error", result))
		}

		return
	}

	NetLib.NTELIB_LOG_DEBUG("[Room- PacketProcess - Fail(Not Registered)]", zap.Int16("PacketID", packet.ID))
}



// TODO: 방 입장을 요청한 유저가 들어갈 방 입장을 탐색할 때,아래의 단순한 Full-Search보다 탐색 범위를 줄일 수 있는 방법으로 개선하는것이 좋다.
func (roomMgr *RoomManager)GetEmptyRoomToEnter() (enterRoomNum int32){
	roomUserCntList := roomMgr.GetAllChannelUserCount()
	roomMaxCnt := len(roomUserCntList)
	roomNum  := -1

	for i:=0; i<roomMaxCnt; i++ {
		if roomUserCntList[i] < 2 {
			roomNum  = i
			break;
		}
	}

	enterRoomNum = int32(roomNum)

	return enterRoomNum
}



func LogStartRoomPacketProcess(maxRoomCount int32, config RoomConfig) {
	NetLib.NTELIB_LOG_INFO("[RoomManager startRoomPacketProcess]",
		zap.Int32("maxRoomCount", maxRoomCount),
		zap.Int32("StartRoomNumber", config.StartRoomNumber),
		zap.Int32("MaxUserCount", config.MaxUserCount),
	)
}


