using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer mixer;

    private const string MasterParam = "MasterVol";
    private const string SaveKey = "MasterVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplySavedVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(MasterParam, db);
        PlayerPrefs.SetFloat(SaveKey, value);
    }

    private void ApplySavedVolume()
    {
        float saved = PlayerPrefs.GetFloat(SaveKey, 0.7f);
        SetVolume(saved);
    }

    public float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat(SaveKey, 0.7f);
    }
}
