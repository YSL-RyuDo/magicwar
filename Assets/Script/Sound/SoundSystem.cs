using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem Instance;

    [Header("설정")]
    public AudioClip nightSkyBGM; // 찾으신 밤하늘 브금을 여기에 할당
    private AudioSource audioSource;

    [Header("허용된 씬 이름")]
    public string lobbySceneName = "LobbyScene";
    public string settingSceneName = "SettingScene";

    void Awake()
    {
        // --- 싱글톤 및 파괴 방지 설정 ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // 이미 존재하면 새로 생긴 것은 삭제
            return;
        }
    }

    void OnEnable()
    {
        // 씬이 로드될 때마다 CheckScene 함수를 실행하도록 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 스크립트 비활성화 시 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndPlayMusic(scene.name);
    }

    private void CheckSceneAndPlayMusic(string sceneName)
    {
        // 로비나 세팅 씬일 경우
        if (sceneName == lobbySceneName || sceneName == settingSceneName)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = nightSkyBGM;
                audioSource.loop = true; // 반복 재생
                audioSource.Play();
            }
        }
        else
        {
            // 그 외의 씬(예: GameScene)으로 가면 음악 정지
            audioSource.Stop();
        }
    }
}
