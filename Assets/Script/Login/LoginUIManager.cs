using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동에 필요

public class LoginUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button changeButton;
    [SerializeField] private Button enterButton;
    [SerializeField] private TMP_Text messageText;

    [Header("Refs")]
    [SerializeField] private NetworkConnector network;

    private bool isSignupMode;

    private readonly System.Collections.Generic.Dictionary<PacketType, string> messages = new()
    {
        { PacketType.SignupSuccess,   "Signup successful." },
        { PacketType.SignupIDError,   "Signup failed: ID already exists." },
        { PacketType.SignupNickError, "Signup failed: Nickname already exists." },
        { PacketType.LoginSuccess,    "Login successful." },
        { PacketType.LoginIDError,    "Login failed: ID not found." },
        { PacketType.LoginPWError,    "Login failed: Wrong password." },
    };

    private void Start()
    {
        enterButton.onClick.AddListener(OnClickEnter);
        changeButton.onClick.AddListener(OnClickChange);

        nicknameInputField.gameObject.SetActive(false);

        if (network != null)
        {
            network.Dispatcher.Register(PacketType.SignupSuccess, msg => OnServerResponse(PacketType.SignupSuccess, msg));
            network.Dispatcher.Register(PacketType.SignupIDError, msg => OnServerResponse(PacketType.SignupIDError, msg));
            network.Dispatcher.Register(PacketType.SignupNickError, msg => OnServerResponse(PacketType.SignupNickError, msg));
            network.Dispatcher.Register(PacketType.LoginSuccess, msg => OnServerResponse(PacketType.LoginSuccess, msg));
            network.Dispatcher.Register(PacketType.LoginIDError, msg => OnServerResponse(PacketType.LoginIDError, msg));
            network.Dispatcher.Register(PacketType.LoginPWError, msg => OnServerResponse(PacketType.LoginPWError, msg));
        }
    }

    private void OnServerResponse(PacketType type, string msg)
    {
        if (messages.TryGetValue(type, out string display))
            SetMessage(display);
        else
            SetMessage($"Unknown server response ({type}): {msg}");

        // 로그인 성공 시 로비 씬으로 이동
        if (type == PacketType.LoginSuccess)
        {
            SessionData.UserID = idInputField.text.Trim();
            SessionData.Nickname = msg;

            SceneManager.LoadScene("LobbyScene");
        }
    }

    private void OnClickEnter()
    {
        string id = idInputField.text.Trim();
        string pw = passwordInputField.text.Trim();

        if (isSignupMode)
        {
            string nick = nicknameInputField.text.Trim();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(nick))
            {
                SetMessage("Please fill all fields.");
                return;
            }
            network.SendPacket(PacketType.Signup, $"{id},{pw},{nick}");
        }
        else
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                SetMessage("Please enter ID and Password.");
                return;
            }
            network.SendPacket(PacketType.Login, $"{id},{pw}");
        }
    }

    private void OnClickChange()
    {
        isSignupMode = !isSignupMode;
        nicknameInputField.gameObject.SetActive(isSignupMode);
        SetMessage(isSignupMode ? "Signup mode" : "Login mode");
    }

    private void SetMessage(string text)
    {
        if (messageText != null)
            messageText.text = text;
    }
}
