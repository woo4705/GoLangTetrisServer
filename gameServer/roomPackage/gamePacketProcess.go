package roomPackage

import (
	"chatServer/protocol"
	NetLib "gohipernetFake"
)

func (room *BaseRoom) PacketProcess_GameReadyRequest(user *RoomUser, packet protocol.Packet) int16 {
	NetLib.NTELIB_LOG_DEBUG("[Game ReadyRequest Process]")

	var notifyPacket protocol.GameUserStatusNotifyPacket
	notifyPacket.RoomUserUniqueID = user.RoomUniqueID

	if user.Status==USER_STATUS_NONE {
		user.Status = USER_STATUS_READY
		room.ReadyUserCount++
	}else if user.Status==USER_STATUS_READY{
		user.Status = USER_STATUS_NONE
		room.ReadyUserCount--
	}

	notifyPacket.UserStatus = user.Status
	notifySendBuf, packetSize := notifyPacket.EncodingPacket()

	room.BroadCastPacket(packetSize, notifySendBuf, 0)


	if(room.ReadyUserCount==2){
		//게임시작 알림패킷 보내기
		var startPacket protocol.GameStartNotifyPacket
		notifySendBuf, packetSize :=  startPacket.EncodingPacket()
		room.BroadCastPacket(packetSize, notifySendBuf, 0)
	}
	
	NetLib.NTELIB_LOG_DEBUG("Notify GameReady" )

	return protocol.ERROR_CODE_NONE
}