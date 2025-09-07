using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [Header("UI ÂüÁ¶")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private UnityEngine.UI.Button endButton;

    private void Start()
    {
        resultPanel.SetActive(false);

        endButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LobbyScene");
        });
    }

    private void OnEnable()
    {
        var network = NetworkConnector.Instance;
        if (network != null)
        {
            network.Dispatcher.Register(PacketType.Victory, OnVictory);
            network.Dispatcher.Register(PacketType.Defeat, OnDefeat);
        }
    }

    private void OnDisable()
    {
        var network = NetworkConnector.Instance;
        if (network != null)
        {
            network.Dispatcher.Unregister(PacketType.Victory, OnVictory);
            network.Dispatcher.Unregister(PacketType.Defeat, OnDefeat);
        }
    }

    private void OnVictory(string msg)
    {
        ShowResult("WIN");
    }

    private void OnDefeat(string msg)
    {
        ShowResult("LOSE");
    }

    private void ShowResult(string result)
    {
        if (resultPanel == null || resultText == null) return;

        resultPanel.SetActive(true);
        resultText.text = result;
    }
}
