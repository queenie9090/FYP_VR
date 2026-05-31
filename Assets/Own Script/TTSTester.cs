using UnityEngine;

public class TTSTester : MonoBehaviour
{
    public TTSManagerRaw tts;

    async void Start()
    {
        await tts.SpeakText("I'll never find another love like this");
    }
}
