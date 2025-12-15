public static class SessionData
{
    public static string UserID;       // 기존에 있던 것
    public static string Nickname;     // 필요하면 그대로 유지
    public static uint Token = 0;
    // 새로 추가 (매칭 시스템에 필요)
    public static uint RoomID;       // 매칭된 방 ID
    public static string OpponentID;   // 상대 플레이어 ID
}
