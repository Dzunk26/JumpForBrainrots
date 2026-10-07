using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Bot : Character {
    public bool IsDestination => (currentTargetPoint - TF.position).sqrMagnitude < 0.001f;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stateTimerMax = 5f;

    private BotState currentState;
    private Vector3 currentTargetPoint;
    private float stateTimer;
    private int floorArea;
    private int safeAreaMask;
    private NavMeshTriangulation navMeshTriangulation;
    private List<int> safeTriangleIndexes = new List<int>();

    private void OnEnable() {
        OnInit();
    }

    private void Update() {
        if (currentState != null) {
            currentState.OnExecute(this);
        }
    }

    public override void OnInit() {
        InitNavMeshArea();
        ChangeState(BotStates.Idle);
        base.OnInit();
    }

    public override void OnDespawn() {
        base.OnDespawn();
    }

    public override void InitBaseHouse() {
        baseHouse.OnInit(this, Random.Range(0, baseHouse.GetMaxLevel() + 1));
        base.InitBaseHouse();
    }

    public void OnEnterIdle() {
        stateTimer = Random.Range(stateTimerMax / 2, stateTimerMax);
        StopMoving();
    }

    public void OnExecuteIdle() {
        stateTimer -= Time.deltaTime;
        characterVisual.OnIdle();

        if (stateTimer <= 0) {
            ChangeState(BotStates.Patrol);
        }
    }

    public void OnEnterPatrol() {
        agent.areaMask = safeAreaMask;
        Vector3 target = GetRandomTargetPoint();
        SetDestination(target);
        ContinueMoving();
    }

    public void OnExecutePatrol() {
        characterVisual.OnRun();
    }

    public void OnEnterCatch() {
        agent.areaMask = NavMesh.AllAreas;
    }

    public void OnExecuteCatch() {

    }

    public void OnEnterDead() {
        stateTimer = Random.Range(stateTimerMax / 2, stateTimerMax);
        StopMoving();
    }

    public void OnExecuteDead() {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0) {
            OnDespawn();
        }
    }

    public Vector3 GetRandomTargetPoint() {
        if (safeTriangleIndexes.Count == 0) return TF.position;

        int triangleIndex = safeTriangleIndexes[Random.Range(0, safeTriangleIndexes.Count)];
        int vertexIndex = navMeshTriangulation.indices[triangleIndex * 3 + Random.Range(0, 3)];

        return navMeshTriangulation.vertices[vertexIndex];
    }

    public void StopMoving() {
        agent.isStopped = true;
    }

    public void ContinueMoving() {
        agent.isStopped = false;
    }

    public void SetDestination(Vector3 target) {
        this.currentTargetPoint = target;
        agent.SetDestination(target);
    }

    private void ChangeState(BotState newState) {
        currentState?.OnExit(this);

        currentState = newState;

        currentState?.OnEnter(this);
    }

    private void InitNavMeshArea() {
        floorArea = NavMesh.GetAreaFromName(Constant.FLOOR_AREA);
        safeAreaMask = NavMesh.AllAreas & ~(1 << floorArea); // tat ca area tru floor

        navMeshTriangulation = NavMesh.CalculateTriangulation();
        safeTriangleIndexes.Clear();

        for (int i = 0; i < navMeshTriangulation.areas.Length; i++) {
            if (navMeshTriangulation.areas[i] == floorArea) continue;
            safeTriangleIndexes.Add(i);
        }
    }
}