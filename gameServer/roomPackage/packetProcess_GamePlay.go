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
	}else if user.Status==USER_STATUS_READY{
		NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] user.Status is set to none");
		user.Status = USER_STATUS_NONE
	}

	notifyPacket.UserStatus = user.Status
	notifySendBuf, packetSize := notifyPacket.EncodingPacket()

	room.BroadCastPacket(packetSize, notifySendBuf, 0)


	if(room.CurUserCount > 1 && room.CheckAllUserIsReady()==true){
		//게임시작 알림패킷 보내기
		NetLib.NTELIB_LOG_DEBUG("[PacketProcess_GameReadyRequest] GAME START!");
		var startPacket protocol.GameStartNotifyPacket
		notifySendBuf, packetSize :=  startPacket.EncodingPacket()
		room.BroadCastPacket(packetSize, notifySendBuf, 0)

		//TODO: User의 상태값을 GAME으로 변경하기
	}
	
	NetLib.NTELIB_LOG_DEBUG("Notify GameReady" )

	return protocol.ERROR_CODE_NONE
}



func (room *BaseRoom) PacketProcess_Game_Sync_Req(user *RoomUser, packet protocol.Packet) int16{
	var requestPacket protocol.GameSyncRequestPacket
	(&requestPacket).DecodingPacket(packet.Data)


	var notifyPacket protocol.GameSyncNotifyPacket
	for i:=0; i<6; i++{
		notifyPacket.EventRecordArr[i] = requestPacket.EventRecordArr[i]
	}
	notifyPacket.Score = requestPacket.Score
	notifyPacket.Line = requestPacket.Line
	notifyPacket.Level = requestPacket.Level


	sendBuf, packetSize := notifyPacket.EncodingPacket()

	//방에있는 다른 유저 가져오기
	room.BroadCastPacket(packetSize, sendBuf, user.NetSessionUniqueID)
	return protocol.ERROR_CODE_NONE
}




func (room *BaseRoom) PacketProcess_GameEndRequest(user *RoomUser, packet protocol.Packet) int16{
	sessionIndex := packet.RequestSessionIndex
	sessionUniqueID := packet.RequestSessionUniqueID

	//TODO: 임시로 요청에 대한 응답을 모두 ERROR_CODE_NONE으로 하였는데 이후 예외상황에 따라 오류를 반환하는 경우가 추가될 수 있음
	SendGameEndResult(sessionIndex, sessionUniqueID , protocol.ERROR_CODE_NONE)
	SendGameNotifyLose(sessionIndex, sessionUniqueID)

	var ntfPacket_Win protocol.GameEndNotifyPacket
	ntfPacket_Win.GameResult = protocol.GAME_RESULT_WIN
	ntfWinPktBuf, packetSize := ntfPacket_Win.EncodingPacket()
	room.BroadCastPacket(packetSize, ntfWinPktBuf, sessionUniqueID)

	return protocol.ERROR_CODE_NONE

}


func SendGameEndResult(sessionIndex int32, sessionUniqueID uint64, result int16) {
	var resPacket  protocol.GameEndResponsePacket
	resPacket.Result = result
	resPktBuf, _ := resPacket.EncodingPacket()

	NetLib.NetLibIPostSendToClient(sessionIndex, sessionUniqueID, resPktBuf)
}


func SendGameNotifyLose(sessionIndex int32, sessionUniqueID uint64){
	var ntfPacket_Lose protocol.GameEndNotifyPacket
	ntfPacket_Lose.GameResult = protocol.GAME_RESULT_LOSE
	ntfLosePktBuf, _ := ntfPacket_Lose.EncodingPacket()

	NetLib.NetLibIPostSendToClient(sessionIndex, sessionUniqueID, ntfLosePktBuf)
}