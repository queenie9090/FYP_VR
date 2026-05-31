using System.Collections.Generic;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    [DisallowMultipleComponent, AddComponentMenu("Ready Player Me/Voice Handler Multiple", 0)]
    public class VoiceHandlerMultiple : VoiceHandler
    {
        [Header("Multiple Audio Clips")]
        public List<AudioClip> AudioClips = new List<AudioClip>();

        public void PlayAudioClip(int index)
        {
            if (AudioClips != null && index >= 0 && index < AudioClips.Count)
            {
                base.PlayAudioClip(AudioClips[index]);
            }
        }
        public AudioClip PlayRandomAudioClip()
        {
            if (AudioClips != null && AudioClips.Count > 0)
            {
                int randomIndex = Random.Range(0, AudioClips.Count);
                AudioClip chosenClip = AudioClips[randomIndex];
                base.PlayAudioClip(chosenClip);
                return chosenClip;
            }

            return null;
        }

    }
}
