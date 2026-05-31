using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConversationStateManager : MonoBehaviour
{
    public enum State { Idle, Recording, Processing }

    public State currentState { get; private set; }

    public Button recordButton;
    public Button endButton;
    public TMP_Text talkText;
    public Image talkIcon;
    public Sprite micIcon;
    public Sprite messageIcon;

    public void SetState(State s)
    {
        currentState = s;
        Debug.Log($"[ConversationStateManager] SetState: {s}");

        switch (s)
        {
            case State.Idle:
                recordButton.interactable = true;
                endButton.interactable = false;
                talkText.text = "Start Talking";
                talkIcon.sprite = micIcon;
                break;

            case State.Recording:
                recordButton.interactable = false;
                endButton.interactable = true;
                talkText.text = "Listening...";
                talkIcon.sprite = micIcon;
                break;

            case State.Processing:
                recordButton.interactable = false;
                endButton.interactable = false;
                talkText.text = "AI is Talking...";
                talkIcon.sprite = messageIcon;
                break;
        }
    }

    public State GetCurrentState() => currentState;
}
