using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FeedbackSummaryAnnouncer : MonoBehaviour
{
    [Header("UI Hookups")]
    [SerializeField] private Button receiveButton;
    [SerializeField] private GameObject buttonObject;
    [SerializeField] private GameObject statusObject;
    [SerializeField] private TMP_Text statusText;
    public AudioSource karinaAudioSource;

    [Header("Other scripts")]
    public GameplayPanelManager panelManager;
    public TTSManagerRaw tts;
    public Animator npcAnimator;

    private OpenAIApi openai = new OpenAIApi();
    private bool hasSpoken = false;

    private string cachedSummary = "";

    private void Start()
    {
        if (receiveButton != null)
            receiveButton.onClick.AddListener(OnReceiveClicked);

        if (buttonObject != null)
            buttonObject.SetActive(false); // Hide until EndSession

        if (statusObject != null)
            statusObject.SetActive(false);
    }

    public void ShowReceiveButton()
    {
        if (buttonObject != null)
            buttonObject.SetActive(true);
    }

    public void ShowStatusObject()
    {
        if (statusObject != null)
            statusObject.SetActive(true);
    }

    private async void OnReceiveClicked()
    {
        if (hasSpoken)
        {
            Debug.Log("[FeedbackSummaryAnnouncer] Already spoken, skipping new GPT request.");
            if (!string.IsNullOrEmpty(cachedSummary))
                await tts.SpeakText(cachedSummary);
            return;
        }

        if (panelManager == null) return;

        float avgDuration = panelManager.GetAverageDuration();
        float avgWords = panelManager.GetTotalWordCount() / (float)Mathf.Max(1, panelManager.GetTranscriptionCount());
        float avgFillers = panelManager.GetAverageFillersPerTurn();

        string prompt =
            "You are a supportive teacher summarizing a student's speaking performance.\n" +
            "Based on the averages:\n" +
            $"- Average Duration per turn: {avgDuration:F1} seconds\n" +
            $"- Average Words per turn: {avgWords:F1}\n" +
            $"- Average Fillers per turn: {avgFillers:F1}\n\n" +
            "Create a short spoken summary in 2 to 3 friendly sentences, encouraging improvement.";

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = "You are a friendly teacher giving concise spoken feedback." },
            new ChatMessage { Role = "user", Content = prompt }
        };

        var completionRequest = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        };

        if (statusText != null) statusText.text = "Generating feedback...";

        var response = await openai.CreateChatCompletion(completionRequest);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            string summary = response.Choices[0].Message.Content.Trim();
            cachedSummary = summary; // Save for replays
            hasSpoken = true;

            if (statusText != null) statusText.text = summary;

            if (npcAnimator != null)
                npcAnimator.SetTrigger("Talk");

            await tts.SpeakText(summary, karinaAudioSource);
        }
        else
        {
            if (statusText != null) statusText.text = "No summary available.";
        }

        if (buttonObject != null)
            buttonObject.SetActive(false);

        ShowStatusObject();
    }
}
