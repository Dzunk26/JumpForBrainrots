using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSetting : UICanvas {
    [SerializeField] private Button closeButton;
    [SerializeField] private Button blockerButton;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;


    private void OnEnable() {
        musicSlider.SetValueWithoutNotify(MusicManager.Instance.MusicVolume);
        fxSlider.SetValueWithoutNotify(SoundManager.Instance.FxVolume);

        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
        musicSlider.onValueChanged.AddListener(MusicManager.Instance.SetMusicVolume);
        fxSlider.onValueChanged.AddListener(SoundManager.Instance.SetFxVolume);
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        blockerButton.onClick.RemoveListener(OnClickCloseButton);
        if (MusicManager.HasInstance) {
            musicSlider.onValueChanged.RemoveListener(MusicManager.Instance.SetMusicVolume);
        }
        if (SoundManager.HasInstance) {
            fxSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetFxVolume);
        }
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}