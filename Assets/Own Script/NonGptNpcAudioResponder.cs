using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe.Core;

public class NonGptNpcAudioResponder : MonoBehaviour
{
    [Header("Audio Settings")]
    public VoiceHandler voiceHandler;
    public List<AudioClip> reactionClips = new List<AudioClip>();

    [Header("Timing Settings")]
    public float delayAfterTtsEnds = 0.5f;

    /// <summary>
    /// This function should be called when GPT NPC finishes TTS.
    /// </summary>
    public void OnGptTtsFinished()
    {
        if (reactionClips.Count > 0 && voiceHandler != null)
        {
            int randomIndex = Random.Range(0, reactionClips.Count);
            AudioClip chosenClip = reactionClips[randomIndex];
            StartCoroutine(PlayReactionAfterDelay(chosenClip));
        }
    }

    private IEnumerator PlayReactionAfterDelay(AudioClip clip)
    {
        yield return new WaitForSeconds(delayAfterTtsEnds);
        voiceHandler.PlayAudioClip(clip);
    }
}
