using UnityEngine;

/// <summary>
/// Toggles a light and plays a sound when flipped
/// </summary>

public class ToggleLight : MonoBehaviour
{
    [Tooltip("Controls the state of the light")]
    public bool isOn = false;

    [Tooltip("Sound to play when flipping the light")]
    public AudioClip toggleSound;

    private Light currentLight = null;
    private AudioSource audioSource = null;
    public ParticleSystem sparkleEffect; 
    private void Awake()
    {
        currentLight = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentLight.enabled = isOn;

        if (sparkleEffect != null)
        { 
            sparkleEffect.Play();
        }
    }

    public void TurnOn()
    {
        isOn = true;
        currentLight.enabled = isOn;
    }

    public void TurnOff()
    {
        isOn = false;
        currentLight.enabled = isOn;
    }

    public void Flip()
    {
        isOn = !isOn;
        currentLight.enabled = isOn;

        audioSource.PlayOneShot(toggleSound);
    }

    private void OnValidate()
    {
        if (currentLight)
            currentLight.enabled = isOn;
    }
}
