using UnityEngine;
using System; // Required for Array.Find

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public Sound[] sounds;

    /// <summary>
    /// Finds and returns the Sound object with the matching name.
    /// </summary>
    /// <param name="soundName">The name of the sound to find.</param>
    /// <returns>The Sound object, or null if not found.</returns>
    public Sound GetSound(string soundName)
    {
        // Use Array.Find to search our list of sounds for one with a matching name.
        Sound s = Array.Find(sounds, sound => sound.name == soundName);

        if (s == null)
        {
            Debug.LogWarning("SoundLibrary: Sound '" + soundName + "' not found!");
            return null;
        }

        return s;
    }
}
