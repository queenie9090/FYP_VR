using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZoneStartMenu : MonoBehaviour
{
    [Header("Assign the Start Menu UI Panel")]
    public GameObject startMenuPanel;
    [Header("Assign the Tutorial Canvas")]
    public GameObject tutorialCanvas;
    [Header("Assign Topic Selection Panel")]
    public GameObject topicSelectionPanel;
    public GameObject conversationRecapPanel;
    [Header("Gameplay Panel")]
    public GameObject gameplayPanel;
    public GameObject locationWaypoint;
    public GameObject endWaypoint;
    public GameObject perturnUIPanel;
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip soundCue;
    public AudioClip celebrateCue;
    public AudioClip onGameStartCheers;
    [Header("Countdown Settings")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public AudioClip countdownBeep;
    [Header("Post Speech Panels")]
    public GameObject calculateResultPanel;
    public GameObject feedbackSummaryPanel;
    public GameObject EndSessionNotification;

    [Header("Accessing other scripts")]
    public PlayerProgressUI progressPlayerUI;
    public GameplayPanelManager gameplayManager;
    public FeedbackSummaryAnnouncer feedbackSummaryAnnouncer;
    public UIFadeOut uiFadeOut;

    public UnityEvent SessionPlayClapAnimation;

    private bool hasStarted = false;
    private bool hasTriggeredZoneOnce = false;
    private bool sessionEnded = false;
    private void Start()
    {
        if (locationWaypoint != null) locationWaypoint.SetActive(true);

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        locationWaypoint.SetActive(false);

        if ((tutorialCanvas !=null && tutorialCanvas.activeSelf))
        return;

        if (!hasTriggeredZoneOnce)
        { 
            progressPlayerUI.NextStep();
            hasTriggeredZoneOnce = true;

            if (audioSource && soundCue)
                audioSource.PlayOneShot(soundCue);

            startMenuPanel.SetActive(true);
        }
        else if(!hasStarted) 
        {
            startMenuPanel.SetActive(true);    
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (sessionEnded) return;
        locationWaypoint.SetActive(true);

        if (other.CompareTag("Player"))
            startMenuPanel.SetActive(false);
    }

    public void StartLecture()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            progressPlayerUI.NextStep();
            audioSource.PlayOneShot(soundCue);
            startMenuPanel.SetActive(false);
            topicSelectionPanel.SetActive(true);
        }
    }

    public void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
        startMenuPanel.SetActive(false);
    }

    public void OnConfirmTopic()
    {
        progressPlayerUI.NextStep();

        topicSelectionPanel.SetActive(false);

        audioSource.PlayOneShot(soundCue);

        StartCoroutine(CountdownToSpeech(3f));

    }

    public void ShowGameplayPanel()
    {
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);

        if(perturnUIPanel !=null)
            perturnUIPanel.SetActive(true);

        gameplayManager.StartSession();
        //SessionPlayClapAnimation?.Invoke();

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

        if (conversationRecapPanel != null)
            conversationRecapPanel.SetActive(true);
        audioSource.PlayOneShot(onGameStartCheers);

        ShowGameplayPanel();
    }

    public void EndSessionAndShowFeedback()
    {
        sessionEnded = true;
        gameplayManager.EndSession();
        progressPlayerUI.NextStep();
        audioSource.PlayOneShot(soundCue);
        StartCoroutine(ShowFeedbackRoutine());
    }

    private IEnumerator ShowFeedbackRoutine()
    {

        if (conversationRecapPanel != null)
            conversationRecapPanel.SetActive(false);

        if(calculateResultPanel != null)
            calculateResultPanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        SessionPlayClapAnimation?.Invoke();

        if (celebrateCue != null)
        {
            audioSource.PlayOneShot(celebrateCue);
        }

        if (calculateResultPanel != null)
            calculateResultPanel.SetActive(false);
        
        if (feedbackSummaryPanel != null)
            feedbackSummaryPanel.SetActive(true);
        
        if(EndSessionNotification !=null)
            EndSessionNotification.SetActive(true);

        uiFadeOut.StartFadeOut();

        if (locationWaypoint!=null)
            locationWaypoint.SetActive(false);

        if(endWaypoint!=null)
            endWaypoint.SetActive(true);

        feedbackSummaryAnnouncer.ShowReceiveButton();
    }
}
