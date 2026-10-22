using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
public enum SoundID {
    BG_1 = 0,
}

public enum FxID {
    Landing = 0,
    MoneyChange = 1,
    StoreBrainrot = 2,
    Dead = 3,
    ObstacleStorm = 4,
    ObstacleRollingRock = 5,
    ObstacleRollingLog = 6, 
    ObstacleTsunami = 7,
    Button = 8,
    UpgradeBaseHouse = 9
}

public class SoundManager : Singleton<SoundManager> {
    [SerializeField] private AudioSource soundSource;
    private AudioSource[] fxSource = new AudioSource[Utils.GetEnumCount<FxID>()];

    [SerializeField] private AudioClip[] soundAus;
    [SerializeField] private AudioClip[] fxAus;

    private bool isLoaded = false;
    private int indexSound;

    public void Awake() {
        DontDestroyOnLoad(gameObject);

        soundSource.loop = true;
    }

    private void Start() {
        Invoke(nameof(OnLoad), 1);
    }

    private void OnLoad() {
        if (soundAus.Length > 0) {
            isLoaded = true;

            indexSound = Random.Range(0, soundAus.Length);
            PlaySound((SoundID)indexSound);
        }
    }

    public void PlaySound(SoundID ID) {
        soundSource.clip = soundAus[(int)ID];
        soundSource.Play();
    }

    public void PlayFx(FxID ID) {
        if (isLoaded) {
            if (fxSource[(int)ID] == null) {
                fxSource[(int)ID] = new GameObject().AddComponent<AudioSource>();
                fxSource[(int)ID].clip = fxAus[(int)ID];
                fxSource[(int)ID].loop = false;
                fxSource[(int)ID].transform.SetParent(transform);
            }
            fxSource[(int)ID].PlayOneShot(fxAus[(int)ID]);
        }
    }

    public void ChangeSound(SoundID ID, float time) {

    }
}