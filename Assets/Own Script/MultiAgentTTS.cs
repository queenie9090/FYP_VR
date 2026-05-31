using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public class NpcVoice
{
    public string npcName;
    public string voice;
    public AudioSource audioSource;
}

public class MultiAgentTTS : MonoBehaviour
{
    public TTSManagerRaw ttsManager;
    public List<NpcVoice> npcVoices = new List<NpcVoice>();

    private Dictionary<string, NpcVoice> npcLookup;

    private void Awake()
    {
        npcLookup = new Dictionary<string, NpcVoice>();
        foreach (var npc in npcVoices)
            npcLookup[npc.npcName] = npc;
    }

    public async Task SpeakAsNpc(string npcName, string text)
    {
        if (!npcLookup.TryGetValue(npcName, out var npc))
        {
            Debug.LogWarning($"NPC {npcName} not found in voice list.");
            return;
        }

        await ttsManager.SpeakText(text, npc.audioSource, npc.voice);
    }
}