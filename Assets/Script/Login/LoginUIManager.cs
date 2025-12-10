using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LoginUIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameInput;
    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField pwInput;

    [SerializeField] Button enterButton;
    [SerializeField] Button modeButton;

    [SerializeField] TMP_Text messageText;

    [SerializeField] NetworkConnector network;

    private bool signupMode = false;

    private void Start()
    {
        modeButton.onClick.AddListener(ToggleMode);
        enterButton.onClick.AddListener(OnClickEnter);

        nicknameInput.gameObject.SetActive(false);

        // 서버 응답 등록
        network.Dispatcher.Register(PacketType.SignupSuccess, OnSignupOK);
        network.Dispatcher.Register(PacketType.SignupIDError, OnSignupIDFail);
        network.Dispatcher.Register(PacketType.SignupNickError, OnSignupNickFail);

        network.Dispatcher.Register(PacketType.LoginSuccess, OnLoginOK);
        network.Dispatcher.Register(PacketType.LoginIDError, OnLoginIDFail);
        network.Dispatcher.Register(PacketType.LoginPWError, OnLoginPWFail);
    }

    // -------------------------
    // UI 입력 처리
    // -------------------------
    private void OnClickEnter()
    {
        string id = idInput.text.Trim();
        string pw = pwInput.text.Trim();

        if (signupMode)
        {
            string nick = nicknameInput.text.Trim();
            SendSignup(id, pw, nick);
        }
        else
        {
            SendLogin(id, pw);
        }
    }

    private void ToggleMode()
    {
        signupMode = !signupMode;
        nicknameInput.gameObject.SetActive(signupMode);

        messageText.text = signupMode ? "Signup Mode" : "Login Mode";
    }

    // -------------------------
    // 전송: 회원가입
    // -------------------------
    private void SendSignup(string id, string pw, string nick)
    {
        byte[] body = new byte[32 + 64 + 32];

        Array.Copy(System.Text.Encoding.UTF8.GetBytes(id), 0, body, 0, Mathf.Min(32, id.Length));
        Array.Copy(System.Text.Encoding.UTF8.GetBytes(pw), 0, body, 32, Mathf.Min(64, pw.Length));
        Array.Copy(System.Text.Encoding.UTF8.GetBytes(nick), 0, body, 96, Mathf.Min(32, nick.Length));

        SendPacket(PacketType.Signup, body);
    }

    // -------------------------
    // 전송: 로그인
    // -------------------------
    private void SendLogin(string id, string pw)
    {
        byte[] body = new byte[32 + 64];

        Array.Copy(System.Text.Encoding.UTF8.GetBytes(id), 0, body, 0, Mathf.Min(32, id.Length));
        Array.Copy(System.Text.Encoding.UTF8.GetBytes(pw), 0, body, 32, Mathf.Min(64, pw.Length));

        SendPacket(PacketType.Login, body);
    }

    // -------------------------
    // 실제 송신 패킷 빌드
    // -------------------------
    private void SendPacket(PacketType type, byte[] body)
    {
        ushort pid = (ushort)type;
        ushort size = (ushort)body.Length;

        byte[] packet = new byte[4 + size];

        Array.Copy(BitConverter.GetBytes(pid), 0, packet, 0, 2);
        Array.Copy(BitConverter.GetBytes(size), 0, packet, 2, 2);
        Array.Copy(body, 0, packet, 4, size);

        network.SendRaw(packet);
    }

    // -------------------------
    // 서버 응답 처리
    // -------------------------
    private void OnSignupOK(byte[] body)
        => messageText.text = "Signup Success!";

    private void OnSignupIDFail(byte[] body)
        => messageText.text = "Signup Failed: ID exists.";

    private void OnSignupNickFail(byte[] body)
        => messageText.text = "Signup Failed: Nickname exists.";

    private void OnLoginOK(byte[] body)
    {
        messageText.text = "Login Success!";
        SceneManager.LoadScene("LobbyScene");
    }

    private void OnLoginIDFail(byte[] body)
        => messageText.text = "Login Failed: ID not found.";

    private void OnLoginPWFail(byte[] body)
        => messageText.text = "Login Failed: Wrong password.";
}
