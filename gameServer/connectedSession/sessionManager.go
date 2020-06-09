package connectedSession

import (
	"go.uber.org/zap"
	NetLib "gohipernetFake"
	"sync"
	"sync/atomic"
)

type SessionManager struct {
	UserIDSessionMap *sync.Map
	MaxSessionCount int32
	SessionList		[]*Session

	MaxUserCount	int32
	CurrentLoginUserCount	int32
}

var sessionManager SessionManager

func Init(maxSessionCount int32, maxUserCount int32) bool {
	sessionManager.UserIDSessionMap = new(sync.Map)
	sessionManager.MaxSessionCount = maxSessionCount
	sessionManager.SessionList = make([]*Session, maxSessionCount)

	sessionManager.MaxUserCount = maxUserCount
	sessionManager.CurrentLoginUserCount = 0

	for i:=int32(0); i<maxSessionCount; i++{
		sessionManager.SessionList[i] = new (Session)
		sessionManager.SessionList[i].Init(i)
	}
	return true
}



func AddSession(sessionIndex int32, sessionUniqueID uint64) bool{
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	if sessionManager.SessionList[sessionIndex].GetSessionIsUsing() == SESSION_IS_USING {
		NetLib.NTELIB_LOG_ERROR("already connected session", zap.Int32("sessionIndex",sessionIndex) )
		return false
	}

	sessionManager.SessionList[sessionIndex].Clear()
	sessionManager.SessionList[sessionIndex].SetSessionIsUsing(SESSION_IS_USING,sessionUniqueID)
	return true
}



func RemoveSession(sessionIndex int32, isLoginUser bool)bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	if isLoginUser {
		atomic.AddInt32(&sessionManager.CurrentLoginUserCount, -1 )
		userID := string(sessionManager.SessionList[sessionIndex].GetUserID())
		sessionManager.UserIDSessionMap.Delete(userID)
	}

	sessionManager.SessionList[sessionIndex].Clear()

	return true
}



func ValidSessionIndex(sessionIndex int32) bool {
	if sessionIndex <0 || sessionIndex >= sessionManager.MaxSessionCount {
		return false
	}

	return true
}



func GetNetworkUniqueID(sessionIndex int32) uint64{
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return 0
	}

	return sessionManager.SessionList[sessionIndex].GetNetworkUniqueID()
}

func GetUserID(sessionIndex int32) (string, bool){
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return "",false
	}

	return sessionManager.SessionList[sessionIndex].GetUserID(),true
}



func SetLogin(sessionIndex int32, sessionUniqueID uint64, userID string, curTimeSec int64) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}


	if _, ok:= sessionManager.UserIDSessionMap.Load(userID); ok{
		//중복로그인
		return false
	}

	sessionManager.SessionList[sessionIndex].SetUser(sessionUniqueID, userID, curTimeSec)
	sessionManager.UserIDSessionMap.Store(userID, sessionManager.SessionList[sessionIndex])

	atomic.AddInt32(&sessionManager.CurrentLoginUserCount, 1)

	return true
}



func CheckSessionIsLoggedIn(sessionIndex int32) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	return sessionManager.SessionList[sessionIndex].IsLoggedInSession()
}



func SetRoomNumber(sessionIndex int32, sessionUniqueID uint64, roomNum int32, curTimeSec int64) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	return sessionManager.SessionList[sessionIndex].SetRoomNumber(sessionUniqueID, roomNum, curTimeSec)
}

func GetRoomNumber(sessionIndex int32) (int32, int32){
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return -1,-1
	}
	return sessionManager.SessionList[sessionIndex].GetRoomNumber()
}