using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GPTStudentResponse : MonoBehaviour
{
    [SerializeField] private WhisperSpeechToText whisperSTT;
    [SerializeField] private GameObject messageReceivedPrefab;
    [SerializeField] private Transform messageContentParent;
    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button endRecordingButton;
    [SerializeField] private GameObject suggestResponseButton;
    [SerializeField] private GameObject EndSessionButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource rachealAudioSource;
    public AudioClip messageReceivedClip;
    public AudioClip suggestionClip;

    [Header("Other scripts")]
    public TopicManager topicManager;
    public TriggerZoneStartMenu triggerZoneStartMenu;
    public TTSManagerRaw tts;
    public ConversationStateManager conversationStateManager;
    public UnityEvent OnReplyReceived;

    private OpenAIApi openai = new OpenAIApi();
    public async void GenerateResponse(string transcription, float duration, float wpm, int fillerCount)
    {
        if (string.IsNullOrEmpty(transcription))
        {
            Debug.LogWarning("No transcription provided to send to GPT.");
            return;
        }

        // Log metrics for verification
        Debug.Log($"[GPTStudentResponse] Generating response with metrics -> Duration: {duration:F1}s, WPM: {wpm:F1}, Fillers: {fillerCount}");

        // Set UI/state to processing right away
        if (conversationStateManager != null)
        {
            Debug.Log("[GPTStudentResponse] SetState -> Processing");
            conversationStateManager.SetState(ConversationStateManager.State.Processing);
        }
        else
        {
            // fallback: disable buttons directly
            if (startRecordingButton != null) startRecordingButton.interactable = false;
            if (endRecordingButton != null) endRecordingButton.interactable = false;
        }

        string topicTitle = topicManager != null ? topicManager.GetSelectedTopicTitle() : "General Topic";

        string dynamicPrompt =
            "You are a curious student in a VR classroom listening to a presentation.\n" +
            $"The topic of the presentation is: {topicTitle}.\n" +
            "Here are the speaker's metrics from this session:\n" +
            $"- Duration: {duration:F1} seconds\n" +
            $"- Words per minute: {wpm:F1}\n" +
            $"- Filler words used: {fillerCount}\n\n" +
            "Only ask questions or make remarks that are directly related to this topic.\n" +
            "Respond with short, relevant questions or remarks based on the presenter’s speech.\n" +
            "Do not break character, do not mention you are an AI.\n" +
            "If my reply indicates that I want to end the conversation, do not ask any further questions. " +
            "Instead, reply with a short friendly farewell like: 'It was nice to hear your presentation, see you around!' " +
            "and end your message with the phrase END_CONVO.\n\n";

        var messages = new System.Collections.Generic.List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = dynamicPrompt },
            new ChatMessage { Role = "user", Content = transcription }
        };

        var completionRequest = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        };

        // Disable both buttons while waiting for GPT & TTS
        if (startRecordingButton != null) startRecordingButton.interactable = false;
        if (endRecordingButton != null) endRecordingButton.interactable = false;

        var response = await openai.CreateChatCompletion(completionRequest);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            string reply = response.Choices[0].Message.Content.Trim();
            Debug.Log("GPT Response: " + reply);

            bool hasEndConvo = reply.Contains("END_CONVO");
            if (hasEndConvo)
            {
                reply = reply.Replace("END_CONVO", "").Trim();
            }

            OnReplyReceived?.Invoke();
            audioSource.PlayOneShot(messageReceivedClip);

            GameObject msg = Instantiate(messageReceivedPrefab, messageContentParent);
            TMP_Text txt = msg.GetComponentInChildren<TMP_Text>();
            txt.text = reply;

            if (tts != null && !string.IsNullOrEmpty(reply))
            {
                // Wait until TTS finishes (SpeakText now waits for playback end)
                await tts.SpeakText(reply, rachealAudioSource);

                // After TTS finishes, transition back to Idle (unless conversation is ending)
                if (!hasEndConvo)
                {
                    Debug.Log("[GPTStudentResponse] TTS finished: returning to Idle");
                    if (conversationStateManager != null)
                        conversationStateManager.SetState(ConversationStateManager.State.Idle);

                    if (EndSessionButton != null) EndSessionButton.SetActive(true);
                    else
                    {
                        conversationStateManager.SetState(ConversationStateManager.State.Processing);
                    }
                }
            }

            if (hasEndConvo)
            {
                suggestResponseButton.SetActive(false);
                EndSessionButton.SetActive(false);
                conversationStateManager.SetState(ConversationStateManager.State.Processing);
                // Give a small delay after speaking before ending session
                Invoke(nameof(TriggerEndSession), 5f);
            }
        }
        else
        {
            Debug.LogWarning("No response received from GPT.");
            if (startRecordingButton != null) startRecordingButton.interactable = true;
            if (endRecordingButton != null) endRecordingButton.interactable = false;
        }
    }

    public async void GenerateFeedbackSuggestions(string prompt, System.Action<string> callback)
    {
        if (string.IsNullOrEmpty(prompt)) return;

        var messages = new System.Collections.Generic.List<ChatMessage>
    {
        new ChatMessage { Role = "system", Content = "You are a helpful tutor providing feedback to a VR student." },
        new ChatMessage { Role = "user", Content = prompt }
    };

        var completionRequest = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        };

        var response = await openai.CreateChatCompletion(completionRequest);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            string reply = response.Choices[0].Message.Content.Trim();
            callback?.Invoke(reply);
        }
        else
        {
            callback?.Invoke("No suggestions available.");
        }
    }

    private void TriggerEndSession()
    {
        if (triggerZoneStartMenu != null)
        {
            triggerZoneStartMenu.EndSessionAndShowFeedback();
        }
    }
}
