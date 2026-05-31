using OpenAI;
using Samples.Whisper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WhisperSpeechToTextMultiAgent : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startSpeakingButton;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Image timeBarFill;
    [SerializeField] private GameObject messageSentPrefab;
    [SerializeField] private GameObject EndSessionButton;
    [SerializeField] private Transform messageContentParent;

    [Header("Recording Settings")]
    [SerializeField] private int maxListeningTime = 12;
    private string fileName = "multiAgentOutput.wav";

    private OpenAIApi openai = new OpenAIApi();

    private AudioClip clip;
    private bool isRecording;
    private float timer;
    private string lastTranscription = "";

    public Action<string> OnTranscriptionComplete;

    private void Start()
    {
        startSpeakingButton.onClick.AddListener(StartRecording);
        endTurnButton.onClick.AddListener(StopRecordingManually);

        // At start, disable End Turn
        endTurnButton.interactable = false;
    }

    private void StartRecording()
    {
        if (isRecording) return;
        if (EndSessionButton != null) EndSessionButton.SetActive(false);
        Debug.Log("[Whisper STT] Start Recording...");
        isRecording = true;
        timer = 0f;

        startSpeakingButton.interactable = false;
        endTurnButton.interactable = true;

        clip = Microphone.Start(null, false, maxListeningTime, 44100);
    }

    private async void EndRecording()
    {
        isRecording = false;
        Debug.Log("[Whisper STT] End Recording, sending to Whisper...");

        Microphone.End(null);

        // Disable UI during transcription
        startSpeakingButton.interactable = false;
        endTurnButton.interactable = false;

        // Convert recorded audio to bytes
        byte[] data = SaveWav.Save(fileName, clip);

        // Send to Whisper
        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = fileName },
            Model = "whisper-1",
            Language = "en"
        };

        var res = await openai.CreateAudioTranscription(req);

        lastTranscription = res.Text;

        // Reset time bar
        ResetTimeBar();

        // Re-enable buttons AFTER transcription finishes
        startSpeakingButton.interactable = true;
        endTurnButton.interactable = false;

        // Notify whoever is listening (e.g. MultiAgentManager)
        OnTranscriptionComplete?.Invoke(lastTranscription);
    }

    private void Update()
    {
        if (!isRecording) return;

        timer += Time.deltaTime;
        timeBarFill.fillAmount = timer / maxListeningTime;

        if (timer >= maxListeningTime)
        {
            Debug.Log("[Whisper STT] Max listening time reached.");
            isRecording = false;
            EndRecording();
        }
    }

    public void StopRecordingManually()
    {
        if (!isRecording) return;
        ResetTimeBar();
        Debug.Log("[Whisper STT] Stop Recording Manually.");
        isRecording = false;
        EndRecording();
    }

    private void ResetTimeBar()
    {
        timer = 0f;
        timeBarFill.fillAmount = 0f;
    }

    public string GetLastTranscription()
    {
        return lastTranscription;
    }

    public void DisableButtonsDuringNpcResponse()
    {
        startSpeakingButton.interactable = false;
        endTurnButton.interactable = false;
    }
    public void ReturnToIdleButtonState()
    {
        startSpeakingButton.interactable = true;
        endTurnButton.interactable = false;
    }

}
