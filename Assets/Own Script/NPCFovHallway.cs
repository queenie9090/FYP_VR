using UnityEngine;
using System.Collections;
using ReadyPlayerMe.Core;

public class NPCFovHallway : NPCFov
{
    [Header("Hallway NPC Animator Settings")]
    public Animator npcAnimator;
    private bool hasGreeted = false;
    private bool isGreeting = false;
    private bool isAgreeing = false;

    [Header("Audio Settings")]
    public VoiceHandlerMultiple voiceHandler;
    public AudioClip greetClip;

    private Coroutine idleCycleCoroutine;

    protected override void Start()
    {
        base.Start();

        if (npcAnimator != null)
        {
            idleCycleCoroutine = StartCoroutine(IdleAgreeCycle());
        }
    }

    protected override void Update()
    {
        base.Update();

        // Only greet if: not greeted before + in FOV + within seeRange
        if (!hasGreeted && IsPlayerInFovAndRange())
        {
            hasGreeted = true;

            if (!isAgreeing)
            {
                if (idleCycleCoroutine != null)
                {
                    StopCoroutine(idleCycleCoroutine);
                }

                StartCoroutine(PlayGreeting());
            }
            else
            {
                StartCoroutine(WaitAndGreet());
            }
        }
    }

    private IEnumerator WaitAndGreet()
    {
        while (isAgreeing)
        {
            yield return null;
        }

        if (idleCycleCoroutine != null)
        {
            StopCoroutine(idleCycleCoroutine);
        }

        yield return PlayGreeting();
    }

    private IEnumerator PlayGreeting()
    {
        isGreeting = true;

        npcAnimator.SetTrigger("Greet");

        float clipLength = 2f;
        if (voiceHandler != null && greetClip != null)
        {
            clipLength = greetClip.length;
            voiceHandler.PlayAudioClip(greetClip);
        }

        yield return new WaitForSeconds(clipLength);

        npcAnimator.SetBool("isAgreeing", false);

        isGreeting = false;

        idleCycleCoroutine = StartCoroutine(IdleAgreeCycle());
    }

    private IEnumerator IdleAgreeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);

            if (npcAnimator != null && !isGreeting)
            {
                if (!isAgreeing)
                {
                    isAgreeing = true;
                    npcAnimator.SetBool("isAgreeing", true);

                    if (voiceHandler != null)
                    {
                        AudioClip clip = voiceHandler.PlayRandomAudioClip();
                        if (clip != null)
                        {
                            yield return new WaitForSeconds(clip.length);
                        }
                    }

                    npcAnimator.SetBool("isAgreeing", false);
                    isAgreeing = false;
                }
            }
        }
    }

    // Modified FOV check: angle + distance
    private bool IsPlayerInFovAndRange()
    {
        if (player == null || npcHead == null) return false;

        Vector3 toPlayer = (player.position - npcHead.position).normalized;
        float angle = Vector3.Angle(npcHead.forward, toPlayer);

        float distance = Vector3.Distance(player.position, npcHead.position);

        return angle <= fieldOfView && distance <= viewRange;
    }

    public bool IsPlayerInFovAndRangePublic()
    {
        return IsPlayerInFovAndRange();
    }

}
