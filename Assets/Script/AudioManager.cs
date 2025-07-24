using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM / 효과음")]
    public AudioClip bgmClip;
    public AudioClip gameOverClip;
    public AudioClip gameClearClip;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // BGM Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f;

        // SFX Source (효과음용)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1.0f;
    }

    // ------------------
    // BGM 재생
    // ------------------
    public void PlayBGM()
    {
        if (bgmClip == null || (bgmSource.clip == bgmClip && bgmSource.isPlaying)) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ------------------
    // 효과음 재생
    // ------------------
    public void PlayGameOver()
    {
        if (gameOverClip != null && !sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(gameOverClip);
        }
    }

    public void PlayGameClear()
    {
        if (gameClearClip != null && !sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(gameClearClip);
        }
    }
}
