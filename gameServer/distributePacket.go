package main

import (
	"chatServer/connectedSession"
	"chatServer/protocol"
	"go.uber.org/zap"
	NetLib "gohipernetFake"
)

func (server *GameServer) DistributePacket(sessionIndex int32, sessionUniqueID uint64, packetData []byte){
	packetID := protocol.PeekPacketID(packetData)
	bodySize, bodyData := protocol.PeekPacketBody(packetData)

	NetLib .NTELIB_LOG_DEBUG("Distribute Packet",
		zap.Int32("sessionIndex", sessionIndex), zap.Uint64("sessionUniqueID",sessionUniqueID), zap.Int16("PacketID",packetID))

	packet := protocol.Packet{ID: packetID}
	packet.RequestSessionIndex = sessionIndex
	packet.RequestSessionUniqueID = sessionUniqueID
	packet.ID = packetID
	packet.DataSize = bodySize
	packet.Data = make([]byte, packet.DataSize)
	copy(packet.Data, bodyData)

	server.PacketChannel <- packet

	NetLib.NTELIB_LOG_DEBUG("distributePacket", zap.Int32("sessionIndex",sessionIndex), zap.Int16("PacketID",packetID))
}


func (server *GameServer) PacketProcess_goroutine() {
	NetLib.NTELIB_LOG_DEBUG("start PacketProcess goroutine")

	//특정 panic에 따른 처리를 넣고싶다면 아래의 구조를 수정하자
	for{
		if server.PacketProcess_goroutine_Impl() {
			NetLib.NTELIB_LOG_INFO("Wanted Stop PacketProcess goroutine")
			break
		}
	}

	NetLib.NTELIB_LOG_INFO("Stop rooms PacketProcess goroutine")
}


func (server *GameServer) PacketProcess_goroutine_Impl() bool {
	IsWantTermination := false
	defer NetLib.PrintPanicStack()

	for{
		packet := <- server.PacketChannel
		sessionIndex := packet.RequestSessionIndex
		sessionUniqueID := packet.RequestSessionUniqueID
		bodySize := packet.DataSize
		bodyData := packet.Data

		// 아래의 함수는 몇몇의 경우가 아니라면 Room을 호출하여 패킷들을 처리한다.
		// 현재 게임서버의 구조가, 거의 대부분의 작업들이 Room안에서 처리되기 때문. 만일 Room밖에서 처리되는 기능들이 많다면 구조가 변경될 수 있다.
		if packet.ID == protocol.PACKET_ID_LOGIN_REQ {
			ProcessPacketLogin(sessionIndex, sessionUniqueID, bodySize, bodyData)
		}else if packet.ID == protocol.PACKET_ID_ROOM_ENTER_REQ {
			ProcessRoomEnterRequest(server , sessionIndex , sessionUniqueID , bodyData, packet)
		}else if packet.ID == protocol.PACKET_ID_SESSION_CLOSE_SYS {
			ProcessPacketSesssionClosed(server, sessionIndex, sessionUniqueID)
		}else {
			roomNumber,_ := connectedSession.GetRoomNumber(sessionIndex)
			server.RoomMgr.PacketProcess(roomNumber, packet)
		}
	}

	return IsWantTermination
}



func ProcessPacketLogin(sessionIndex int32, sessionUniqueID uint64, bodySize int16, bodyData []byte)  {
	var reqPacket protocol.LoginRequestPacket
	NetLib.NTELIB_LOG_DEBUG("Process Login Packet");


	if (&reqPacket).DecodingPacket(bodyData) == false {
		SendLoginResult(sessionIndex, sessionUniqueID, protocol.ERROR_CODE_PACKET_DECODING_FAIL)
		return
	}

	userID := reqPacket.UserID

	if len(userID) <= 0 {
		SendLoginResult(sessionIndex, sessionUniqueID, protocol.ERROR_CODE_LOGIN_USER_INVALID_ID)
		return
	}

	curTime := NetLib.NetLib_GetCurrnetUnixTime()

	if connectedSession.SetLogin(sessionIndex, sessionUniqueID, userID, curTime) == false {
		SendLoginResult(sessionIndex, sessionUniqueID, protocol.ERROR_CODE_LOGIN_USER_DUPLICATION)
		return
	}

	SendLoginResult(sessionIndex, sessionUniqueID, protocol.ERROR_CODE_NONE)
}



func SendLoginResult(sessionIndex int32, sessionUniqueID uint64, result int16){
	var resPacket protocol.LoginResponsePacket
	resPacket.Result = result
	sendPacket,_ := resPacket.EncodingPacket()

	NetLib.NetLibIPostSendToClient(sessionIndex, sessionUniqueID, sendPacket)
	NetLib.NTELIB_LOG_DEBUG("SendLoginResult", zap.Int32("sessionIndex", sessionIndex), zap.Int16("result",result))
}



func ProcessRoomEnterRequest(server *GameServer, sessionIndex int32, sessionUniqueID uint64, bodyData []byte, packet protocol.Packet){
	var reqPacket protocol.RoomEnterRequestPacket

	NetLib.NTELIB_LOG_DEBUG("Process Room Enter Packet");

	if (&reqPacket).DecodingPacket(bodyData) == false {
		SendLoginResult(sessionIndex, sessionUniqueID, protocol.ERROR_CODE_PACKET_DECODING_FAIL)
		return
	}

	enterRoomNum := server.RoomMgr.GetEmptyRoomToEnter()
	room := server.RoomMgr.GetRoomByNumber(enterRoomNum)

	//packet
	room.PacketProcess_EnterUser(packet)


}




func ProcessPacketSesssionClosed(server *GameServer, sessionIndex int32, sessionUniqueID uint64){

	//먼저 로그인한 유저인지 확인. 로그인한 유저가 아니라면 바로 아래의 조건문에서 처리
	if connectedSession.CheckSessionIsLoggedIn(sessionIndex) == false {
		NetLib.NTELIB_LOG_INFO("DisConnectClient", zap.Int32("sessionIndex",sessionIndex))
		connectedSession.RemoveSession(sessionIndex, false)
		return
	}

	roomNumber,_ := connectedSession.GetRoomNumber(sessionIndex)


	if roomNumber > -1 {
		packet := protocol.Packet{
			sessionIndex,
			sessionUniqueID,
			protocol.PACKET_ID_ROOM_LEAVE_REQ,
			0,
			nil,
		}

		server.RoomMgr.PacketProcess(roomNumber, packet)
	}

	connectedSession.RemoveSession(sessionIndex, true)
}