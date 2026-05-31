using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicTracks;
    private int currentTrackIndex = 0;
    public Slider volumeSlider;

    private void Start()
    {
        audioSource.volume = 1f;

        if (volumeSlider != null)
        {
            volumeSlider.value = audioSource.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (musicTracks.Length > 0)
        { 
            PlayTrack(currentTrackIndex);
        }
    }

    public void NextTrack()
    { 
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        PlayTrack(currentTrackIndex);
    }

    public void PreviousTrack()
    {
        currentTrackIndex--;
        if(currentTrackIndex < 0)
            currentTrackIndex = musicTracks.Length - 1;
        PlayTrack(currentTrackIndex);
    }

    private void PlayTrack(int index)
    {
        audioSource.clip = musicTracks[index];
        audioSource.Play();

    }

    public void SetVolume(float volume)
    { 
        audioSource.volume = volume;
    }
}
