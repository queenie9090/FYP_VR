using UnityEngine;

public class CorridorTriggerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject uiPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip triggerSound; 


    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (uiPanel != null)
                uiPanel.SetActive(true);

            if (audioSource != null && triggerSound != null)
                audioSource.PlayOneShot(triggerSound);

        }
    }
}
