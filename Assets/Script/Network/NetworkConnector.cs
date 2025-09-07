using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public static class SessionData
{
    public static string UserID;
    public static string Nickname;
    public static string RoomID;
    public static string OpponentID;
}

public class NetworkConnector : MonoBehaviour
{
    private static NetworkConnector instance;
    public static NetworkConnector Instance => instance;

    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    private bool isRunning;

    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;

    private readonly ConcurrentQueue<Action> mainThreadActions = new();
    public PacketDispatcher Dispatcher { get; } = new PacketDispatcher();

    private const int BufferSize = 4096;

    public bool IsConnected => client != null && client.Connected;

    private void Awake()
    {
        // 싱글톤 + DontDestroyOnLoad
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
    }

    private void Start() => Connect(serverIP, serverPort);

    private void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
            action?.Invoke();
    }

    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);
            stream = client.GetStream();
            isRunning = true;

            recvThread = new Thread(ReceiveLoop) { IsBackground = true };
            recvThread.Start();

            Debug.Log($"[Network] Connected {ip}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Connect failed: {e.Message}");
        }
    }

    private void ReceiveLoop()
    {
        var buffer = new byte[BufferSize];
        while (isRunning)
        {
            try
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                if (len > 4)
                {
                    int packetId = BitConverter.ToInt32(buffer, 0);
                    var type = (PacketType)packetId;
                    string msg = Encoding.UTF8.GetString(buffer, 4, len - 4);

                    mainThreadActions.Enqueue(() =>
                    {
                        Dispatcher.Dispatch(type, msg);
                    });
                }
                else if (len == 0)
                {
                    // 서버가 정상적으로 끊었을 때
                    mainThreadActions.Enqueue(() =>
                    {
                        Debug.LogWarning("[Network] Server closed connection");
                        Disconnect();
                    });
                    break;
                }
            }
            catch (Exception e)
            {
                mainThreadActions.Enqueue(() =>
                {
                    Debug.LogError($"[Network] Receive error: {e.Message}");
                    Disconnect();
                });
                break;
            }
        }
    }

    public void SendPacket(PacketType type, string msg)
    {
        if (!IsConnected) return;
        try
        {
            byte[] idBytes = BitConverter.GetBytes((int)type);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(msg);

            byte[] sendData = new byte[idBytes.Length + bodyBytes.Length];
            Buffer.BlockCopy(idBytes, 0, sendData, 0, idBytes.Length);
            Buffer.BlockCopy(bodyBytes, 0, sendData, idBytes.Length, bodyBytes.Length);

            stream.Write(sendData, 0, sendData.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Send failed: {e.Message}");
        }
    }

    public void Disconnect()
    {
        if (!isRunning) return;
        isRunning = false;

        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }

        if (recvThread != null && recvThread.IsAlive)
        {
            try { recvThread.Join(100); } catch { }
        }

        Debug.Log("[Network] Disconnected");
    }

    private void OnDestroy() => Disconnect();
}
