using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum MusicID {
    BG_1 = 0,
}

public class MusicManager : Singleton<MusicManager>, IDataPersistence {
    public bool IsDirty => isDirty;
    public float MusicVolume => musicVolume;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip[] musicClips;

    private bool isDirty;
    private float musicVolume = 1f;

    private void Start() {
        if (musicClips.Length == 0) return;

        PlayMusic((MusicID)Random.Range(0, musicClips.Length));
    }

    public void LoadData(GameData gameData) {
        musicVolume = gameData.musicVolume;
        ApplyVolume(Constant.MUSIC_VOLUME_PARAM, musicVolume);
    }

    public void SaveData(ref GameData gameData) {
        gameData.musicVolume = musicVolume;
        isDirty = false;
    }

    public void PlayMusic(MusicID id) {
        int index = (int)id;
        if (index >= musicClips.Length || musicClips[index] == null) return;

        musicSource.clip = musicClips[index];
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume) {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolume(Constant.MUSIC_VOLUME_PARAM, musicVolume);
        isDirty = true;
    }

    public void StopMusic() {
        musicSource.Stop();
    }

    private void ApplyVolume(string param, float volume) {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        audioMixer.SetFloat(param, dB);
    }
}