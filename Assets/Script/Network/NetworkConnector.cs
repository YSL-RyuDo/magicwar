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

    public PacketDispatcher Dispatcher { get; } = new();

    public static NetworkConnector Instance { get; private set; }

    // Token (Runtime + Inspector)
    [Header("Debug (Inspector)")]
    [SerializeField]
    private uint inspectorToken;   // 인스펙터 표시용

    [SerializeField]
    private uint token;            // 실제 내부 토큰

    public uint Token
    {
        get => token;
        private set
        {
            token = value;
            inspectorToken = value;   // 인스펙터 자동 갱신
        }
    }

    private readonly ConcurrentQueue<Action> mainThreadQueue = new();

    private const int BUFFER_SIZE = 4096;
    private readonly byte[] recvBuffer = new byte[8192];
    private int recvBufferSize = 0;

    // Unity LifeCycle
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
        Connect("211.247.47.240", 9000);
        //SendPacket(PacketType.PING, Array.Empty<byte>());
    }

    private void Update()
    {
        while (mainThreadQueue.TryDequeue(out var act))
            act();
    }

    public bool IsConnected => client != null && client.Connected;

    // CONNECT
    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);

            stream = client.GetStream();
            running = true;

            recvThread = new Thread(ReceiveLoop)
            {
                IsBackground = true
            };
            recvThread.Start();

            Debug.Log("[Network] 서버 연결 성공");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Network] 연결 실패: " + ex.Message);
        }
    }

    // SEND (Raw)
    public void SendRaw(byte[] packet)
    {
        if (!IsConnected) return;

        try
        {
            stream.Write(packet, 0, packet.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Network] SendRaw 오류: " + ex.Message);
        }
    }

    // SEND (Header + Body)
    // Header: [packetId(2)][token(4)][bodySize(2)]
    public void SendPacket(PacketType type, byte[] body)
    {
        if (!IsConnected) return;

        ushort packetId = (ushort)type;
        ushort bodySize = (ushort)(body?.Length ?? 0);

        byte[] packet = new byte[8 + bodySize];

        // packetId
        Array.Copy(BitConverter.GetBytes(packetId), 0, packet, 0, 2);

        // token
        Array.Copy(BitConverter.GetBytes(Token), 0, packet, 2, 4);

        // bodySize
        Array.Copy(BitConverter.GetBytes(bodySize), 0, packet, 6, 2);

        // body
        if (bodySize > 0)
            Array.Copy(body, 0, packet, 8, bodySize);

        SendRaw(packet);
    }

    // 문자열 전송용
    public void SendPacket(PacketType type, string msg)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(msg);
        SendPacket(type, body);
    }

    // RECEIVE LOOP
    private void ReceiveLoop()
    {
        byte[] tempBuffer = new byte[4096];

        while (running)
        {
            try
            {
                int recv = stream.Read(tempBuffer, 0, tempBuffer.Length);
                if (recv <= 0)
                    break;

                // 1. 누적 버퍼에 복사
                Buffer.BlockCopy(
                    tempBuffer, 0,
                    recvBuffer, recvBufferSize,
                    recv);

                recvBufferSize += recv;

                // 2. 패킷 파싱 루프
                while (true)
                {
                    // 헤더 최소 크기 체크
                    if (recvBufferSize < 8)
                        break;

                    ushort packetId = BitConverter.ToUInt16(recvBuffer, 0);
                    uint recvToken = BitConverter.ToUInt32(recvBuffer, 2);
                    ushort bodySize = BitConverter.ToUInt16(recvBuffer, 6);

                    int totalSize = 8 + bodySize;

                    // 바디가 아직 덜 왔으면 대기
                    if (recvBufferSize < totalSize)
                        break;

                    // 3. 바디 추출
                    byte[] body = new byte[bodySize];
                    if (bodySize > 0)
                        Buffer.BlockCopy(recvBuffer, 8, body, 0, bodySize);

                    // 4. 버퍼 정리 (남은 데이터 앞으로 당김)
                    Buffer.BlockCopy(
                        recvBuffer, totalSize,
                        recvBuffer, 0,
                        recvBufferSize - totalSize);

                    recvBufferSize -= totalSize;

                    // 5. 메인 스레드로 디스패치
                    mainThreadQueue.Enqueue(() =>
                    {
                        // 로그인 성공 시 토큰 갱신
                        if (packetId == (ushort)PacketType.LoginSuccess)
                        {
                            Token = recvToken;
                            Debug.Log($"[Network] Token 갱신: {Token}");
                        }

                        Dispatcher.Dispatch((PacketType)packetId, body);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Network] 서버 연결 종료: " + e.Message);
                break;
            }
        }

        running = false;
    }

    // STRUCT 변환
    public static T ByteArrayToStructure<T>(byte[] data) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(data, 0, ptr, size);
        T result = Marshal.PtrToStructure<T>(ptr);

        Marshal.FreeHGlobal(ptr);
        return result;
    }

    public static byte[] StructToBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    public void ResetToken()
    {
        Token = 0;
        Debug.Log("[Network] Token reset to 0");
    }
}
