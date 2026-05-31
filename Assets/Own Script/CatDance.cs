using UnityEngine;

public class NPCTriggerAnimation : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 2f;       
    public Transform player;           

    [Header("Animation & Sound")]
    public Animator animator;               
    public string triggerName = "Dance";   
    public AudioSource audioSource;         
    public AudioClip soundClip;           

    private bool playerInside = false;   

    void Update()
    {
        if (player == null || animator == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!playerInside && distance <= detectionRange)
        {
            playerInside = true;

            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);

            if (audioSource != null && soundClip != null)
            {
                audioSource.PlayOneShot(soundClip);
            }
        }

        if (playerInside && distance > detectionRange)
        {
            playerInside = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
