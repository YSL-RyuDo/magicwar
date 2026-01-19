using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class LoginService : MonoBehaviour
{
    [SerializeField] private NetworkConnector network;
    [SerializeField] private LoginUIController ui;

    private string pendingLoginId; // 로그인 시도한 ID 저장

    // Unity Lifecycle

    void Awake()
    {
        Screen.SetResolution(576, 864, false);
    }


    private void Start()
    {
        network.Dispatcher.Register(PacketType.LoginSuccess, OnLoginOK);
        network.Dispatcher.Register(PacketType.LoginIDError, OnLoginIDError);
        network.Dispatcher.Register(PacketType.LoginPWError, OnLoginPWError);
        network.Dispatcher.Register(PacketType.LOGIN_ALREADY, OnLoginAlready);

        network.Dispatcher.Register(PacketType.SignupSuccess, OnSignupSuccess);
        network.Dispatcher.Register(PacketType.SignupIDError, OnSignupIDError);
        network.Dispatcher.Register(PacketType.SignupNickError, OnSignupNickError);
    }

    private void OnDestroy()
    {
        if (network == null || network.Dispatcher == null)
            return;

        network.Dispatcher.Unregister(PacketType.LoginSuccess, OnLoginOK);
        network.Dispatcher.Unregister(PacketType.LoginIDError, OnLoginIDError);
        network.Dispatcher.Unregister(PacketType.LoginPWError, OnLoginPWError);
        network.Dispatcher.Unregister(PacketType.LOGIN_ALREADY, OnLoginAlready);

        network.Dispatcher.Unregister(PacketType.SignupSuccess, OnSignupSuccess);
        network.Dispatcher.Unregister(PacketType.SignupIDError, OnSignupIDError);
        network.Dispatcher.Unregister(PacketType.SignupNickError, OnSignupNickError);

        Debug.Log("[LoginService] Dispatcher Unregister 완료");
    }

    // Login Request
    public void RequestLogin(string id, string pw)
    {
        pendingLoginId = id;

        PKT_LOGIN_REQ req = new PKT_LOGIN_REQ
        {
            id = id,
            pw = pw
        };

        network.SendPacket(PacketType.Login, StructToBytes(req));
        ui.SetMessage("Logging in...");
    }

    // Signup Request
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

    // Packet Callbacks
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

    private void OnLoginIDError(byte[] _)
    {
        ui.SetMessage("Login failed. ID not found.");
    }

    private void OnLoginPWError(byte[] _)
    {
        ui.SetMessage("Login failed. Incorrect password.");
    }

    private void OnLoginAlready(byte[] _)
    {
        ui.SetMessage("This account is already logged in.");
    }

    private void OnSignupSuccess(byte[] _)
    {
        ui.SetMessage("Signup successful. Please log in.");
    }

    private void OnSignupIDError(byte[] _)
    {
        ui.SetMessage("Signup failed. ID already exists.");
    }

    private void OnSignupNickError(byte[] _)
    {
        ui.SetMessage("Signup failed. Nickname already exists.");
    }

    // Struct → byte[]
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
