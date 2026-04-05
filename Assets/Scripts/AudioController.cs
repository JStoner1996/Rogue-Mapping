using UnityEngine;

public class AudioController : MonoBehaviour
{

    public static AudioController Instance;

    public AudioSource pause;
    public AudioSource unpause;
    public AudioSource enemyDie;
    public AudioSource selectUpgrade;
    public AudioSource levelUp;
    public AudioSource areaWeaponSpawn;
    public AudioSource gameOver;
    public AudioSource heal;
    public AudioSource bomb;
    public AudioSource getExp;
    public AudioSource magnet;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public void PlaySound(AudioSource sound)
    {
        sound.Stop();
        sound.Play();
    }
    public void PlayModifiedSound(AudioSource sound, float minPitch = 0.5f, float maxPitch = 1.5f)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        sound.pitch = Mathf.Clamp(pitch, 0.1f, 3f);

        sound.Stop();
        sound.Play();
    }
}
