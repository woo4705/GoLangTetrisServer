package roomPackage

import (
	"go.uber.org/zap"
	NetLib "gohipernetFake"
)


func (room *BaseRoom)SetUserStatus(user *RoomUser, status int16) {
	NetLib.NTELIB_LOG_DEBUG("User status is changed",zap.Int16("Status", status));
	user.Status = status
}


func (room *BaseRoom)SetAllUserStatus(status int16) {
	NetLib.NTELIB_LOG_DEBUG("All user status is changed",zap.Int16("Status", status));

	for _, user := range room.UserSessionUniqueIDMap {
		user.Status = status
	}
}


func (room *BaseRoom)CheckGameStartCondition() bool{
	if room.CurUserCount < 2 {
		return false
	}

	for _, user := range room.UserSessionUniqueIDMap {
		if user.Status != USER_STATUS_READY{
			return false
		}
	}

	return true
}