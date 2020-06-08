package protocol

import (
	NetLib "gohipernetFake"
	"reflect"
)

type GameReadyRequestPacket struct {
}


type GameUserStatusNotifyPacket struct {
	RoomUserUniqueID	uint64
	UserStatus			int16
}

func (packet GameUserStatusNotifyPacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + 8 + 2
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_USER_STATUS_NTF,0)
	writer.WriteU64(packet.RoomUserUniqueID)
	writer.WriteS16(packet.UserStatus)

	return sendBuf, totalPacketSize
}

func (packet *GameUserStatusNotifyPacket) Decoding(bodyData []byte) bool{
	reader := NetLib.MakeReader(bodyData, true)
	packet.RoomUserUniqueID,_ = reader.ReadU64()
	return true
}


type GameStartNotifyPacket struct {

}

func (packet GameStartNotifyPacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_GAME_START_NTF,0)

	return sendBuf, totalPacketSize
}



type GameSyncRequestPacket struct {
	EventRecordArr [6]int16
	Score int32
	Line  int32
	Level  int32
}

func (packet GameSyncRequestPacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + int16(NetLib.Sizeof(reflect.TypeOf(packet)));
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_GAME_SYNC_NTF,0)

	for i:=0; i<6; i++{
		writer.WriteS16(packet.EventRecordArr[i])
	}
	writer.WriteS32(packet.Score)
	writer.WriteS32(packet.Line)
	writer.WriteS32(packet.Level)

	return sendBuf, totalPacketSize
}

func (packet *GameSyncRequestPacket) DecodingPacket(bodyData []byte) bool{
	reader := NetLib.MakeReader(bodyData, true)
	for i:=0; i<6; i++{
		packet.EventRecordArr[i],_ = reader.ReadS16()
	}
	packet.Score,_ = reader.ReadS32()
	packet.Line,_ = reader.ReadS32()
	packet.Level,_ = reader.ReadS32()
	return true
}




type GameSyncNotifyPacket struct {
	EventRecordArr [6]int16
	Score int32
	Line  int32
	Level  int32
}


func (packet GameSyncNotifyPacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + int16(NetLib.Sizeof(reflect.TypeOf(packet)));
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_GAME_SYNC_NTF,0)

	for i:=0; i<6; i++{
	writer.WriteS16(packet.EventRecordArr[i])
	}
	writer.WriteS32(packet.Score)
	writer.WriteS32(packet.Line)
	writer.WriteS32(packet.Level)

	return sendBuf, totalPacketSize
}

func (packet *GameSyncNotifyPacket) DecodingPacket(bodyData []byte) bool{
	reader := NetLib.MakeReader(bodyData, true)
	for i:=0; i<6; i++{
		packet.EventRecordArr[i],_ = reader.ReadS16()
	}
	packet.Score,_ = reader.ReadS32()
	packet.Line,_ = reader.ReadS32()
	packet.Level,_ = reader.ReadS32()
	return true
}




type GameEndRequestPacket struct {

}


type GameEndResponsePacket struct {
	Result int16
}

func (packet GameEndResponsePacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + 2;
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_GAME_END_RES,0)

	writer.WriteS16(packet.Result)
	return sendBuf, totalPacketSize
}




type GameEndNotifyPacket struct {
	GameResult int16
}

func (packet GameEndNotifyPacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + 2;
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_GAME_END_NTF,0)

	writer.WriteS16(packet.GameResult)
	return sendBuf, totalPacketSize
}
