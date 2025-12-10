using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

public class NetworkConnector : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    private bool running;

    public PacketDispatcher Dispatcher { get; } = new PacketDispatcher();

    const int BufferSize = 4096;

    private readonly ConcurrentQueue<Action> mainThreadQueue = new();

    public bool IsConnected => client != null && client.Connected;

    public static NetworkConnector Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect("127.0.0.1", 9000);
    }

    private void Update()
    {
        while (mainThreadQueue.TryDequeue(out var action))
            action();
    }


    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);

            stream = client.GetStream();
            running = true;

            recvThread = new Thread(ReceiveLoop);
            recvThread.IsBackground = true;
            recvThread.Start();

            Debug.Log("Connected to server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connect error: {ex.Message}");
        }
    }

    // ------------------------------
    //  바이너리 패킷 송신
    // ------------------------------
    public void SendRaw(byte[] packet)
    {
        if (!IsConnected)
            return;

        try
        {
            stream.Write(packet, 0, packet.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send error: {ex.Message}");
        }
    }

    public void SendPacket(PacketType type, string message)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[Network] 연결되지 않아 패킷을 보낼 수 없습니다.");
            return;
        }

        byte[] body = System.Text.Encoding.UTF8.GetBytes(message);
        ushort bodySize = (ushort)body.Length;

        // 서버가 요구하는 구조 = ID(2) + SIZE(2) + BODY
        byte[] packet = new byte[4 + bodySize];

        // [0~1] packetId
        Buffer.BlockCopy(BitConverter.GetBytes((ushort)type), 0, packet, 0, 2);

        // [2~3] bodySize
        Buffer.BlockCopy(BitConverter.GetBytes(bodySize), 0, packet, 2, 2);

        // [4~] body
        Buffer.BlockCopy(body, 0, packet, 4, bodySize);

        stream.Write(packet, 0, packet.Length);
        stream.Flush();

        Debug.Log($"[Network] SendPacket → ID={type}, Body=\"{message}\"");
    }



    // ------------------------------
    //  바이너리 패킷 수신
    // ------------------------------
    private void ReceiveLoop()
    {
        byte[] buffer = new byte[BufferSize];

        while (running)
        {
            try
            {
                int recv = stream.Read(buffer, 0, buffer.Length);

                if (recv < 4)      // 최소 헤더(4바이트)보다 작음 → 오류 또는 연결 종료
                    continue;

                ushort packetId = BitConverter.ToUInt16(buffer, 0);
                ushort bodySize = BitConverter.ToUInt16(buffer, 2);

                if (recv < 4 + bodySize)
                    continue;

                byte[] body = new byte[bodySize];
                Array.Copy(buffer, 4, body, 0, bodySize);

                mainThreadQueue.Enqueue(() =>
                {
                    Dispatcher.Dispatch((PacketType)packetId, body);
                });
            }
            catch
            {
                running = false;
                Debug.LogWarning("Server disconnected.");
                break;
            }
        }
    }

    public static T ByteArrayToStructure<T>(byte[] data, int startIndex) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(data, startIndex, ptr, size);
        T value = Marshal.PtrToStructure<T>(ptr);

        Marshal.FreeHGlobal(ptr);
        return value;
    }

    public static T ByteArrayToStructure<T>(byte[] data) where T : struct
    {
        return ByteArrayToStructure<T>(data, 0);
    }

}
