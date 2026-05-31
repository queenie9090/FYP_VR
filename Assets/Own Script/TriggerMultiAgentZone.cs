using TMPro;
using UnityEngine;
using System.Collections;
public class TriggerMultiAgentZone : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMultiagentPanel;
    public GameObject tutorialPanel;
    public GameObject gameplaySelection;
    public GameObject conversationRecapPanel;
    public GameObject locationWaypoint;
    public GameObject endlocationWaypoint;
    public GameObject textInputField;
    public GameObject startSessionNotification;
    public GameObject endSessionNotification;
    [SerializeField] private GameObject exitButtonCanvas;
    [SerializeField] private GameObject endsessionPanelButton;
    [SerializeField] private GameObject gameStartUI;

    [Header("Gameplay Triggers")]
    //public GameObject subtitleGameplayContainer;
    public GameObject keyboardInputSystem;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip soundCue;
    public AudioClip countdownBeep;

    [Header("Countdown Settings")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    [Header("Reference other script")]
    public UIFadeOut uiFadeOut;

    private bool hasStarted = false;
    private bool hasTriggeredZoneOnce = false;
    private bool sessionEnded = false;
    void Start()
    {
        if (locationWaypoint != null) locationWaypoint.SetActive(true);
        if (endlocationWaypoint != null) endlocationWaypoint.SetActive(false);
        if (startMultiagentPanel != null) startMultiagentPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (conversationRecapPanel != null) conversationRecapPanel.SetActive(false);
        if (gameplaySelection != null) gameplaySelection.SetActive(false);
        //if (subtitleGameplayContainer != null) subtitleGameplayContainer.SetActive(false);
        if (keyboardInputSystem != null) keyboardInputSystem.SetActive(false);
        if (textInputField != null) textInputField.SetActive(false);
        if (gameStartUI != null) gameStartUI.SetActive(false);
        uiFadeOut.StartFadeOut();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        locationWaypoint.SetActive(false);

        if (tutorialPanel != null && tutorialPanel.activeSelf)
            return;

        if (!hasTriggeredZoneOnce)
        {
            hasTriggeredZoneOnce = true;

            if (audioSource && soundCue)
                audioSource.PlayOneShot(soundCue);

            startMultiagentPanel.SetActive(true);
        }
        else if (!hasStarted)
        {
            startMultiagentPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (sessionEnded) return;
            locationWaypoint.SetActive(true);
            startMultiagentPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        if (!hasStarted)
        {
            hasStarted = true;

            if (audioSource && soundCue) audioSource.PlayOneShot(soundCue);

            startMultiagentPanel.SetActive(false);
            gameplaySelection.SetActive(true);
        }
    }

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        startMultiagentPanel.SetActive(false);
    }

    public void ReturnButton()
    {
        gameplaySelection.SetActive(false);
        startMultiagentPanel.SetActive(true);
        hasStarted = false;
    }

    private IEnumerator CountdownToSpeech(float delay)
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        if (audioSource && countdownBeep)
            audioSource.PlayOneShot(countdownBeep);

        for (int i = (int)delay; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.text = "Begin!";

        yield return new WaitForSeconds(1f);

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        ShowRecapUIAfterCountdown();
    }


    // Option 1: Recap gameplay
    public void StartConversationRecapMode()
    {
        if (audioSource && soundCue)
            audioSource.PlayOneShot(soundCue);
        exitButtonCanvas.SetActive(false);
        gameplaySelection.SetActive(false);
        StartCoroutine(CountdownToSpeech(3f));
    }

    // Option 2: Subtitles gameplay
    public void StartSutitleMode()
    {
        gameplaySelection.SetActive(false);
        //subtitleGameplayContainer.SetActive(true);
        EnableKeyboardInput();
    }

    private void EnableKeyboardInput()
    {
        if (keyboardInputSystem != null)
        { 
            keyboardInputSystem.SetActive(true);
        }
    }

    private void ShowRecapUIAfterCountdown()
    {
        if (conversationRecapPanel != null)
            conversationRecapPanel.SetActive(true);

        if (textInputField != null)
            textInputField.SetActive(true);

        if (gameStartUI != null) gameStartUI.SetActive(true);

        EnableKeyboardInput();
    }

    public void EndSession()
    {
        sessionEnded = true;
        if (audioSource && soundCue)
            audioSource.PlayOneShot(soundCue);

        endsessionPanelButton.SetActive(false);
        exitButtonCanvas.SetActive(true);
        locationWaypoint.SetActive(false);
        endlocationWaypoint.SetActive(true);
        keyboardInputSystem.SetActive(false);
        textInputField.SetActive(false);
        if (gameStartUI != null) gameStartUI.SetActive(false);
        if (endSessionNotification != null)
            endSessionNotification.SetActive(true);
        uiFadeOut.StartFadeOut();
    }
}
