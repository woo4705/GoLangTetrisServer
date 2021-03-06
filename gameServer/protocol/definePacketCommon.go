package protocol

import (
	"encoding/binary"
	"reflect"
	NetLib "gohipernetFake"
)


const (
	//데이터 길이관련
	MAX_CHAT_MESSAGE_BYTE_LENGTH = 257

	GAME_SYNC_RECORD_SIZE = 6

	GAME_RESULT_WIN = 1
	GAME_RESULT_LOSE = 0

)



type Header struct {
	TotalSize	int16
	ID			int16
	PacketType	int8
}

type Packet struct {
	RequestSessionIndex		int32
	RequestSessionUniqueID	uint64
	ID						int16
	DataSize				int16
	Data					[]byte
}



var public_clientSessionHeaderSize int16
var public_serverSessionHeaderSize int16

func Init_packetSize() {
	public_clientSessionHeaderSize = ProtocolInitHeaderSize()
	public_serverSessionHeaderSize = ProtocolInitHeaderSize()
}

func ClientHeaderSize() int16{
	return public_clientSessionHeaderSize
}

func ServerHeaderSize() int16{
	return public_serverSessionHeaderSize
}


func ProtocolInitHeaderSize() int16 {
	header := Header{}
	return int16( NetLib.Sizeof(reflect.TypeOf(header)) )
}


func PeekPacketID(rawData []byte) int16{
	packetID := binary.LittleEndian.Uint16(rawData[2:])
	return int16(packetID)
}

func PeekPacketBody(rawData []byte) (bodySize int16, refBody []byte){
	headerSize := ClientHeaderSize()
	//totalSize := int16(binary.LittleEndian.Uint16(rawData))
	bodyData := rawData[headerSize:]
	bodySize = int16( binary.LittleEndian.Uint16(rawData)) - headerSize

	return bodySize, bodyData
}

func DecodingPacketHeader(header *Header, data []byte)  {
	reader := NetLib.MakeReader(data,true)
	header.TotalSize, _ = reader.ReadS16()
	header.ID, _ = reader.ReadS16()
	header.PacketType,_ = reader.ReadS8()
}

func EncodingPacketHeader(writer *NetLib.RawPacketData, totalSize int16, packetID int16, packetType int8)  {
	writer.WriteS16(totalSize)
	writer.WriteS16(packetID)
	writer.WriteS8(packetType)
}
