using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Defaults")]
    [SerializeField] private SoundData defaultMusic;
    [SerializeField] private GameObject audioManagerPrefab;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 10;

    private Queue<AudioSource> pool = new Queue<AudioSource>();
    private List<AudioSource> activeSources = new List<AudioSource>();

    [Header("Mixer Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Library")]
    [SerializeField] private AudioLibrary audioLibrary;

    private Dictionary<SoundType, SoundData> soundMap;


    public static void EnsureExists()
    {
        if (Instance != null) return;

        var prefab = Resources.Load<GameObject>("AudioManager");

        if (prefab != null)
        {
            Instantiate(prefab);
        }
        else
        {
            Debug.LogError("AudioManager prefab missing in Resources folder!");
        }
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSoundMap();
        CreatePool();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Scene-based music control
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "Game")
        {
            PlayMusic(audioLibrary.sounds.Find(s => s.type == SoundType.GameMusic)?.sound);
        }
        else
        {
            PlayMusic(defaultMusic);
        }
    }

    // Build the dictionary for quick sound lookup
    void BuildSoundMap()
    {
        soundMap = new Dictionary<SoundType, SoundData>();

        foreach (var entry in audioLibrary.sounds)
        {
            if (!soundMap.ContainsKey(entry.type))
            {
                soundMap.Add(entry.type, entry.sound);
            }
        }
    }

    // Pooling system for sound effects
    void CreatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSource();
        }
    }

    AudioSource CreateNewSource()
    {
        GameObject go = new GameObject("AudioSource_Pooled");
        go.transform.parent = transform;

        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;

        source.outputAudioMixerGroup = sfxMixerGroup;

        pool.Enqueue(source);
        return source;
    }

    AudioSource GetSource()
    {
        if (pool.Count == 0)
        {
            return CreateNewSource();
        }

        return pool.Dequeue();
    }

    void ReturnToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.loop = false;

        activeSources.Remove(source);
        pool.Enqueue(source);
    }

    // Play a sound by type
    public void Play(SoundType type)
    {
        if (!soundMap.TryGetValue(type, out SoundData sound))
        {
            Debug.LogWarning($"Sound not found: {type}");
            return;
        }

        PlaySound(sound);
    }

    public void PlaySound(SoundData sound)
    {
        AudioSource source = GetSource();

        // Volume
        float volume = sound.randomizeVolume
            ? Random.Range(sound.volumeRange.x, sound.volumeRange.y)
            : sound.volume;

        // Pitch
        float pitch = sound.randomizePitch
            ? Random.Range(sound.pitchRange.x, sound.pitchRange.y)
            : 1f;

        source.pitch = pitch;
        source.loop = sound.loop;

        activeSources.Add(source);

        source.PlayOneShot(sound.clip, volume);
    }

    IEnumerator ReturnWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        ReturnToPool(source);
    }

    // Music control
    public void PlayMusic(SoundData music)
    {
        if (musicSource == null || music == null) return;

        if (musicSource.clip == music.clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = music.clip;
        musicSource.volume = music.volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Global Control
    public void StopAllSounds()
    {
        foreach (var source in activeSources.ToArray())
        {
            ReturnToPool(source);
        }
    }
}