using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; // The name we'll use to call the sound (e.g., "PlayerJump")

    public AudioClip clip; // The actual audio file

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool isLooping = false;

    // This allows us to hide the AudioSource reference in the inspector,
    // as it will be assigned at runtime.
    [HideInInspector]
    public AudioSource source;
}
