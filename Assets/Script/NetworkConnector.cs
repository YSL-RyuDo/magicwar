using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkConnector : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    private bool isRunning;

    [Header("Server Settings")]
    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;

    // 다른 스크립트에서 이벤트 구독 가능
    public Action<string> OnMessageReceived;

    void Start()
    {
        Connect(serverIP, serverPort);
    }

    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient(ip, port);
            stream = client.GetStream();
            isRunning = true;

            recvThread = new Thread(ReceiveLoop);
            recvThread.IsBackground = true;
            recvThread.Start();

            Debug.Log("서버 연결 성공!");          // 한국어 로그
            Send("Unity Client 연결됨!");          // UTF-8 메시지 전송
        }
        catch (Exception e)
        {
            Debug.LogError("서버 연결 실패: " + e.Message); // 한국어 로그
        }
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (isRunning)
        {
            try
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                if (len > 0)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, len);
                    Debug.Log("서버: " + msg);   // 한국어 로그
                    OnMessageReceived?.Invoke(msg);
                }
            }
            catch
            {
                Disconnect();
            }
        }
    }

    public void Send(string msg)
    {
        if (stream == null) return;
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("메시지 전송 실패: " + e.Message); // 한국어 로그
        }
    }

    public void Disconnect()
    {
        if (!isRunning) return;
        isRunning = false;

        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }

        if (recvThread != null && recvThread.IsAlive)
            recvThread.Join();

        Debug.Log("서버 연결 종료"); // 한국어 로그
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}
