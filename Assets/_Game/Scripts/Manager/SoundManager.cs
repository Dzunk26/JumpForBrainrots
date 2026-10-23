using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.XR;

public enum FxID {
    Button = 0,
    MoneyChange = 1,
    Dead = 2,
    Landing = 3,
    ObstacleRollingLog = 4, 
    ObstacleRollingRock = 5,
    ObstacleTornado = 6,
    ObstacleTsunami = 7,
    StoreBrainrot = 8,
    UpgradeBaseHouse = 9
}

public class SoundManager : Singleton<SoundManager>, IDataPersistence {
    public float FxVolume => fxVolume;

    public bool IsDirty => isDirty;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup fxMixerGroup;
    [SerializeField] private AudioClip[] fxAus;

    private AudioSource[] fxSource = new AudioSource[Utils.GetEnumCount<FxID>()];
    private bool isDirty;
    private float fxVolume;

    public void LoadData(GameData gameData) {
        fxVolume = gameData.fxVolume;
        ApplyVolume(Constant.FX_VOLUME_PARAM, fxVolume);
    }

    public void SaveData(ref GameData gameData) {
        gameData.fxVolume = fxVolume;
        isDirty = false;
    }

    public void PlayFx(FxID ID) {
        int index = (int)ID;
        if (index >= fxAus.Length || fxAus[index] == null) return;

        if (fxSource[index] == null) {
            fxSource[index] = new GameObject(ID.ToString()).AddComponent<AudioSource>();
            fxSource[index].outputAudioMixerGroup = fxMixerGroup;
            fxSource[index].loop = false;
            fxSource[index].transform.SetParent(transform);
        }
        fxSource[index].PlayOneShot(fxAus[index]);
    }

    public void SetFxVolume(float volume) {
        fxVolume = Mathf.Clamp01(volume);
        ApplyVolume(Constant.FX_VOLUME_PARAM, fxVolume);
        isDirty = true;
    }

    private void ApplyVolume(string param, float volume) {
        // mixer dung dB, slider 0-1 phai doi sang log de nghe tuyen tinh
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        audioMixer.SetFloat(param, dB);
    }
}