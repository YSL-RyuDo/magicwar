using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        volumeSlider.value = AudioManager.Instance.GetSavedVolume();
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        AudioManager.Instance.SetVolume(value);
    }
}
