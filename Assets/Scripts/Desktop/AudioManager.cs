using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    [Range(0, 1)]
    public float audioVolume;

    private SoundWidget soundWidget;
    private DataManager.DataCategory cat;
    async void Start()
    {
        soundWidget = FindAnyObjectByType<SoundWidget>();
        cat = DataManager.DataCategory.System;

        audioVolume = await DataManager.ReadDataAsync<float>(cat, "AudioVolume");
        soundWidget.SetSliderValue(audioVolume);
    }

    public async void ChangeVolumeLevel(float newVolume)
    {
        audioVolume = newVolume;
        audioSource.volume = audioVolume;

        await Awaitable.BackgroundThreadAsync();

        await DataManager.WriteDataAsync(cat, "AudioVolume", audioVolume);

        await Awaitable.MainThreadAsync();
    }
}
