using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackSummaryUI : MonoBehaviour
{
    // Cache AI suggestions by metric name
    private System.Collections.Generic.Dictionary<string, string> metricSuggestionsCache
        = new System.Collections.Generic.Dictionary<string, string>();

    [Header("Link Gameplay Manager")]
    public GameplayPanelManager panelManager;

    [Header("Panels")]
    public GameObject turnFeedbackPanel;
    public GameObject overallFeedbackPanel;
    public GameObject feedbackDetailPanel;

    [Header("Circular Sliders")]
    public Slider overallDurationSlider;
    public Slider overallWordSlider;
    public Slider overallFillerSlider;

    [Header("Per Turn Circular Sliders")]
    public Slider turnDurationSlider;
    public Slider turnWordSlider;
    public Slider turnFillerSlider;

    [Header("Detail Circular Slider")]
    public Slider detailMetricSlider;

    [Header("Slider Settings")]
    public float durationMax = 60f;
    public float wordsMax = 50f;
    public float fillerMax = 10f;
    public float sliderSmoothSpeed = 5f;

    // ====== PER TURN FEEDBACK ======
    [Header("Per Turn UI References")]
    public TMP_Text turnDurationTitle;
    public TMP_Text turnDurationValue;
    public Button turnDurationButton;

    public TMP_Text turnWordTitle;
    public TMP_Text turnWordValue;
    public Button turnWordButton;

    public TMP_Text turnFillerTitle;
    public TMP_Text turnFillerValue;
    public Button turnFillerButton;

    // ====== OVERALL FEEDBACK ======
    [Header("Overall UI References")]
    public TMP_Text overallDurationTitle;
    public TMP_Text overallDurationValue;
    public Button overallDurationButton;

    public TMP_Text overallWordTitle;
    public TMP_Text overallWordValue;
    public Button overallWordButton;

    public TMP_Text overallFillerTitle;
    public TMP_Text overallFillerValue;
    public Button overallFillerButton;

    // ====== DETAILS FEEDBACK ======
    [Header("Details Panel UI")]
    public TMP_Text detailTitleText;
    public TMP_Text detailValueText;
    public Button returnButton;

    [Header("Suggestions Scroll UI")]
    public TMP_Text suggestionsHeaderText;
    public TMP_Text suggestionsContentText;
    public GPTStudentResponse gptStudentResponse;

    private float detailTargetValue = 0f;

    void Start()
    {
        // Titles for per-turn
        turnDurationTitle.text = "Turn Duration";
        turnWordTitle.text = "Turn Words";
        turnFillerTitle.text = "Turn Fillers";

        // Titles for overall
        overallDurationTitle.text = "Total Duration";
        overallWordTitle.text = "Total Words";
        overallFillerTitle.text = "Total Fillers";

        // Button events (turn feedback)
        turnDurationButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(panelManager.GetTurnDuration() / durationMax);
            ShowDetail("Turn Duration", $"{panelManager.GetTurnDuration():F1}s", normalized);
        });

        turnWordButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(panelManager.GetTurnWordCount() / wordsMax);
            ShowDetail("Turn Words", panelManager.GetTurnWordCount().ToString(), normalized);
        });

        turnFillerButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(1f - (panelManager.GetTurnFillerCount() / fillerMax));
            ShowDetail("Turn Fillers", panelManager.GetTurnFillerCount().ToString(), normalized);
        });

        // Button events (overall feedback)
        overallDurationButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(panelManager.GetAverageDuration() / durationMax);
            ShowDetail("Average Duration per Turn", $"{panelManager.GetAverageDuration():F1}s", normalized);
            RequestAISuggestions("Average Duration per Turn", panelManager.GetAverageDuration());
        });

        overallWordButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(panelManager.GetAverageWPM() / wordsMax);
            ShowDetail("Average Words per Min", $"{panelManager.GetAverageWPM():F1}", normalized);
            RequestAISuggestions("Average Words per Min", panelManager.GetAverageWPM());
        });

        overallFillerButton.onClick.AddListener(() =>
        {
            float normalized = Mathf.Clamp01(1f - (panelManager.GetAverageFillersPerTurn() / fillerMax));
            ShowDetail("Average Fillers per Turn", $"{panelManager.GetAverageFillersPerTurn():F1}", normalized);
            RequestAISuggestions("Average Fillers per Turn", panelManager.GetAverageFillersPerTurn());
        });
    }

    void UpdatePerTurnSliders()
    {
        float targetDuration = Mathf.Clamp01(panelManager.GetTurnDuration() / durationMax);
        float targetWords = Mathf.Clamp01(panelManager.GetTurnWordCount() / wordsMax);
        float targetFiller = Mathf.Clamp01(1f - (panelManager.GetTurnFillerCount() / fillerMax));

        turnDurationSlider.value = Mathf.Lerp(turnDurationSlider.value, targetDuration, Time.deltaTime * sliderSmoothSpeed);
        turnWordSlider.value = Mathf.Lerp(turnWordSlider.value, targetWords, Time.deltaTime * sliderSmoothSpeed);
        turnFillerSlider.value = Mathf.Lerp(turnFillerSlider.value, targetFiller, Time.deltaTime * sliderSmoothSpeed);
    }

    void UpdateCircularSliders()
    {
        if (panelManager == null) return;

        // Target values normalized 0-1
        float targetDuration = Mathf.Clamp01(panelManager.GetAverageDuration() / durationMax);
        float targetWords = Mathf.Clamp01(panelManager.GetAverageWPM() / wordsMax);
        float targetFiller = Mathf.Clamp01(1f - (panelManager.GetAverageFillersPerTurn() / fillerMax));

        // Smoothly animate sliders using Lerp
        overallDurationSlider.value = Mathf.Lerp(overallDurationSlider.value, targetDuration, Time.deltaTime * sliderSmoothSpeed);
        overallWordSlider.value = Mathf.Lerp(overallWordSlider.value, targetWords, Time.deltaTime * sliderSmoothSpeed);
        overallFillerSlider.value = Mathf.Lerp(overallFillerSlider.value, targetFiller, Time.deltaTime * sliderSmoothSpeed);
    }

    void Update()
    {
        if (panelManager == null) return;

        // Live update per-turn feedback
        turnDurationValue.text = $"{panelManager.GetTurnDuration():F1}s";
        turnWordValue.text = panelManager.GetTurnWordCount().ToString();
        turnFillerValue.text = panelManager.GetTurnFillerCount().ToString();

        // Live update overall feedback
        overallDurationValue.text = $"{panelManager.GetTotalDuration():F1}s";
        overallWordValue.text = panelManager.GetTotalWordCount().ToString();
        overallFillerValue.text = panelManager.GetTotalFillerCount().ToString();

        // Sliders
        UpdatePerTurnSliders();
        UpdateCircularSliders();

        // Animate detail metric slider if panel is active
        if (feedbackDetailPanel.activeSelf)
        {
            detailMetricSlider.value = Mathf.Lerp(detailMetricSlider.value, detailTargetValue, Time.deltaTime * sliderSmoothSpeed);
        }
    }

    void ShowDetail(string title, string value, float normalizedScore)
    {
        detailTitleText.text = title;
        detailValueText.text = value;

        detailTargetValue = normalizedScore;

        feedbackDetailPanel.SetActive(true);
        turnFeedbackPanel.SetActive(false);
        overallFeedbackPanel.SetActive(false);
    }

    public void ReturnToSummary()
    {
        feedbackDetailPanel.SetActive(false);
        overallFeedbackPanel.SetActive(true);
    }

    void RequestAISuggestions(string category, float score)
    {
        // If already have a cached suggestion, just display it
        if (metricSuggestionsCache.ContainsKey(category))
        {
            suggestionsContentText.text = metricSuggestionsCache[category];
            suggestionsHeaderText.text = "Suggestions";
            return;
        }

        if (gptStudentResponse == null) return;

        string prompt = $"Provide 2–3 concise suggestions (2-3 sentences) for the student " +
                        $"based on the following metric:\n" +
                        $"Metric: {category}\n" +
                        $"Score: {score:F1}\n" +
                        $"Use encouraging tone and actionable advice. " +
                        $"Return only text suitable for a feedback scroll view.";

        gptStudentResponse.GenerateFeedbackSuggestions(prompt, (result) =>
        {
            // Cache the result
            metricSuggestionsCache[category] = result;

            // Display in scroll content
            suggestionsContentText.text = result;
            suggestionsHeaderText.text = "Suggestions";
        });
    }
}