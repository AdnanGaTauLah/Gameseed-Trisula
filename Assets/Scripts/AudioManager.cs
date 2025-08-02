using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [Header("Sound Library")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Plays a sound effect once at a specific position in the world.
    /// Perfect for jumps, explosions, UI clicks, etc.
    /// </summary>
    public void PlaySound(string soundName, Vector3 position)
    {
        Sound s = soundLibrary.GetSound(soundName);
        if (s == null || s.isLooping)
        {
            if (s != null && s.isLooping) Debug.LogWarning($"AudioManager: Cannot play '{soundName}' as a one-shot. It is marked as a looping sound.");
            return;
        }

        // Create a temporary GameObject to host the sound
        GameObject soundObject = new GameObject("TempAudio");
        soundObject.transform.position = position;
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();

        // Configure and play the sound
        audioSource.clip = s.clip;
        audioSource.volume = s.volume * sfxVolume * masterVolume;
        audioSource.pitch = s.pitch;
        audioSource.Play();

        // Destroy the temporary object after the clip has finished playing
        Destroy(soundObject, s.clip.length);
    }

    /// <summary>
    /// Starts playing a looping sound on an AudioSource provided by another object.
    /// Perfect for running, wall-sliding, etc.
    /// </summary>
    public void StartLoopingSound(AudioSource source, string soundName)
    {
        if (source == null)
        {
            Debug.LogError("AudioManager: Provided AudioSource is null.");
            return;
        }

        Sound s = soundLibrary.GetSound(soundName);
        if (s == null || !s.isLooping)
        {
            if (s != null && !s.isLooping) Debug.LogWarning($"AudioManager: Cannot play '{soundName}' as a loop. It is not marked as a looping sound.");
            return;
        }

        // Configure the provided AudioSource
        source.clip = s.clip;
        source.volume = s.volume * sfxVolume * masterVolume;
        source.pitch = s.pitch;
        source.loop = true;
        source.Play();
    }

    /// <summary>
    /// Stops a looping sound on a specific AudioSource.
    /// </summary>
    public void StopLoopingSound(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
        }
    }
}
