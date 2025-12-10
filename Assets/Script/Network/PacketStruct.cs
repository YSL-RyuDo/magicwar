using System;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ProfilePacket
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string nickname;

    public UInt32 level;
    public UInt32 rankId;
    public UInt32 rankScore;
    public UInt32 rankMin;
    public UInt32 rankMax;
}


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
