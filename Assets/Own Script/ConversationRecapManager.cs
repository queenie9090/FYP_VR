using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ConversationRecapManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text topicText;
    public Transform messageContent;
    public GameObject messageSentPrefab;
    public GameObject messageReceivedPrefab;
    public GameObject messageSuggestionPrefab;

    [Header("Buttons")]
    public Button suggestResponseButton;

    [Header("Test Input")]
    [TextArea(2, 5)] public string testPlayerMessage;
    [TextArea(2, 5)] public string testAIResponse;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip messageReceivedClip;
    public AudioClip suggestionClip;

    [Header("Reference")]
    public TopicManager topicManager;
    [SerializeField] private WhisperSpeechToText whisper;

    private string lastUserMessage;
    void Start()
    {
        whisper.OnTranscriptionComplete += HandlePlayerTranscription;
        SetTopic(topicManager.GetSelectedTopicTitle());
        suggestResponseButton.onClick.AddListener(OnSuggestResponse);
    }

    public void SetTopic(string topic)
    {
        topicText.text = $" {topic}";
    }

    public void AddPlayerMessage(string message)
    { 
        GameObject newMsg = Instantiate(messageSentPrefab, messageContent);
        newMsg.GetComponentInChildren<TMP_Text>().text = message;
    }

    public void AddAIMessage(string message)
    {
        audioSource.PlayOneShot(messageReceivedClip);
        GameObject newMsg = Instantiate(messageReceivedPrefab, messageContent);
        newMsg.GetComponentInChildren<TMP_Text>().text = message; 
    }

    public void AddSuggestionMessage(string message)
    {
        audioSource.PlayOneShot(messageReceivedClip);
        GameObject newMsg = Instantiate(messageSuggestionPrefab, messageContent);
        newMsg.GetComponentInChildren<TMP_Text>().text = message;
    }

    private async void OnSuggestResponse()
    {
        audioSource.PlayOneShot(suggestionClip);

        string topicTitle = topicManager != null ? topicManager.GetSelectedTopicTitle() : "General Topic";

        string lastUserMessageForPrompt = !string.IsNullOrEmpty(lastUserMessage)
        ? lastUserMessage
        : "Hello";


        string suggestionPrompt =
            $"The conversation topic is {topicTitle}.\n" +
            $"The user just said: \"{lastUserMessageForPrompt}\".\n" +
            "Suggest one short, natural follow-up question or reply that the user could say next. " +
            "Keep it conversational, casual, and different from earlier suggestions. " +
            "Do not write explanations, only the raw suggested sentence.";

        var messages = new System.Collections.Generic.List<ChatMessage>
    {
        new ChatMessage { Role = "system", Content = suggestionPrompt}
    };

        var request = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages,
            Temperature = 0.9f
        };

        var response = await new OpenAIApi().CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            string suggestion = response.Choices[0].Message.Content.Trim();
            AddSuggestionMessage($"Suggestion: {suggestion}");
        }
        else
        {
            AddSuggestionMessage("Could not generate a suggestion right now.");
        }
    }
    private void HandlePlayerTranscription(string transcription)
    {
        Debug.Log("Player said: " + transcription);
        lastUserMessage = transcription;
    }

}
