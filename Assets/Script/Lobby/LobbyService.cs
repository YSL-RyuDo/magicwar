using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class LobbyService : MonoBehaviour
{
    private NetworkConnector network;
    private LobbyUIController ui;

    private void Awake()
    {
        network = NetworkConnector.Instance;
        ui = FindFirstObjectByType<LobbyUIController>();
    }

    private void Start()
    {
        if (network == null || !network.IsConnected)
        {
            Debug.LogError("[LobbyService] NetworkConnector not connected");
            return;
        }

        network.Dispatcher.Register(PacketType.ResponseProfile, OnProfileReceived);
        network.Dispatcher.Register(PacketType.LobbyChat, OnChatReceived);

        RequestProfile();
    }

    public void RequestProfile()
    {
        byte[] body = new byte[32];
        byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(SessionData.UserID);
        Array.Copy(idBytes, body, Math.Min(32, idBytes.Length));

        network.SendPacket(PacketType.RequestProfile, body);
        Debug.Log("[LobbyService] 프로필 요청 전송");
    }

    private void OnProfileReceived(byte[] body)
    {
        if (body.Length < Marshal.SizeOf<PKT_RES_PROFILE_DATA>())
        {
            Debug.LogError("[LobbyService] 프로필 패킷 크기 오류");
            return;
        }

        PKT_RES_PROFILE_DATA profile =
            NetworkConnector.ByteArrayToStructure<PKT_RES_PROFILE_DATA>(body);

        ui.ApplyProfile(profile);

        Debug.Log($"[LobbyService] 프로필 수신: {profile.nickname}");
    }

    public void SendChat(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;

        byte[] body = System.Text.Encoding.UTF8.GetBytes(msg);
        network.SendPacket(PacketType.LobbyChat, body);

        Debug.Log($"[LobbyService] 채팅 송신: {msg}");
    }

    private void OnChatReceived(byte[] body)
    {
        int size = Marshal.SizeOf<PKT_CHAT_LOBBY_DATA>();
        if (body.Length < size)
        {
            Debug.LogWarning("[LobbyService] 채팅 패킷 크기 부족");
            return;
        }

        PKT_CHAT_LOBBY_DATA chat =
            NetworkConnector.ByteArrayToStructure<PKT_CHAT_LOBBY_DATA>(body);

        Debug.Log($"[LobbyService] 채팅 수신: {chat.nickname} : {chat.message}");

        ui.AddChat(chat);
    }
}
