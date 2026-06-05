using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider musicSlider;

    private void Start()
    {
        musicSlider.value = 0.8f;
        SetVolume(0.8f);
    }

    public void SetVolume(float sliderValue)
    {
        audioSource.volume = sliderValue;
    }
}
