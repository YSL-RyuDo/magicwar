using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_SIGNUP_REQ
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string pw;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string nick;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_LOGIN_REQ
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string pw;
}


[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct LoginAckPacket
{
    public ushort result;   // uint16_t
    public uint token;      // uint32_t

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string nick;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct PKT_RES_PROFILE_DATA
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string nickname;

    public UInt32 level;
    public UInt32 exp;

    public UInt32 rank;
    public UInt32 rankScore;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string rankName;

    public UInt32 minScore;
    public UInt32 maxScore;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_CHAT_LOBBY_DATA
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string nickname;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string message;
}


//fffffffffffffffffffffffffffffffffffffffffff


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MatchResponsePacket
{
    public byte result;   // 0=WAIT, 1=OK, 2=FAIL
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string roomId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userA;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userB;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MagicSlotData
{
    public int id;
    public int cooldown;
    public int attack;
    public float speed;
    public float radius;
    public float duration;
    public int amount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct DamagePacket
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string targetId;

    public int damage;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnemyMovePacket
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string enemyId;   // 적 유저 ID (문자열은 고정 길이)

    public float posX;
    public float posY;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpawnMagicPacket
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userId;

    public int cardId;

    public float posX;
    public float posY;

    public float dirX;
    public float dirY;

    public int attack;
    public float speed;
}
