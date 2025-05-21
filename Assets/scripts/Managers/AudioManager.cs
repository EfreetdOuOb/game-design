using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    
    [Range(0f, 1f)]
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    
    public bool loop = false;
    
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("音效設置")]
    [SerializeField] private Sound[] sounds;
    
    [Header("混音器")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    
    private void Awake()
    {
        // 單例模式
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
        
        // 為每個音效創建音頻源
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.spatialBlend = sound.spatialBlend;
            
            // 根據類型分配到不同的混音器組
            if (sound.name.StartsWith("Music_"))
            {
                sound.source.outputAudioMixerGroup = musicGroup;
            }
            else
            {
                sound.source.outputAudioMixerGroup = sfxGroup;
            }
        }
    }
    
    private void Start()
    {
        // 自動播放背景音樂
        Play("Music_Background");
    }
    
    // 播放指定名稱的音效
    public void Play(string name)
    {
        Sound sound = FindSound(name);
        if (sound != null)
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound '" + name + "' not found!");
        }
    }
    
    // 在指定位置播放3D音效
    public void PlayAt(string name, Vector3 position)
    {
        Sound sound = FindSound(name);
        if (sound != null)
        {
            // 臨時創建一個音頻源播放3D音效
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound '" + name + "' not found!");
        }
    }
    
    // 停止播放指定音效
    public void Stop(string name)
    {
        Sound sound = FindSound(name);
        if (sound != null && sound.source.isPlaying)
        {
            sound.source.Stop();
        }
    }
    
    // 查找指定名稱的音效
    private Sound FindSound(string name)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == name)
            {
                return sound;
            }
        }
        return null;
    }
    
    // 設置音樂音量
    public void SetMusicVolume(float volume)
    {
        if (musicGroup != null)
        {
            musicGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
    }
    
    // 設置音效音量
    public void SetSFXVolume(float volume)
    {
        if (sfxGroup != null)
        {
            sfxGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }
    
    // 靜態方法便於直接調用
    public static void PlaySound(string name)
    {
        if (Instance != null)
        {
            Instance.Play(name);
        }
    }
    
    // 靜態方法便於在位置處播放
    public static void PlaySoundAt(string name, Vector3 position)
    {
        if (Instance != null)
        {
            Instance.PlayAt(name, position);
        }
    }
} 