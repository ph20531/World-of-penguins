using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("MASTER VOLUME")]
    [Range(0f, 1f)]
    public float masterVolume = 0.5f;

    [Header("BACKGROUND MUSICS SETTINGS")]
    public List<AudioClip> backgroundMusicClips = new List<AudioClip>();

    [Range(0f, 1f)]
    public float backgroundMusicVolume = 1f;

    [Header("ENVIRONMENTAL SOUNDS SETTINGS")]
    public List<AudioClip> EnvironmentalSoundClips = new List<AudioClip>();

    [Range(0f, 1f)]
    public float environmentalSoundVolume = 1f;

    [Header("SOUND EFFECTS SETTINGS")]
    public List<AudioClip> soundEffectClips = new List<AudioClip>();
    public List<string> soundEffectAdjustNames = new List<string>();

    [Range(0f, 1f)]
    public float soundEffectVolume = 1f;
    
    private int backgroundMusicIndex = 0;
    private int poolIndex = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        PlayEnvironmentalSounds();
        
        backgroundMusicIndex = Random.Range(0, backgroundMusicClips.Count);
        PlayBackgroundMusic();
    }

    void PlayEnvironmentalSounds()
    {
        if(EnvironmentalSoundClips.Count == 0 || EnvironmentalSoundClips == null)
        {
            Debug.LogError("There is no environmental sound!");
            return;
        }

        foreach(AudioClip clip in EnvironmentalSoundClips)
        {
            GameObject obj = new GameObject("environmental_sound_" + clip.name);
            obj.transform.parent = transform;
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = environmentalSoundVolume * masterVolume;
            source.Play();
        }
    }

    void PlayBackgroundMusic()
    {
        if(backgroundMusicClips.Count == 0 || backgroundMusicClips == null)
        {
            Debug.LogError("There is no background music!");
            return;
        }

        AudioClip clip = backgroundMusicClips[backgroundMusicIndex];

        if (clip != null)
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.clip = clip;
            source.volume = backgroundMusicVolume * masterVolume;
            source.Play();
            StartCoroutine(PlayNextBackgroundMusicAfterClipFinished(source));
        }
        else
        {
            Debug.LogError("There is no background music!");
        }
    }

    IEnumerator PlayNextBackgroundMusicAfterClipFinished(AudioSource source)
    {
        // Wait until the clip has finished playing
        yield return new WaitForSeconds(source.clip.length);
        backgroundMusicIndex++;
        if(backgroundMusicClips.Count <= backgroundMusicIndex) backgroundMusicIndex = 0;
        PlayBackgroundMusic();
    }

    public void PlaySoundEffect(string clipName)
    {
        AudioClip clip = null;

        if (soundEffectAdjustNames.Count > 0 && EnvironmentalSoundClips != null)
        {
            foreach (string name in soundEffectAdjustNames)
            {
                if (clipName.Contains(name))
                    clip = soundEffectClips.Find(c => c.name == name);
            }
        }

        if (clip == null)
            clip = soundEffectClips.Find(c => clipName.Contains(c.name));

        if (clip != null)
        {
            // Check if any AudioSource is currently playing the same clip
            bool isClipPlaying = false;
            foreach (AudioSource audioSource in GetComponentsInChildren<AudioSource>())
            {
                if (audioSource.clip == clip && audioSource.isPlaying)
                {
                    isClipPlaying = true;
                    break;
                }
            }

            if (!isClipPlaying)
            {
                // Check if any AudioSource is available
                AudioSource source = GetAvailableSoundEffectSource();
                if (source != null)
                {
                    source.clip = clip;
                    source.volume = soundEffectVolume * masterVolume;
                    source.Play();
                    StartCoroutine(DestroyAudioSourceAfterClipFinished(source));
                }
            }
            else
            {
                Debug.LogWarning("Sound effect clip '" + clipName + "' is already playing.");
            }
        }
        else
        {
            Debug.LogError("Sound effect clip '" + clipName + "' not found!");
        }
    }

    IEnumerator DestroyAudioSourceAfterClipFinished(AudioSource source)
    {
        // Wait until the clip has finished playing
        yield return new WaitForSeconds(source.clip.length);
        // Once the clip has finished playing, remove the AudioSource from the pool and destroy the GameObject
        poolIndex--;
        Destroy(source.gameObject);
    }

    AudioSource GetAvailableSoundEffectSource()
    {
        // If all sources are currently in use, create a new one
        GameObject obj = new GameObject("sound_effect_" + poolIndex);
        obj.transform.parent = transform;
        AudioSource newSource = obj.AddComponent<AudioSource>();
        poolIndex++;
        return newSource;
    }
}