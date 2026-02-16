using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        // 슬라이더 초기값 설정 (기존 믹서 값 불러오기 등 가능)
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    public void SetMasterVolume(float sliderValue)
    {
        // 슬라이더 0.0001~1의 값을 -80dB ~ 0dB로 변환
        // 마스터 볼륨을 로그 단위로 조절
        float volume = Mathf.Log10(sliderValue) * 20;
        masterMixer.SetFloat("MasterVol", volume);
    }
}
