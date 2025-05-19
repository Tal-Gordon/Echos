using UnityEngine;
using UnityEngine.UI;

public class SoundWidget : MonoBehaviour
{
    public AudioSource audioSource;
    public Sprite volumeOn;
    public Sprite volumeOff;

    private AudioManager audioManager;
    private Image widgetGraphic;
    private Image primaryWidgetGraphic;
    private GameObject volumeMixer;
    private Slider volumeSlider;
    void Start()
    {
        Utils.GetOrAddComponent<BoxCollider2D>(gameObject).size = new Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y);
        volumeMixer = transform.Find("VolumeMixer").gameObject;
        volumeSlider = volumeMixer.transform.Find("Slider").GetComponent<Slider>();
        widgetGraphic = volumeMixer.transform.Find("Graphic").GetComponent<Image>();
        primaryWidgetGraphic = GetComponent<Image>();
        audioManager = FindAnyObjectByType<AudioManager>();
    }
    public void OnSliderValueChange()
    {
        audioManager.ChangeVolumeLevel(volumeSlider.value);
        if (volumeSlider.value == 0)
        {
            widgetGraphic.sprite = volumeOff;
            primaryWidgetGraphic.sprite = volumeOff;
        }
        else
        {
            if (widgetGraphic.sprite != volumeOn) 
            { 
                widgetGraphic.sprite = volumeOn;
                primaryWidgetGraphic.sprite = volumeOn;
            }
        }
    }
    public void OnGraphicButtonClick()
    {
        if (widgetGraphic.sprite == volumeOff)
        {
            widgetGraphic.sprite = volumeOn;
            primaryWidgetGraphic.sprite = volumeOn;
            audioManager.ChangeVolumeLevel(volumeSlider.value);
        }
        else if (widgetGraphic.sprite == volumeOn)
        {
            widgetGraphic.sprite = volumeOff;
            primaryWidgetGraphic.sprite = volumeOff;
            audioManager.ChangeVolumeLevel(0);
        }
    }
    public void SetSliderValue(float value)
    {
        volumeSlider.value = value;
        OnSliderValueChange();
    }
}
