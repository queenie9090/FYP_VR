using System;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class ClockTicking : MonoBehaviour
{
    public Transform clockHandSecond, clockHandMinute, clockHandHour;
    public AudioClip clockTickingClip;
    public bool analog;

    private const float hoursToDegrees = -360f / 12f;
    private const float minutesToDegrees = -360f / 60f;
    private const float secondsToDegrees = -360f / 60f;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clockTickingClip;
            audioSource.Play();
        }
        if (analog)
        {
            TimeSpan timespan = DateTime.Now.TimeOfDay;
            clockHandHour.localRotation = Quaternion.Euler(0f, 0f, (float)timespan.TotalHours * -hoursToDegrees);
            clockHandMinute.localRotation = Quaternion.Euler(0f, 0f, (float)timespan.TotalMinutes * -minutesToDegrees);
            clockHandSecond.localRotation = Quaternion.Euler(0f, 0f, (float)timespan.TotalSeconds * -secondsToDegrees);
        }
        else
        {
            DateTime time = DateTime.Now;
            clockHandHour.localRotation = Quaternion.Euler(0f, 0f, time.Hour * -hoursToDegrees);
            clockHandMinute.localRotation = Quaternion.Euler(0f, 0f, time.Minute * -minutesToDegrees);
            clockHandSecond.localRotation = Quaternion.Euler(0f, 0f, time.Second * -secondsToDegrees);
        }
    }
}
