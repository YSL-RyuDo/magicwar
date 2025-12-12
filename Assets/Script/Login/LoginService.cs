using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginService : MonoBehaviour
{
    [SerializeField] private NetworkConnector network;
    [SerializeField] private LoginUIController ui;

    private string pendingLoginId; // 로그인 시도한 ID를 저장

    private void Start()
    {
        network.Dispatcher.Register(PacketType.LoginSuccess, OnLoginOK);
        network.Dispatcher.Register(PacketType.LoginIDError,
            _ => ui.SetMessage("Login failed. ID not found."));
        network.Dispatcher.Register(PacketType.LoginPWError,
            _ => ui.SetMessage("Login failed. Incorrect password."));
        network.Dispatcher.Register(PacketType.LOGIN_ALREADY,
            _ => ui.SetMessage("This account is already logged in."));

        network.Dispatcher.Register(PacketType.SignupSuccess,
            _ => ui.SetMessage("Signup successful. Please log in."));
        network.Dispatcher.Register(PacketType.SignupIDError,
            _ => ui.SetMessage("Signup failed. ID already exists."));
        network.Dispatcher.Register(PacketType.SignupNickError,
            _ => ui.SetMessage("Signup failed. Nickname already exists."));
    }

    // =========================
    // Login Request
    // =========================
    public void RequestLogin(string id, string pw)
    {
        pendingLoginId = id; // UI에서 받은 ID를 저장

        PKT_LOGIN_REQ req = new PKT_LOGIN_REQ
        {
            id = id,
            pw = pw
        };

        network.SendPacket(PacketType.Login, StructToBytes(req));
        ui.SetMessage("Logging in...");
    }

    // =========================
    // Signup Request
    // =========================
    public void RequestSignup(string id, string pw, string nick)
    {
        PKT_SIGNUP_REQ req = new PKT_SIGNUP_REQ
        {
            id = id,
            pw = pw,
            nick = nick
        };

        network.SendPacket(PacketType.Signup, StructToBytes(req));
        ui.SetMessage("Signing up...");
    }

    // =========================
    // Login Success
    // =========================
    private void OnLoginOK(byte[] body)
    {
        if (body.Length < Marshal.SizeOf<LoginAckPacket>())
        {
            ui.SetMessage("Login failed. Invalid server response.");
            return;
        }

        LoginAckPacket ack =
            NetworkConnector.ByteArrayToStructure<LoginAckPacket>(body);

        SessionData.UserID = pendingLoginId;
        SessionData.Nickname = ack.nick.TrimEnd('\0');
        SessionData.Token = ack.token;

        Debug.Log($"[Login] Success: {SessionData.UserID}");

        ui.OnLoginSuccess();
    }

    // =========================
    // Struct → byte[]
    // =========================
    private static byte[] StructToBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }
}
