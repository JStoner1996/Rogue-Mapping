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
    public void PlayModifiedSound(AudioSource sound)
    {
        sound.pitch = Random.Range(0.5f, 1.5f);
        sound.Stop();
        sound.Play();
    }
}
