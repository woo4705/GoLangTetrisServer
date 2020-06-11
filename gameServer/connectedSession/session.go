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


func (session *Session) SetRoomNumber(sessionUniqueID uint64, roomNum int32 ) bool {
	atomic.StoreInt32(&session.RoomNumber, roomNum)
	return true
}


func (session *Session) GetRoomNumber() (int32) {
	roomNum := atomic.LoadInt32(&session.RoomNumber)
	return roomNum
}