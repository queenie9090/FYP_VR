using OpenAI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class backup : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private WorldSetting worldSetting;
    [SerializeField] private NpcInfo[] npcAgents;
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform messageContent;
    [SerializeField] private GameObject messageSentPrefab;
    [SerializeField] private GameObject messageReceivedPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip messageReceivedClip;

    [Header("Access scripts")]
    [SerializeField] private TriggerMultiAgentZone triggerMultiAgentZone;

    private OpenAIApi openai = new OpenAIApi();
    private bool isSessionEnded = false;
    private string lastNpcMessage = "";
    private NpcInfo lastNpcAgent = null;

    void Start()
    {
        sendButton.onClick.AddListener(OnUserSendMessage);
    }

    private void OnUserSendMessage()
    {
        if (isSessionEnded) return;

        string userMessage = userInputField.text.Trim();
        if (string.IsNullOrWhiteSpace(userMessage)) return;

        Debug.Log("[User Message] " + userMessage);
        ShowMessage(userMessage, true);
        userInputField.text = "";

        NpcInfo targetedAgent = null;
        foreach (var agent in npcAgents)
        {
            if (userMessage.ToLower().Contains(agent.GetNpcName().ToLower()))
            {
                targetedAgent = agent;
                break;
            }
        }

        if (targetedAgent != null)
        {
            Debug.Log("[Targeted NPC] " + targetedAgent.GetNpcName());
            string prompt = GenerateUserPrompt(userMessage, targetedAgent, true);
            StartCoroutine(SendToGPT(prompt, targetedAgent));
        }
        else
        {
            StartCoroutine(NpcReactToUser(userMessage));
        }
    }

    private IEnumerator NpcReactToUser(string userMessage)
    {
        Debug.Log("[Flow] NPC Reacting to user");

        List<NpcInfo> shuffledAgents = new List<NpcInfo>(npcAgents);
        Shuffle(shuffledAgents);

        // First NPC responds
        NpcInfo firstResponder = shuffledAgents[0];
        string prompt = GenerateUserPrompt(userMessage, firstResponder, false);
        yield return SendToGPT(prompt, firstResponder);

        yield return new WaitForSeconds(1f);
        if (isSessionEnded) yield break;

        // Second NPC responds to the first, if not the same
        if (shuffledAgents.Count > 1)
        {
            NpcInfo secondResponder = shuffledAgents[1];
            if (secondResponder == lastNpcAgent) yield break;

            string npcToNpcPrompt = GenerateNpcToNpcPrompt(secondResponder, lastNpcAgent, lastNpcMessage);
            yield return SendToGPT(npcToNpcPrompt, secondResponder);

            yield return new WaitForSeconds(1f);
            if (isSessionEnded) yield break;

            string followUpPrompt = GenerateFollowUpToUserPrompt(secondResponder, lastNpcMessage);
            yield return SendToGPT(followUpPrompt, secondResponder);
        }
    }

    private string GenerateUserPrompt(string userMessage, NpcInfo agent, bool isTargeted)
    {
        string intro = worldSetting.GetPrompt() + "\n" + agent.GetPrompt();
        string instruction =
            $"You're a university student. Always reply briefly in 1 sentence. " +
            $"Stay in character, based on your personality, occupation, and talent. " +
            $"If the user directly refers to your name, respond to them using their name. " +
            $"Never say you are an NPC. If something is outside your knowledge, admit you don’t know. " +
            $"If the user seems to end the conversation, reply with a friendly goodbye and silently append END_CONVERSATION.";

        return $"{intro}\n{instruction}\nUser: {userMessage}";
    }

    private string GenerateNpcToNpcPrompt(NpcInfo newAgent, NpcInfo previousAgent, string previousMessage)
    {
        if (newAgent == previousAgent) return "";

        Debug.Log($"[GenerateNpcToNpcPrompt] {newAgent.GetNpcName()} responding to {previousAgent.GetNpcName()}");

        string intro = worldSetting.GetPrompt() + "\n" + newAgent.GetPrompt();
        string instruction =
            $"You're chatting with another student named {previousAgent.GetNpcName()}. Reply in 1 short sentence. " +
            $"Stay in character and do not use emotional descriptions. Do not say you are an NPC. " +
            $"Respond based on your personality, occupation, and talents.";

        return $"{intro}\n{instruction}\n{previousAgent.GetNpcName()}: \"{previousMessage}\"";
    }

    private string GenerateFollowUpToUserPrompt(NpcInfo agent, string lastMessage)
    {
        Debug.Log($"[GenerateFollowUpToUserPrompt] {agent.GetNpcName()} asking user");

        string intro = worldSetting.GetPrompt() + "\n" + agent.GetPrompt();
        string instruction =
            $"Now ask the user a short and simple follow-up question in 1 sentence, to involve them. " +
            $"Start naturally with 'By the way,' or 'How about you?'. " +
            $"Do not use emotional tone. Stay in character.";

        return $"{intro}\n{instruction}\nContext: \"{lastMessage}\"";
    }

    private IEnumerator SendToGPT(string prompt, NpcInfo agent)
    {
        if (string.IsNullOrWhiteSpace(prompt)) yield break;

        Debug.Log($"[SendToGPT] Prompt for {agent.GetNpcName()}:\n{prompt}");

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = prompt }
        };

        var request = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        };

        var gptTask = openai.CreateChatCompletion(request);
        yield return new WaitUntil(() => gptTask.IsCompleted);

        var response = gptTask.Result;
        if (response.Choices != null && response.Choices.Count > 0)
        {
            string reply = response.Choices[0].Message.Content.Trim();
            Debug.Log($"[GPT Reply from {agent.GetNpcName()}] {reply}");

            ShowMessage(reply.Replace("END_CONVERSATION", "").Trim(), false, agent);

            lastNpcMessage = reply;
            lastNpcAgent = agent;

            audioSource?.PlayOneShot(messageReceivedClip);

            if (reply.Contains("END_CONVERSATION"))
            {
                Debug.Log("[Trigger] Detected END_CONVERSATION, ending session...");
                isSessionEnded = true;
                triggerMultiAgentZone.EndSession();
            }
        }
    }

    private void ShowMessage(string message, bool isUser, NpcInfo agent = null)
    {
        GameObject msg = Instantiate(isUser ? messageSentPrefab : messageReceivedPrefab, messageContent);
        TMP_Text textComponent = msg.GetComponentInChildren<TMP_Text>();

        if (textComponent != null)
        {
            if (!isUser && agent != null)
                textComponent.text = $"<b>{agent.GetNpcName()}</b>\n{message}";
            else
                textComponent.text = message;
        }

        if (!isUser && agent != null && agent.agentAvatar != null)
        {
            Transform avatarTransform = msg.transform.Find("AvatarIcon") ??
                System.Array.Find(msg.GetComponentsInChildren<Transform>(), t => t.CompareTag("AvatarIcon"));

            if (avatarTransform != null)
            {
                Image avatarImage = avatarTransform.GetComponent<Image>();
                if (avatarImage != null)
                {
                    avatarImage.sprite = agent.agentAvatar;
                }
            }
        }
    }

    public void SendMessageFromKeyboard()
    {
        OnUserSendMessage();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
