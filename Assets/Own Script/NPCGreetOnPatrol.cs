using ReadyPlayerMe.Core;
using UnityEngine;

public class NPCGreetOnPatrol : MonoBehaviour
{
    [Header("References")]
    public NPCFovHallway npcFov;             // Reference to your FOV script
    public VoiceHandlerMultiple voiceHandler;
    public AudioClip hiClip;

    [Header("Settings")]
    public bool greetOnce = true;            // Only say hi once
    public float greetCooldown = 10f;        // If greetOnce = false, use cooldown

    private bool hasGreeted = false;
    private float lastGreetTime = -999f;

    void Update()
    {
        if (npcFov == null || voiceHandler == null || hiClip == null) return;

        // Check if player is detected within FOV + range
        if (npcFov.player != null && npcFov.IsPlayerInFovAndRangePublic())
        {
            if (greetOnce && !hasGreeted)
            {
                PlayGreeting();
                hasGreeted = true;
            }
            else if (!greetOnce && Time.time >= lastGreetTime + greetCooldown)
            {
                PlayGreeting();
                lastGreetTime = Time.time;
            }
        }
    }

    private void PlayGreeting()
    {
        voiceHandler.PlayAudioClip(hiClip);
        Debug.Log($"{gameObject.name} greets the player!");
    }
}
