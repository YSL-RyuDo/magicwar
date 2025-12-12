using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject loginPanel;

    [Header("Images")]
    [SerializeField] private GameObject loginEnterImage;
    [SerializeField] private GameObject gameEnterImage;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField idInput;
    [SerializeField] private TMP_InputField pwInput;
    [SerializeField] private TMP_InputField nicknameInput;

    [Header("Buttons")]
    [SerializeField] private Button loginEnterButton;
    [SerializeField] private Button gameEnterButton;

    [SerializeField] private Button submitButton;
    [SerializeField] private Button loginModeButton;   // 로그인 모드 버튼
    [SerializeField] private Button signupModeButton;  // 회원가입 모드 버튼
    [SerializeField] private Button quitButton;

    [Header("Text")]
    [SerializeField] private TMP_Text messageText;

    private bool signupMode = false;
    private LoginService loginService;

    private void Awake()
    {
        loginService = FindFirstObjectByType<LoginService>();
    }

    private void Start()
    {
        // 초기 UI 상태
        titlePanel.SetActive(true);
        loginPanel.SetActive(false);

        loginEnterImage.SetActive(true);
        gameEnterImage.SetActive(false);

        SetLoginMode(); // 기본은 로그인 모드

        // 버튼 연결
        loginEnterButton.onClick.AddListener(OpenLoginPanel);
        gameEnterButton.onClick.AddListener(EnterGame);

        submitButton.onClick.AddListener(OnSubmit);
        loginModeButton.onClick.AddListener(SetLoginMode);
        signupModeButton.onClick.AddListener(SetSignupMode);
        quitButton.onClick.AddListener(QuitGame);
    }

    // =====================
    // Login Enter
    // =====================
    private void OpenLoginPanel()
    {
        titlePanel.SetActive(false);
        loginPanel.SetActive(true);
        messageText.text = "Please enter your account information.";
    }

    // =====================
    // Mode 설정
    // =====================
    private void SetLoginMode()
    {
        signupMode = false;
        nicknameInput.gameObject.SetActive(false);
        messageText.text = "Login Mode";
    }

    private void SetSignupMode()
    {
        signupMode = true;
        nicknameInput.gameObject.SetActive(true);
        messageText.text = "Signup Mode";
    }

    // =====================
    // Submit
    // =====================
    private void OnSubmit()
    {
        string id = idInput.text.Trim();
        string pw = pwInput.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            messageText.text = "ID and password are required.";
            return;
        }

        if (signupMode)
        {
            string nick = nicknameInput.text.Trim();
            if (string.IsNullOrEmpty(nick))
            {
                messageText.text = "Nickname is required.";
                return;
            }

            loginService.RequestSignup(id, pw, nick);
        }
        else
        {
            loginService.RequestLogin(id, pw);
        }
    }

    // =====================
    // 로그인 성공 후 (Service에서 호출)
    // =====================
    public void OnLoginSuccess()
    {
        loginPanel.SetActive(false);
        titlePanel.SetActive(true);

        loginEnterImage.SetActive(false);
        gameEnterImage.SetActive(true);

        messageText.text = $"Welcome, {SessionData.Nickname}";
    }

    // =====================
    // Game Enter
    // =====================
    private void EnterGame()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void SetMessage(string msg)
    {
        messageText.text = msg;
    }

    // =====================
    // Quit
    // =====================
    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
