using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // 효과음과 배경음을 재생할 AudioSource
    // [SerializeField] private AudioSource sfxSource; // 기존 단일 AudioSource 
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSourcePrefab;  // 풀링용 프리팹
    [SerializeField] private int poolSize = 10;  // 풀 크기

    [SerializeField] private List<SoundData> soundDataList;

    private Dictionary<string, SoundData> soundDictionary;
    private Dictionary<string, RandomSoundData> randomSoundDictionary;

    // 풀링 관련 변수들
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;
    private List<AudioSource> sleepMusicSources = new List<AudioSource>();

    private bool isMusicPlaying = false;
    private Coroutine fadeCoroutine;

    private float sfxVolume = 0.5f;
    [SerializeField] private float bgmVolume = 0.5f;

    private void Awake()
    {
        if (Instance == this)
        {
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxVolume = GameData.SettingInfo?.SFXSoundRatio ?? sfxVolume;
        bgmVolume = GameData.SettingInfo?.BGMSoundRatio ?? bgmVolume;

        InitializeAudioSourcePool(); // 풀 초기화 추가

        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);

        soundDictionary = new Dictionary<string, SoundData>();
        foreach (var sound in soundDataList)
        {
            if (!soundDictionary.ContainsKey(sound.soundName))
            {
                soundDictionary.Add(sound.soundName, sound);
            }
        }
    }

    void Start()
    {
        // InitializeAudioSourcePool(); // 풀 초기화 추가

        // SetBGMVolume(bgmVolume);
        // SetSFXVolume(sfxVolume);
        musicSource.Play();
    }

    // 풀 초기화
    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource audioSource = Instantiate(sfxSourcePrefab, transform);
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    // AudioSource 풀에서 가져오기
    private AudioSource GetAudioSourceFromPool()
    {
        if (audioSourcePool.Count == 0)
        {
            // 재생 완료된 AudioSource 회수
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                if (!activeAudioSources[i].isPlaying)
                {
                    AudioSource source = activeAudioSources[i];
                    activeAudioSources.RemoveAt(i);
                    source.gameObject.SetActive(false);
                    audioSourcePool.Enqueue(source);
                }
            }

            if (audioSourcePool.Count == 0)
            {
                Debug.LogWarning("No available audio sources in pool!");
                return null;
            }
        }

        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);
        activeAudioSources.Add(audioSource);
        return audioSource;
    }

    // AudioSource 풀로 반환
    private IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        if (activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Remove(audioSource);
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    /// <summary>
    /// 효과음을 재생 (풀링 버전)
    /// </summary>
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundData sound))
        {
            AudioSource audioSource = GetAudioSourceFromPool();
            if (audioSource == null) return;

            AudioClip randomClip = sound.clip[Random.Range(0, sound.clip.Count)];
            audioSource.clip = randomClip;
            audioSource.volume = sfxVolume * sound.volume; // SFX 볼륨 적용
            audioSource.loop = sound.loop;
            audioSource.Play();

            if (!sound.loop)
            {
                StartCoroutine(ReturnToPoolAfterPlay(audioSource, randomClip.length));
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }

    /* 기존 PlaySFX 코드
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundData sound))
        {
            AudioClip randomClip = sound.clip[Random.Range(0, sound.clip.Count)];
            sfxSource.clip = randomClip;
            sfxSource.volume = sfxVolume * sound.volume;
            sfxSource.loop = false;
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }
    */

    /// <summary>
    /// 브금 재생
    /// </summary>
    public void PlayBGM(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundData sound) && sound.loop)
        {
            AudioClip randomClip = sound.clip[Random.Range(0, sound.clip.Count)];
            musicSource.clip = randomClip;
            musicSource.volume = bgmVolume * sound.volume;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found or is not a BGM!");
        }
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        foreach (var source in activeAudioSources)
        {
            source.volume = sfxVolume;
        }
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        musicSource.volume = bgmVolume;
    }

    /// <summary>
    /// SFX 볼륨 반환
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// BGM 볼륨 반환
    /// </summary>
    public float GetBGMVolume() => bgmVolume;

    /// <summary>
    /// 음악 전환 (페이드아웃 + 페이드인)
    /// </summary>
    public void CrossfadeMusic(string newSoundName, float fadeDuration = 1.0f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(CrossfadeCoroutine(newSoundName, fadeDuration));
    }

    private IEnumerator CrossfadeCoroutine(string newSoundName, float fadeDuration)
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0;
        musicSource.Stop();

        if (soundDictionary.TryGetValue(newSoundName, out SoundData newSound))
        {
            AudioClip selectedClip = newSound.clip[Random.Range(0, newSound.clip.Count)];
            musicSource.clip = selectedClip;
            musicSource.volume = 0;
            musicSource.loop = true;
            musicSource.Play();

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
                yield return null;
            }
            musicSource.volume = startVolume;
        }
        else
        {
            Debug.LogWarning($"Sound '{newSoundName}' not found in the SoundDictionary!");
        }

        fadeCoroutine = null;
    }

    public void CrossfadeMusicForSleep(List<string> sleepSounds, string wakeSound, float sleepDuration, float fadeDuration = 1.0f)
    {
        StartCoroutine(SleepMusicSequence(sleepSounds, wakeSound, sleepDuration, fadeDuration));
    }

    private IEnumerator SleepMusicSequence(List<string> sleepSounds, string wakeSound, float sleepDuration, float fadeDuration)
    {
        if (sleepSounds == null || sleepSounds.Count == 0)
        {
            Debug.LogWarning("No sleep sounds provided!");
            yield break;
        }

        // 기존 음악 중지
        if (musicSource != null)
            musicSource.Stop();

        // 이전 잠자기 음악들 정리
        foreach (var source in sleepMusicSources.ToList()) // ToList로 복사본 사용
        {
            if (source != null)
            {
                Destroy(source);
            }
        }
        sleepMusicSources.Clear();

        // 각 잠자기 사운드에 대한 AudioSource 생성
        foreach (string soundName in sleepSounds)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found in dictionary!");
                continue;
            }

            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            if (newSource != null)
            {
                newSource.volume = 0;
                newSource.loop = true;
                sleepMusicSources.Add(newSource);
            }
        }

        // 각 사운드 순차적으로 페이드인
        for (int i = 0; i < sleepSounds.Count && i < sleepMusicSources.Count; i++)
        {
            if (soundDictionary.TryGetValue(sleepSounds[i], out SoundData sound) &&
                sleepMusicSources[i] != null)
            {
                AudioSource source = sleepMusicSources[i];
                if (sound.clip != null && sound.clip.Count > 0)
                {
                    AudioClip selectedClip = sound.clip[Random.Range(0, sound.clip.Count)];
                    if (selectedClip != null)
                    {
                        Debug.Log($"Selected sleep music: {selectedClip.name}");
                        source.clip = selectedClip;
                        source.Play();
                        float targetVolume = bgmVolume * sound.volume;
                        StartCoroutine(FadeIn(source, targetVolume, fadeDuration));

                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(sleepDuration);

        // 잠자기 사운드들 페이드아웃
        CleanupSleepMusic(fadeDuration);
    }

    public void CleanupSleepMusic(float fadeDuration)
    {
        foreach (var source in sleepMusicSources)
        {
            if (source != null) StartCoroutine(FadeOutAndDestroy(source, fadeDuration));
        }
        sleepMusicSources.Clear();
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration && source != null)  // null 체크 추가
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0, targetVolume, elapsedTime / duration);
            yield return null;
        }
        if (source != null)  // null 체크 추가
            source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, elapsedTime / duration);
            yield return null;
        }

        source.Stop();
        Destroy(source);
    }
}