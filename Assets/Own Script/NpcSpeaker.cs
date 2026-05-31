using System.Collections.Generic;
using UnityEngine;

public class NpcSpeaker : MonoBehaviour
{
    public NpcInfo npcInfo;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip voiceClip;
    public AudioClip followupClip;

    [Tooltip("Other NPCs that should react when this NPC performs talent")]
    public List<NpcSpeaker> linkedNpcs = new List<NpcSpeaker>();

    [HideInInspector] public string LastLine;

    private bool isPerformingTalent = false;

    public void ReceiveLine(string text)
    {
        if (npcInfo == null) return;

        string name = npcInfo.GetNpcName();
        LastLine = text;

        Debug.Log($"[NpcSpeaker] {name} says: {text}");

        if (animator != null && !isPerformingTalent)
        {
            animator.SetTrigger("Talk");
            Debug.Log("Talk trigger sent to Animator");
        }

        if (audioSource && voiceClip)
            audioSource.PlayOneShot(voiceClip);
    }

    public void PerformTalent()
    {
        if (animator == null || npcInfo == null) return;

        isPerformingTalent = true;
        animator.applyRootMotion = false;

        switch (npcInfo.GetTalent())
        {
            case Talent.Dancing:
            case Talent.Acting:
                animator.CrossFade("Dance", 0f);
                break;
            case Talent.Singing:
                animator.CrossFade("Sing", 0f);
                break;
            case Talent.Painting:
                animator.CrossFade("Paint", 0f);
                break;
        }

        Debug.Log($"[NpcSpeaker] {npcInfo.GetNpcName()} is performing {npcInfo.GetTalent()}!");

        // Notify linked NPCs to react
        foreach (var npc in linkedNpcs)
        {
            if (npc != null && npc.animator != null)
            {
                audioSource.PlayOneShot(followupClip);
                npc.animator.SetTrigger("followUp");
                Debug.Log($"[NpcSpeaker] {npcInfo.GetNpcName()} triggered followUp on {npc.npcInfo.GetNpcName()}");
            }
        }

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(ResetPerformingTalent), animLength);
    }

    private void ResetPerformingTalent()
    {
        isPerformingTalent = false;
        Debug.Log("[NpcSpeaker] Talent finished, can talk again.");
    }
}
