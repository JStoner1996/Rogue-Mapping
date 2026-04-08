using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public AudioClip clip;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomizeVolume = false;
    public Vector2 volumeRange = new Vector2(0.8f, 1f);

    [Header("Pitch")]
    public bool randomizePitch = true;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Loop")]
    public bool loop = false;
}