package protocol

import (
	"go.uber.org/zap"
	NetLib "gohipernetFake"
	"reflect"
)


// Login Request패킷 정의
type LoginRequestPacket struct {
	IDLen int16
	PWLen int16
	UserID string
	UserPW string
}


func (packet *LoginRequestPacket) DecodingPacket(bodyData []byte) bool {

	reader := NetLib.MakeReader(bodyData, true)
	packet.IDLen,_ = reader.ReadS16();
	packet.PWLen,_ = reader.ReadS16();

	bodySize := int(packet.IDLen + packet.PWLen) + 4

	NetLib.NTELIB_LOG_DEBUG("LoginPacket Bodysize = ",zap.Int("bodySize",bodySize ))

	if len(bodyData) != bodySize {
		return false
	}

	packet.UserID = string( reader.ReadBytes(int(packet.IDLen)) )
	packet.UserPW = string( reader.ReadBytes(int(packet.PWLen)) )

	return true
}




// Login Response패킷 정의
type LoginResponsePacket struct {
	Result int16
}

//여기에서는 인자로 에러코드를 받는데, 이것은
func (packet LoginResponsePacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + 2
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_LOGIN_RES,0)
	writer.WriteS16(packet.Result)

	return sendBuf, totalPacketSize
}




// RoomEnter RequestPacket
type RoomEnterRequestPacket struct {

}

func (packet RoomEnterRequestPacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ROOM_ENTER_REQ,0)

	return sendBuf, totalPacketSize
}

func (packet *RoomEnterRequestPacket) DecodingPacket(bodyData []byte) bool{
	bodySize := 0

	NetLib.NTELIB_LOG_DEBUG("[roomEnterRequestPacket body size]", zap.Int32("bodySize)",int32(len(bodyData))))

	if len(bodyData) != bodySize {

		return false
	}

	return true
}




//RoomEnter ResponsePacket
type RoomEnterResponsePacket struct {
	Result		 	 int16
	RoomNumber 		 int32
	RoomUserUniqueID uint64
}

func (packet RoomEnterResponsePacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + int16( NetLib.Sizeof(reflect.TypeOf(packet)) )
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ROOM_ENTER_RES,0)
	writer.WriteS16(packet.Result)
	writer.WriteS32(packet.RoomNumber)
	writer.WriteU64(packet.RoomUserUniqueID)

	return sendBuf, totalPacketSize
}

func (packet *RoomEnterResponsePacket) DecodingPacket(bodyData []byte) bool {
	bodySize := NetLib.Sizeof(reflect.TypeOf(packet))
	if len(bodyData) != bodySize {
		return false
	}

	reader := NetLib.MakeReader(bodyData, true)
	packet.Result,_ = reader.ReadS16()
	packet.RoomNumber,_ = reader.ReadS32()
	packet.RoomUserUniqueID,_ = reader.ReadU64()

	return true
}




//RoomUserListData Packet
/*
type RoomUserData struct {

	SessionUniqueID uint64
	ID		 		string
	Status	 		int16
}
*/

type RoomUserListNotifyPacket struct{
	UserCount int8
	UserList []byte
}

func (packet RoomUserListNotifyPacket) EncodingPacket(userInfoListSize int16) ([]byte, int16){
	bodySize := userInfoListSize + 1
	totalPacketSize := public_clientSessionHeaderSize + bodySize

	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ROOM_USER_LIST_NTF,0)

	writer.WriteS8(packet.UserCount)
	writer.WriteBytes(packet.UserList)

	return sendBuf, totalPacketSize
}

func (packet *RoomUserListNotifyPacket) DecodingPacket(bodyData []byte)bool {
	bodySize := NetLib.Sizeof(reflect.TypeOf(packet))
	if len(bodyData) != bodySize {
		return false
	}

	reader := NetLib.MakeReader(bodyData, true)
	packet.UserCount,_ = reader.ReadS8()
	packet.UserList = reader.ReadBytes(len(bodyData)-1)
	return true
}




//RoomNewUserNotify Packet
type RoomNewUserNotifyPacket struct{
	User []byte
}

func (packet RoomNewUserNotifyPacket) EncodingPacket(userInfoSize int16) ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + userInfoSize
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ROOM_NEW_USER_NTF,0)

	writer.WriteBytes(packet.User)
	return sendBuf, totalPacketSize
}



//RoomLeaveResponse Packet
type RoomLeaveUserResponsePacket struct{
	Result int16
}

func (packet RoomLeaveUserResponsePacket) EncodingPacket() ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + 2
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_ROOM_LEAVE_RES,0)
	writer.WriteS16(packet.Result)

	return sendBuf, totalPacketSize
}

func (packet *RoomLeaveUserResponsePacket) Decoding(bodyData []byte) bool {
	reader := NetLib.MakeReader(bodyData, true)
	packet.Result,_ =  reader.ReadS16()
	return true
}


//RoomLeaveNotifyPacket
type RoomLeaveUserNotifyPacket struct{
	RoomUserUniqueID uint64
}


func (packet RoomLeaveUserNotifyPacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + 8
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ROOM_LEAVE_USER_NTF,0)

	writer.WriteU64(packet.RoomUserUniqueID)
	return sendBuf, totalPacketSize
}

func (packet RoomLeaveUserNotifyPacket) Decoding(bodyData []byte)bool {
	bodySize := NetLib.Sizeof(reflect.TypeOf(packet))
	if len(bodyData) != bodySize {
		return false
	}

	reader := NetLib.MakeReader(bodyData, true)
	packet.RoomUserUniqueID,_ = reader.ReadU64()
	return true
}






//RoomChatRequestPacket
type RoomChatRequestPacket struct{
	MsgLength	int16
	MsgData		[]byte
}

func (packet RoomChatRequestPacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + 2 + int16(packet.MsgLength)
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize, PACKET_ID_ROOM_CHAT_REQ,0)
	writer.WriteS16(packet.MsgLength)
	writer.WriteBytes(packet.MsgData)

	return sendBuf, totalPacketSize
}

func (packet *RoomChatRequestPacket) Decoding(bodyData []byte) bool {
	reader := NetLib.MakeReader(bodyData, true)
	packet.MsgLength,_ =  reader.ReadS16()
	packet.MsgData = reader.ReadBytes(int(packet.MsgLength))
	return true
}




//RoomChatNotifyPacket
type RoomChatNotifyPacket struct {
	RoomUserUniqueID	uint64
	MsgLen				int16
	Msg					[]byte
}

func (packet RoomChatNotifyPacket) EncodingPacket() ([]byte, int16){
	totalPacketSize := public_clientSessionHeaderSize + 8 + 2 + packet.MsgLen
	sendBuf := make([]byte, totalPacketSize)
	writer := NetLib.MakeWriter(sendBuf, true)
	EncodingPacketHeader(&writer, totalPacketSize, PACKET_ID_ROOM_CHAT_NOTIFY, 0)

	writer.WriteU64(packet.RoomUserUniqueID)
	writer.WriteS16(packet.MsgLen)
	writer.WriteBytes(packet.Msg)

	return sendBuf, totalPacketSize
}


func (packet *RoomChatNotifyPacket) Decoding(bodyData []byte) bool{
	reader := NetLib.MakeReader(bodyData, true)
	packet.RoomUserUniqueID,_ = reader.ReadU64()
	packet.MsgLen,_ = reader.ReadS16()
	packet.Msg = reader.ReadBytes(int(packet.MsgLen))

	return true
}



//Error Notify패킷 정의
type ErrorNotifyPacket struct {
	ErrorCode int16
}

func (packet ErrorNotifyPacket) EncodingPacket(errorCode int16) ([]byte, int16) {
	totalPacketSize := public_clientSessionHeaderSize + 2
	sendBuf := make([]byte,totalPacketSize)

	writer := NetLib.MakeWriter(sendBuf,true)
	EncodingPacketHeader(&writer,totalPacketSize,PACKET_ID_ERROR_NTF,0)
	writer.WriteS16(errorCode)

	return sendBuf, totalPacketSize
}

func (packet *ErrorNotifyPacket) DecodingPacket(bodyData []byte) bool {
	bodySize := 2
	if len(bodyData) != bodySize {
		return false
	}

	reader := NetLib.MakeReader(bodyData, true)
	packet.ErrorCode,_ = reader.ReadS16()

	return true
}



//오류가 발생하면 오류를 알려주는 패킷
func NotifyErrorPacket(sessionIndex int32, sessionUniqueID uint64, errorCode int16){
	var packet ErrorNotifyPacket
	sendBuf,_ := packet.EncodingPacket(errorCode)
	NetLib.NetLibIPostSendToClient(sessionIndex, sessionUniqueID, sendBuf)
}



