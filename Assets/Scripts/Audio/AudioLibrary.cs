using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public SoundType type;
        public SoundData sound;
    }

    public List<SoundEntry> sounds;
}