using UnityEngine;
using System.Collections;
public class MicrophoneCapture : MonoBehaviour
{
    public AudioClip recordedClip;
    public int maxRecordingTime = 10;
    public string microphoneDevice;

    public void StartRecording()
    {
        microphoneDevice = Microphone.devices[0];
        recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingTime, 44100);
        Debug.Log("Recording started...");
    }

    public AudioClip StopRecording()
    { 
        Microphone.End(microphoneDevice);
        Debug.Log("Recording Stopped!");
        return recordedClip;
    }
}
