package connectedSession

import (
	"sync/atomic"
)

type Session struct {
	Index 					int32
	NetworkUniqueID 		uint64

	UserID 					string

	IsUsing					int32
	RoomNumber				int32
	RoomNumberOfEntering	int32
}

const(
	SESSION_IS_NOT_USING = 0
	SESSION_IS_USING = 1
)



func (session *Session) Init(index int32){
	session.Index = index
	session.Clear()
}



func (session *Session) ClearUserID(){
	session.UserID = ""
}

func (session *Session) Clear(){
	session.ClearUserID()
	session.SetRoomNumber(0, -1)
	session.SetSessionIsUsing(SESSION_IS_NOT_USING, 0)
}



func (session *Session) GetIndex() int32{
	return session.Index
}

func (session *Session) GetNetworkUniqueID() uint64{
	return atomic.LoadUint64(&session.NetworkUniqueID)
}

func (session *Session) GetNetworkInfo() (int32, uint64) {
	index := session.GetIndex()
	uniqueID := atomic.LoadUint64(&session.NetworkUniqueID)

	return index, uniqueID
}



func (session *Session) SetUserID(userID string){
	session.UserID = userID
}

func (session *Session) GetUserID() string {
	return session.UserID
}

func (session *Session) GetUserIDLength() int{
	return len(session.UserID)
}



func (session *Session) SetSessionIsUsing( usingState int32, sessionUniqueID uint64 ){
	atomic.StoreInt32(&session.IsUsing, usingState)
	atomic.StoreUint64(&session.NetworkUniqueID,sessionUniqueID )
}

func (session *Session) GetSessionIsUsing() int32{
	return atomic.LoadInt32(&session.IsUsing)
}


func (session *Session) SetUser(sessionUserID uint64, userID string){
	session.SetUserID(userID)
	session.SetRoomNumber(sessionUserID, -1 )

}

func (session *Session) IsLoggedInSession () bool{
	if session.UserID != "" {
		return true
	}
	return false
}


func (session *Session) ValidNetworkUniqueID(uniqueID uint64) bool {
	return atomic.LoadUint64(&session.NetworkUniqueID) == uniqueID
}



func (session *Session) SetRoomEntering(roomNum int32) bool {
	if atomic.CompareAndSwapInt32(&session.RoomNumberOfEntering, -1, roomNum) == false {
		return false
	}
	return true
}

/*

roomNumber가 기존에 -1로 되어있으면 새로운 roomNum를 대입하고, true 리턴함
roomNumber가 -1가 아니라면 false를 반환하고 return해버림.

세션이 자신이들어간 roomNumber가 이미 존재하는데,
다른 부분에서 의도치않게 입장되어진 roomNumber를 교체하지 못하도록 방어코딩인지??

또한 distributePacket에서 패킷을 처리할 방을 불러오는 부분에서는 roomNumberOfEntering을 버리고 roomNumber만 가지고 오고있다.

 */
func (session *Session) SetRoomNumber(sessionUniqueID uint64, roomNum int32 ) bool {

	if roomNum == -1 {
		atomic.StoreInt32(&session.RoomNumber, roomNum)
		atomic.StoreInt32(&session.RoomNumberOfEntering, roomNum)
	}

	if sessionUniqueID != 0 && session.ValidNetworkUniqueID(sessionUniqueID) == false{
		return false
	}

	if atomic.CompareAndSwapInt32(&session.RoomNumber, -1, roomNum) == false {
		return false
	}

	atomic.StoreInt32(&session.RoomNumberOfEntering, roomNum)

	return true
}



func (session *Session) GetRoomNumber() (int32, int32) {
	roomNum := atomic.LoadInt32(&session.RoomNumber)
	roomNumOfEntering := atomic.LoadInt32(&session.RoomNumberOfEntering)

	return roomNum, roomNumOfEntering
}