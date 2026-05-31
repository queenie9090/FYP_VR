using OpenAI;
using Samples.Whisper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WhisperSpeechToText : MonoBehaviour
{
    [SerializeField] private Button startRecordingButton;
    [SerializeField] private GameObject EndSessionButton;
    [SerializeField] private Image timeLeftBar;
    [SerializeField] private GameObject messageSentPrefab;
    [SerializeField] private Transform messageContentParent;

    private string lastTranscription = "";
    private readonly string fileName = "output.wav";
    private readonly int duration = 12;

    private AudioClip clip;
    private bool isRecording;
    private float time;

    private OpenAIApi openai = new OpenAIApi();

    public Action<string> OnTranscriptionComplete;

    private void Start()
    {
        startRecordingButton.onClick.AddListener(StartRecording);
    }

    public void StartRecording()
    {
        if (EndSessionButton != null) EndSessionButton.SetActive(false);

        isRecording = true;
        startRecordingButton.interactable = false;
        clip = Microphone.Start(null, false, duration, 44100);
    }

    private async void EndRecording()
    {
        Microphone.End(null);

        // Show temporary "Transcribing..." bubble
        GameObject temp = Instantiate(messageSentPrefab, messageContentParent);
        TMP_Text tempText = temp.GetComponentInChildren<TMP_Text>();
        tempText.text = "Transcribing...";

        byte[] data = SaveWav.Save(fileName, clip);

        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = fileName },
            Model = "whisper-1",
            Language = "en"
        };

        var res = await openai.CreateAudioTranscription(req);

        lastTranscription = res.Text;
        tempText.text = res.Text;
        timeLeftBar.fillAmount = 0;

        // Notify whoever is listening (GameplayPanelManager)
        OnTranscriptionComplete?.Invoke(lastTranscription);
    }

    private void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;
            timeLeftBar.fillAmount = time / duration;

            if (time >= duration)
            {
                isRecording = false;
                time = 0;
                EndRecording();
            }
        }
    }

    public string GetLastTranscription()
    {
        return lastTranscription;
    }

    public void ResetTimeLeftBar()
    {
        timeLeftBar.fillAmount = 0;
    }

    public void StopRecordingManually()
    {
        if (!isRecording) return;

        isRecording = false;
        time = 0f;
        Microphone.End(null);
        ResetTimeLeftBar();

        EndRecording();
    }
}
