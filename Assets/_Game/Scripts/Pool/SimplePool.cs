using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimplePool {
    private static Dictionary<PoolType, Pool> poolInstances = new Dictionary<PoolType, Pool>();

    // khoi tao pool
    public static void PreLoad(GameUnit prefab, int amount, Transform parent) {
        if (prefab == null) {
            Debug.LogError("Prefab is null !!");
            return;
        }

        if (!poolInstances.ContainsKey(prefab.poolType) || poolInstances[prefab.poolType] == null) {
            Pool pool = new Pool();
            pool.PreLoad(prefab, amount, parent);
            poolInstances[prefab.poolType] = pool;
        }
    }
    
    // lay phan tu khoi pool
    public static T GetFromPool<T>(PoolType poolType, Vector3 position, Quaternion rotation) where T : GameUnit {
        if (!poolInstances.ContainsKey(poolType)) {
            Debug.LogError(poolType + " is not loaded");
            return null;
        }

        return poolInstances[poolType].GetFromPool(position, rotation) as T;
    }

    // tra phan tu ve pool
    public static void ReturnToPool<T>(T unit) where T : GameUnit {
        if (!poolInstances.ContainsKey(unit.poolType)) {
            Debug.LogError(unit.poolType + " is not loaded");
            return;
        }

        poolInstances[unit.poolType].ReturnToPool(unit);
    }

    // thu tat ca cac phan tu dang dung ve pool
    public static void CollectAllActiveUnitsInPool(PoolType poolType) {
        if (!poolInstances.ContainsKey(poolType)) {
            Debug.LogError(poolType + " is not loaded");
            return;
        }

        poolInstances[poolType].CollectAllActiveUnits();
    }
    
    // thu tat ca cac phan tu dang dung cua tat ca ca pool
    public static void CollectAllActiveUnitsInAllPools() {
        foreach (Pool pool in poolInstances.Values) {
            pool.CollectAllActiveUnits();
        }
    }

    // giai phong pool
    public static void ReleasePool(PoolType poolType) {
        if (!poolInstances.ContainsKey(poolType)) {
            Debug.LogError(poolType + " is not loaded");
            return;
        }

        poolInstances[poolType].ReleasePool();
    }

    // giai phong tat ca cac pool
    public static void ReleaseAllPools() {
        foreach (Pool pool in poolInstances.Values) {
            pool.ReleasePool();
        }
    }
}

public class Pool {
    Transform parent;
    GameUnit prefab;
    // danh sach cac unit dang o trong pool
    Queue<GameUnit> inactives = new Queue<GameUnit>();
    // danh sach cac unit dang duoc su dung
    List<GameUnit> actives = new List<GameUnit>();

    // khoi tao pool
    public void PreLoad(GameUnit prefab, int amount, Transform parent) {
        this.parent = parent;
        this.prefab = prefab;

        for (int i = 0; i < amount; i++) {
            ReturnToPool(GetFromPool(Vector3.zero, Quaternion.identity));
        }
    }
    
    // lay phan tu ra khoi pool
    public GameUnit GetFromPool(Vector3 position, Quaternion rotation) {
        GameUnit unit;
        if (inactives.Count <= 0) {
            unit = GameObject.Instantiate(prefab, position, rotation, parent);
        }
        else {
            unit = inactives.Dequeue();
            unit.TF.SetPositionAndRotation(position, rotation);
            unit.gameObject.SetActive(true);
        }

        actives.Add(unit);

        return unit;
    }

    // tra phan tu ve pool
    public void ReturnToPool(GameUnit gameUnit) {
        if (gameUnit != null && gameUnit.gameObject.activeSelf) {
            actives.Remove(gameUnit);
            inactives.Enqueue(gameUnit);
            gameUnit.gameObject.SetActive(false);
        }
    }

    // tra ta ca cac phan tu dang dung ve pool
    public void CollectAllActiveUnits() {
        while (actives.Count > 0) {
            ReturnToPool(actives[0]);
        }
    }

    // giai phong tat ca cac phan tu trong pool
    public void ReleasePool() {
        CollectAllActiveUnits();

        while (inactives.Count > 0) {
            GameObject.Destroy(inactives.Dequeue().gameObject);
        }

        inactives.Clear();
    }
}
