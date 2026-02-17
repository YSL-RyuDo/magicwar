using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider volumeSlider;

    // 저장할 때 사용할 키 이름
    private const string MasterVolumeKey = "MasterVolume";

    void Start()
    {
        // 1. 저장된 볼륨 값이 있다면 불러오고, 없으면 기본값 0.75f 사용
        float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);

        // 2. 슬라이더의 위치를 저장된 값으로 세팅
        volumeSlider.value = savedVolume;

        // 3. 믹서에 실제 볼륨 적용
        SetMasterVolume(savedVolume);

        // 4. 슬라이더를 움직일 때마다 이벤트 발생하도록 등록
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        SetMasterVolume(value);

        // 5. 값이 변할 때마다 실시간으로 저장
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void SetMasterVolume(float sliderValue)
    {
        // 로그 스케일 변환 (슬라이더 0.0001~1 -> -80dB~0dB)
        float volume = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        masterMixer.SetFloat("MasterVol", volume);
    }

    // 씬이 꺼질 때 확실히 저장 (안전장치)
    void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
