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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_RES_USER_MAGIC
{
    public int slotCount;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int[] cardIds;
}

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
public struct PKT_REQ_MAGIC_DETAIL
{
    public int cardId;
}


[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct PKT_RES_MAGIC_DETAIL
{
    public int id;
    public int attack;
    public int cooldown;
    public int cost;
    public int speed;
    public int radius;
    public int duration;
    public int amount;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string effectType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_REQ_REPLACE_MAGIC
{
    public int oldCardId;
    public int newCardId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_RES_REPLACE_USER_MAGIC
{
    public byte result; // 1 = 성공, 0 = 실패
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_MATCH_RESPONSE_STRUCT
{
    public byte result;
    public uint roomId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userA;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userB;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_MOVE_STRUCT
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userId;

    public float x;
    public float y;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_RES_HP_STRUCT
{
    public int currentHp;
    public int maxHp;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct DamageRequestPacket
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string attackerId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string targetId;

    public int cardId;
    public int damage;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_CAST_MAGIC
{
    public int cardId;
    public float dirX;
    public float dirY;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct PKT_SPAWN_MAGIC
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string casterId;

    public int cardId;

    public float dirX;
    public float dirY;

    public int attack;
    public float projectileSpeed;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_REQ_MAGIC_DETAIL_GAME_STRUCT
{
    public int cardId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_RES_MAGIC_DETAIL_GAME_STRUCT
{
    public int cardId;

    public int attack;
    public float speed;
    public float cooldown;

    public float radius;
    public float duration;
    public int amount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct PKT_DIE
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string deadUserId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct PKT_RESULT
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string userId;
}

// 상태이상
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_STATUS_SLOW_STRUCT
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string targetId;

    public float slowRatio;
    public int durationMs;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PKT_STATUS_STUN_STRUCT
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string targetId;

    public int durationMs;
}
