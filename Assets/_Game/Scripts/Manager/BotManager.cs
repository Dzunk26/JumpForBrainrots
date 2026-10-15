using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : Singleton<BotManager> {
    [SerializeField] private List<BaseHouse> listBaseHouse;
    [SerializeField] private BoxCollider patrolArea;
    [SerializeField] private ListBotNameSO listBotNameSO;
    [Header("Room Config")]
    [SerializeField] private int initialBotCount = 3;
    [SerializeField] private int minBotCount = 2;
    [SerializeField] private float joinTimerMin = 20f;
    [SerializeField] private float joinTimerMax = 60f;
    [SerializeField] private float refillTimerMax = 5f;
    [SerializeField] private float stayTimerMin = 120f;
    [SerializeField] private float stayTimerMax = 480f;

    private List<Bot> listBot = new List<Bot>();
    private List<BaseHouse> listEmptyBaseHouse = new List<BaseHouse>();
    private List<string> listAvailableName = new List<string>();
    private float joinTimer;
    private bool isInitialized;

    private void Update() {
        if (!isInitialized) return;

        HandleLeave();
        HandleJoin();
    }

    public void OnInit() {
        listAvailableName.Clear();
        listAvailableName.AddRange(listBotNameSO.GetListBotName());

        for (int i = 0; i < listBaseHouse.Count; i++) {
            if (listBaseHouse[i].IsEmpty) {
                listBaseHouse[i].OnDespawn();
            }
        }

        for (int i = 0; i < initialBotCount; i++) {
            SpawnBot();
        }

        joinTimer = Random.Range(joinTimerMin, joinTimerMax);
        isInitialized = true;
    }

    private void HandleJoin() {
        joinTimer -= Time.deltaTime;
        if (joinTimer > 0f) return;

        SpawnBot();
        joinTimer = Random.Range(joinTimerMin, joinTimerMax);
    }

    private void HandleLeave() {
        for (int i = listBot.Count - 1; i >= 0; i--) {
            if (!listBot[i].IsReadyToLeave) continue;

            DespawnBot(listBot[i]);
        }
    }

    private void SpawnBot() {
        BaseHouse baseHouse = GetRandomEmptyBaseHouse();
        if (baseHouse == null) return;

        Quaternion rotation = Quaternion.LookRotation(baseHouse.GetSpawnForward());
        Bot bot = SimplePool.GetFromPool<Bot>(PoolType.Bot, baseHouse.GetSpawnPosition(), rotation);
        bot.OnInit(baseHouse, patrolArea, Random.Range(stayTimerMin, stayTimerMax), GetRandomName());
        listBot.Add(bot);
    }

    private void DespawnBot(Bot bot) {
        BaseHouse baseHouse = bot.GetBaseHouse();
        listAvailableName.Add(bot.GetCharacterName());
        bot.OnDespawn();
        baseHouse.OnDespawn();
        listBot.Remove(bot);
        SimplePool.ReturnToPool(bot);

        if (listBot.Count < minBotCount) {
            joinTimer = Mathf.Min(joinTimer, refillTimerMax);
        }
    }

    private BaseHouse GetRandomEmptyBaseHouse() {
        listEmptyBaseHouse.Clear();

        for (int i = 0; i < listBaseHouse.Count; i++) {
            if (listBaseHouse[i].IsEmpty) {
                listEmptyBaseHouse.Add(listBaseHouse[i]);
            }
        }

        if (listEmptyBaseHouse.Count == 0) return null;

        return listEmptyBaseHouse[Random.Range(0, listEmptyBaseHouse.Count)];
    }

    private string GetRandomName() {
        if (listAvailableName.Count == 0) {
            Debug.LogWarning("Bot name list is empty !!");
            return "Player" + Random.Range(1000, 10000);
        }

        int randomIndex = Random.Range(0, listAvailableName.Count);
        string botName = listAvailableName[randomIndex];
        listAvailableName.RemoveAt(randomIndex);

        return botName;
    }
}