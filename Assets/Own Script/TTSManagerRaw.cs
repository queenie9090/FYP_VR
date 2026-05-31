using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using OpenAI;

public class TTSManagerRaw : MonoBehaviour
{
    [SerializeField] private AudioSource defaultAudioSource;
    private string apiKey;

    public event Action OnTTSFinished;

    private void Awake()
    {
        var config = new Configuration();

        if (!string.IsNullOrEmpty(config.Auth.ApiKey))
        {
            apiKey = config.Auth.ApiKey;
        }
        else
        {
            Debug.LogError("API key missing. Please check your auth.json file.");
        }
    }

    /// <summary>
    /// Speak text with a specific NPC's AudioSource (or default if null).
    /// </summary>
    public async Task SpeakText(string text, AudioSource npcAudioSource = null, string voice = "sage")
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key missing.");
            return;
        }

        // Decide which audio source to use
        AudioSource targetSource = npcAudioSource ?? defaultAudioSource;
        if (targetSource == null)
        {
            Debug.LogError("No AudioSource available for TTS playback.");
            return;
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = new Dictionary<string, object>()
            {
                { "model", "gpt-4o-mini-tts" },
                { "voice", voice },
                { "input", text }
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/audio/speech", content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"TTS failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return;
            }

            byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();

            // Save to temp file
            string tempPath = System.IO.Path.Combine(Application.persistentDataPath, "tts.mp3");
            System.IO.File.WriteAllBytes(tempPath, audioBytes);

            using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
            {
                var op = www.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Audio load error: {www.error}");
                    return;
                }

                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                if (clip == null)
                {
                    Debug.LogError("Failed to decode audio clip.");
                    return;
                }

                // Play on the NPC's AudioSource
                targetSource.clip = clip;
                targetSource.Play();
                Debug.Log($"TTS playback started on {targetSource.gameObject.name}: Length={clip.length}s");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(clip.length));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("TTS wait interrupted: " + ex.Message);
                }

                OnTTSFinished?.Invoke();
                Debug.Log("TTS playback finished.");
            }
        }
    }
}
