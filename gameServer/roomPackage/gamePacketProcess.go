package roomPackage

import (
	"chatServer/protocol"
	"go.uber.org/zap"
	NetLib "gohipernetFake"
)

func (room *BaseRoom) PacketProcess_GameReadyRequest(user *RoomUser, packet protocol.Packet) int16 {
	NetLib.NTELIB_LOG_DEBUG("[Game ReadyRequest Process]")

	var notifyPacket protocol.GameUserStatusNotifyPacket
	notifyPacket.RoomUserUniqueID = user.RoomUniqueID
	NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] ",zap.Int16("user.Status:",user.Status))
	if user.Status==USER_STATUS_NONE {
		NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] user.Status is set to ready");
		user.Status = USER_STATUS_READY
		room.ReadyUserCount++
	}else if user.Status==USER_STATUS_READY{
		NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] user.Status is set to none");
		user.Status = USER_STATUS_NONE
		room.ReadyUserCount--
	}

	notifyPacket.UserStatus = user.Status
	notifySendBuf, packetSize := notifyPacket.EncodingPacket()

	room.BroadCastPacket(packetSize, notifySendBuf, 0)


	if(room.ReadyUserCount==2){
		//게임시작 알림패킷 보내기
		NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] GAME START!");
		var startPacket protocol.GameStartNotifyPacket
		notifySendBuf, packetSize :=  startPacket.EncodingPacket()
		room.BroadCastPacket(packetSize, notifySendBuf, 0)
	}
	
	NetLib.NTELIB_LOG_DEBUG("Notify GameReady" )

	return protocol.ERROR_CODE_NONE
}