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

var global_sessionManager SessionManager

func Init(maxSessionCount int32, maxUserCount int32) bool {
	global_sessionManager.UserIDSessionMap = new(sync.Map)
	global_sessionManager.MaxSessionCount = maxSessionCount
	global_sessionManager.SessionList = make([]*Session, maxSessionCount)

	global_sessionManager.MaxUserCount = maxUserCount
	global_sessionManager.CurrentLoginUserCount = 0

	for i:=int32(0); i<maxSessionCount; i++{
		global_sessionManager.SessionList[i] = new (Session)
		global_sessionManager.SessionList[i].Init(i)
	}
	return true
}



func AddSession(sessionIndex int32, sessionUniqueID uint64) bool{
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	//ConnectTimeSecond가 해당 세션이 이미 연결된 것인지 아닌지 확인하는 용도로 사용된다.
	//하지만 이것은 굳이 time값 아니여도 bool값이나 다른 값들로 확인할 수 있지 않을까..? timesecond를 쓰는 이유 알아보기

	if global_sessionManager.SessionList[sessionIndex].GetConnectTimeSecond() > 0 {
		NetLib.NTELIB_LOG_ERROR("already connected session", zap.Int32("sessionIndex",sessionIndex) )
		return false
	}

	global_sessionManager.SessionList[sessionIndex].Clear()
	global_sessionManager.SessionList[sessionIndex].SetConnectTimeSecond(NetLib.NetLib_GetCurrnetUnixTime(), sessionUniqueID)
	return true
}



func RemoveSession(sessionIndex int32, isLoginUser bool)bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	if isLoginUser {
		atomic.AddInt32(&global_sessionManager.CurrentLoginUserCount, -1 )
		userID := string(global_sessionManager.SessionList[sessionIndex].GetUserID())
		global_sessionManager.UserIDSessionMap.Delete(userID)
	}

	global_sessionManager.SessionList[sessionIndex].Clear()

	return true
}



func ValidSessionIndex(index int32) bool {
	if index <0 || index >= global_sessionManager.MaxSessionCount {
		return false
	}

	return true
}



func GetNetworkUniqueID(sessionIndex int32) uint64{
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return 0
	}

	return global_sessionManager.SessionList[sessionIndex].GetNetworkUniqueID()
}

func GetUserID(sessionIndex int32) (string, bool){
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return "",false
	}

	return global_sessionManager.SessionList[sessionIndex].GetUserID(),true
}



func SetLogin(sessionIndex int32, sessionUniqueID uint64, userID string, curTimeSec int64) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}


	if _, ok:= global_sessionManager.UserIDSessionMap.Load(userID); ok{
		//중복로그인
		return false
	}

	global_sessionManager.SessionList[sessionIndex].SetUser(sessionUniqueID, userID, curTimeSec)
	global_sessionManager.UserIDSessionMap.Store(userID, global_sessionManager.SessionList[sessionIndex])

	atomic.AddInt32(&global_sessionManager.CurrentLoginUserCount, 1)

	return true
}


func IsLoginUser(sessionIndex int32) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	return global_sessionManager.SessionList[sessionIndex].IsAuthorized()
}



func SetRoomNumber(sessionIndex int32, sessionUniqueID uint64, roomNum int32, curTimeSec int64) bool {
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return false
	}

	return global_sessionManager.SessionList[sessionIndex].SetRoomNumber(sessionUniqueID, roomNum, curTimeSec)
}

func GetRoomNumber(sessionIndex int32) (int32, int32){
	if ValidSessionIndex(sessionIndex) == false {
		NetLib.NTELIB_LOG_ERROR("Invalid sessionIndex", zap.Int32("sessionIndex",sessionIndex))
		return -1,-1
	}
	return global_sessionManager.SessionList[sessionIndex].GetRoomNumber()
}