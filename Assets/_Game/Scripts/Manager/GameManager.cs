using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State {
    Loading,
    MainMenu,
    GamePause,
    GamePlaying,
    Lose,
    Win
}

public class GameManager : Singleton<GameManager> {
    public event EventHandler OnStateChanged;

    private State currentState;

    private void Start() {
        LoadingGame();
    }

    private void OnDestroy() {
        Cache.Clear();
    }

    private void ChangeState(State state) {
        currentState = state;
        OnStateChanged?.Invoke(this, new EventArgs());
    }

    public void LoadingGame() {
        ChangeState(State.Loading);
        DataManager.Instance.OnInit();
        DataManager.Instance.LoadGame();
    }
}