using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [Header("Defaults")]
    [SerializeField] private SoundData defaultMusic;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<AudioSource> pool = new Queue<AudioSource>();
    private readonly List<AudioSource> activeSources = new List<AudioSource>();
    private readonly Dictionary<AudioSource, int> sourcePlaybackVersions = new Dictionary<AudioSource, int>();

    [Header("Mixer Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Library")]
    [SerializeField] private AudioLibrary audioLibrary;

    private Dictionary<SoundType, SoundData> soundMap;

    private void Awake()
    {
        if (!TryInitializeSingleton(persistAcrossScenes: true))
        {
            return;
        }

        BuildSoundMap();
        CreatePool();
        ConfigureMusicSource();
    }

    private void Start()
    {
        HandleSceneMusic(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene);
    }

    private void HandleSceneMusic(Scene scene)
    {
        SoundData sceneMusic = scene.name == SceneCatalog.Game
            ? GetLibrarySound(SoundType.GameMusic) ?? defaultMusic
            : defaultMusic;

        PlayMusic(sceneMusic);
    }

    private void BuildSoundMap()
    {
        soundMap = new Dictionary<SoundType, SoundData>();

        if (audioLibrary == null || audioLibrary.sounds == null)
        {
            return;
        }

        for (int i = 0; i < audioLibrary.sounds.Count; i++)
        {
            AudioLibrary.SoundEntry entry = audioLibrary.sounds[i];

            if (!soundMap.ContainsKey(entry.type))
            {
                soundMap.Add(entry.type, entry.sound);
            }
        }
    }

    private void ConfigureMusicSource()
    {
        if (musicSource != null && musicMixerGroup != null)
        {
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        }
    }

    private void CreatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSource();
        }
    }

    private AudioSource CreateNewSource()
    {
        GameObject go = new GameObject("AudioSource_Pooled");
        go.transform.parent = transform;

        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        ResetSource(source);
        pool.Enqueue(source);
        return source;
    }

    private AudioSource GetSource()
    {
        if (pool.Count == 0)
        {
            return CreateNewSource();
        }

        return pool.Dequeue();
    }

    private void ReturnToPool(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        ResetSource(source);
        activeSources.Remove(source);
        pool.Enqueue(source);
    }

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
        if (sound == null || sound.clip == null)
        {
            return;
        }

        AudioSource source = GetSource();
        PrepareSource(source);

        float volume = sound.randomizeVolume
            ? Random.Range(sound.volumeRange.x, sound.volumeRange.y)
            : sound.volume;

        float pitch = sound.randomizePitch
            ? Random.Range(sound.pitchRange.x, sound.pitchRange.y)
            : 1f;

        source.clip = sound.clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = sound.loop;

        activeSources.Add(source);
        int playbackVersion = GetNextPlaybackVersion(source);

        source.Play();

        if (!sound.loop)
        {
            StartCoroutine(ReturnWhenFinished(source, playbackVersion));
        }
    }

    public void PlayMusic(SoundData music)
    {
        if (musicSource == null || music == null || music.clip == null)
        {
            return;
        }

        if (musicSource.clip == music.clip && musicSource.isPlaying)
        {
            return;
        }

        if (musicMixerGroup != null)
        {
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        }

        musicSource.clip = music.clip;
        musicSource.volume = music.volume;
        musicSource.pitch = 1f;
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

    public void StopAllSounds()
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeSources[i]);
        }
    }

    private SoundData GetLibrarySound(SoundType type)
    {
        return soundMap != null && soundMap.TryGetValue(type, out SoundData sound) ? sound : null;
    }

    private void PrepareSource(AudioSource source)
    {
        ResetSource(source);
    }

    private int GetNextPlaybackVersion(AudioSource source)
    {
        int nextVersion = sourcePlaybackVersions.TryGetValue(source, out int currentVersion)
            ? currentVersion + 1
            : 1;

        sourcePlaybackVersions[source] = nextVersion;
        return nextVersion;
    }

    private IEnumerator ReturnWhenFinished(AudioSource source, int playbackVersion)
    {
        yield return new WaitWhile(() => source != null && source.isPlaying);

        if (source == null)
        {
            yield break;
        }

        if (!sourcePlaybackVersions.TryGetValue(source, out int currentVersion) || currentVersion != playbackVersion)
        {
            yield break;
        }

        ReturnToPool(source);
    }

    private void ResetSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.pitch = 1f;
        source.volume = 1f;
        source.outputAudioMixerGroup = sfxMixerGroup;
    }
}
