using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class GameplayPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject exitButtonCanvas;
    public TMP_Text timerText;
    public float maxListeningTime = 12f;

    private float sessionTime = 0f;
    private bool isSessionActive = false;
    private bool isListening = false;
    private Coroutine timerCoroutine;
    private Coroutine listeningTimeoutCoroutine;

    [Header("Other scripts")]
    public WhisperSpeechToText whisperSTT;
    public TriggerZoneStartMenu triggerZoneStartMenu;
    public TTSManagerRaw tts;
    public GPTStudentResponse gptStudentResponse;
    public ConversationStateManager conversationStateManager;
    public UnityEvent StartSessionPlayClapAnimation;

    // --- Metrics tracking across whole session ---
    private float totalDuration = 0f;
    private int totalWordCount = 0;
    private int totalFillerCount = 0;
    private int transcriptionCount = 0;

    // --- Per-turn metrics (reset each turn) ---
    private float lastTurnDuration = 0f;
    private int lastTurnWordCount = 0;
    private int lastTurnFillerCount = 0;

    void Start()
    {
        if (tts != null) tts.OnTTSFinished += OnAIResponseFinished;

        conversationStateManager.SetState(ConversationStateManager.State.Idle);
        conversationStateManager.recordButton.onClick.AddListener(ToggleTalk);
        conversationStateManager.endButton.onClick.AddListener(EndTurn);
        whisperSTT.OnTranscriptionComplete += HandleTranscriptionComplete;
    }

    public void StartSession()
    {
        sessionTime = 0f;
        isSessionActive = true;
        StartSessionPlayClapAnimation?.Invoke();
        timerCoroutine = StartCoroutine(SessionTimer());
    }

    IEnumerator SessionTimer()
    {
        while (isSessionActive)
        {
            sessionTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(sessionTime / 60);
            int seconds = Mathf.FloorToInt(sessionTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            yield return null;
        }
    }

    void ToggleTalk()
    {
        if (conversationStateManager.GetCurrentState() == ConversationStateManager.State.Idle)
        {
            isListening = true;
            conversationStateManager.SetState(ConversationStateManager.State.Recording);
            whisperSTT.StartRecording();
            listeningTimeoutCoroutine = StartCoroutine(ListeningTimeoutCoroutine());
        }
        else
        {
            EndTurn();
        }
    }

    void EndTurn()
    {
        if (conversationStateManager.GetCurrentState() != ConversationStateManager.State.Recording)
            return;

        isListening = false;
        StopListening();
        whisperSTT.StopRecordingManually();

        conversationStateManager.SetState(ConversationStateManager.State.Processing);

        string result = whisperSTT.GetLastTranscription();
    }

    public void OnAIResponseFinished()
    {
        conversationStateManager.SetState(ConversationStateManager.State.Idle);
    }

    public void EndSession()
    {
        exitButtonCanvas.SetActive(true);
        isSessionActive = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        triggerZoneStartMenu.gameplayPanel.SetActive(false);
        triggerZoneStartMenu.perturnUIPanel.SetActive(false);
    }

    IEnumerator ListeningTimeoutCoroutine()
    {
        yield return new WaitForSeconds(maxListeningTime);
        if (isListening)
        {
            EndTurn();
        }
    }

    void StopListening()
    {
        if (listeningTimeoutCoroutine != null)
            StopCoroutine(listeningTimeoutCoroutine);
    }

    private void HandleTranscriptionComplete(string transcription)
    {
        Debug.Log("Whisper transcription complete: " + transcription);

        if (string.IsNullOrEmpty(transcription)) return;

        // --- Calculate metrics for this transcription ---
        string[] words = transcription.Split(' ');
        int wordCount = words.Length;

        string[] fillerWords = {
                "um", "uh", "er", "ah", "hmm",
                "like", "you know", "i mean", "actually",
                "basically", "literally", "seriously", "honestly",
                "sort of", "kind of", "maybe", "probably",
                "okay", "right", "well", "so",
                "yeah", "no", "haha", "i guess", "alright"
            };

        int fillerCount = 0;
        foreach (string w in words)
        {
            foreach (string f in fillerWords)
            {
                if (w.ToLower() == f) fillerCount++;
            }
        }

        float wpm = sessionTime > 0 ? (wordCount / (sessionTime / 60f)) : 0f;

        Debug.Log($"[Metrics] Duration: {sessionTime:F1}s, WordCount: {wordCount}, WPM: {wpm:F1}, FillerCount: {fillerCount}");

        // --- Update cumulative stats ---
        totalDuration += sessionTime;
        totalWordCount += wordCount;
        totalFillerCount += fillerCount;
        transcriptionCount++;

        // --- Save last turn stats ---
        lastTurnDuration = sessionTime;
        lastTurnWordCount = wordCount;
        lastTurnFillerCount = fillerCount;


        // --- Calculate aggregated metrics ---
        float avgWPM = totalDuration > 0 ? (totalWordCount / (totalDuration / 60f)) : 0f;
        float avgDuration = transcriptionCount > 0 ? totalDuration / transcriptionCount : 0f;
        float avgFillers = transcriptionCount > 0 ? (float)totalFillerCount / transcriptionCount : 0f;

        Debug.Log($"[Cumulative Metrics] TotalDuration: {totalDuration:F1}s, " +
                  $"TotalWords: {totalWordCount}, AvgWPM: {avgWPM:F1}, " +
                  $"TotalFillers: {totalFillerCount}" +
                  $"AvgDuration: {avgDuration:F1}s, AvgFillersPerTurn: {avgFillers:F1}");

        // --- Pass both per-turn metrics and cumulative summary to GPT ---
        gptStudentResponse.GenerateResponse(transcription, sessionTime, wpm, fillerCount);
    }

    // --- Public getters for cumulative metrics ---
    public float GetTotalDuration() => totalDuration;
    public int GetTotalWordCount() => totalWordCount;
    public int GetTotalFillerCount() => totalFillerCount;
    public int GetTranscriptionCount() => transcriptionCount;

    // --- Public getters for last turn metrics ---
    public float GetTurnDuration() => lastTurnDuration;
    public int GetTurnWordCount() => lastTurnWordCount;
    public int GetTurnFillerCount() => lastTurnFillerCount;

    public float GetAverageWPM()
    {
        return totalDuration > 0 ? (totalWordCount / (totalDuration / 60f)) : 0f;
    }

    public float GetAverageDuration()
    {
        return transcriptionCount > 0 ? totalDuration / transcriptionCount : 0f;
    }

    public float GetAverageFillersPerTurn()
    {
        return transcriptionCount > 0 ? (float)totalFillerCount / transcriptionCount : 0f;
    }

}
