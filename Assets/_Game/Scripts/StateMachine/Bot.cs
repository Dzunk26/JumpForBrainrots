using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class BotRarityByLevel {
    public BrainrotRarity minRarity;
    public BrainrotRarity maxRarity;
}

public class Bot : Character {
    public bool IsReadyToLeave => stayTimer <= 0f && !isTraversingLink && (currentState == BotStates.Idle || currentState == BotStates.Patrol);

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stateTimerMax = 5f;

    [Header("Catch State")]
    [SerializeField] private float catchRange = 3f;
    [SerializeField] private float catchTimerMax = 20f;
    [SerializeField] private float catchRate = 0.5f;

    [Header("Handle Jump Navmesh Link")]
    [SerializeField] private LayerMask jumpWallLayerMask;
    [SerializeField] private float jumpClearance = 2f;
    [SerializeField] private float jumpWallOffset = 0.25f;
    [SerializeField] private float jumpRiseGravity = 40f;
    [SerializeField] private float jumpFallGravity = 35f;
    [SerializeField] private float jumpAirSpeed = 15f;

    [Header("Return State")]
    [SerializeField] private float arriveDistance = 1.5f;

    [Header("Patrol State")]
    [SerializeField] private float patrolSampleDistance = 2f;
    [SerializeField] private int patrolSampleRetry = 10;

    [Header("Inventory")]
    [SerializeField] private int inventoryCapacity = 20;
    [SerializeField] private float holdRandomBrainrotRate = 0.3f;
    [SerializeField] private float dropRandomBrainrotRate = 0.5f;

    [Header("Dead State")]
    [SerializeField] private float deadTimerMax = 3f;

    [Header("Base Slot")]
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private List<BotRarityByLevel> listRarityByLevel;
    [SerializeField] private float highRarityRate = 0.35f;
    [SerializeField] private float emptySlotRate = 0.1f;

    [Header("Skin")]
    [SerializeField] private ListSkinSO listSkinSO;

    [SerializeField] private float rotateSpeed = 15f;

    private bool isTraversingLink;
    private bool isLinkFalling;
    private float linkTimer;
    private float linkRiseDuration;
    private float linkFallDuration;
    private Vector3 linkStartPoint;
    private Vector3 linkPeakPoint;
    private Vector3 linkEndPoint;
    private BotState currentState;
    private Vector3 currentTargetPoint;
    private float stateTimer;
    private int floorArea;
    private int safeAreaMask;
    private BrainrotSpawner targetBrainrotSpawner;
    private Brainrot targetBrainrot;
    private List<BrainrotSO> capturedBrainrotSOs = new List<BrainrotSO>();
    private BoxCollider patrolArea;
    private float stayTimer;

    private void Update() {
        if (currentState == BotStates.Dead) {
            currentState.OnExecute(this);
            return;
        }

        stayTimer -= Time.deltaTime;

        if (agent.isOnOffMeshLink) {
            HandleTraverseLink();
            return;
        }

        isTraversingLink = false;
        TF.position = agent.nextPosition;
        HandleRotation();

        if (currentState != null) {
            currentState.OnExecute(this);
        }

    }

    public override void OnInit() {
        agent.autoTraverseOffMeshLink = false;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = characterStat.GetMoveSpeed();
        InitNavMeshArea();
        base.OnInit();
        characterVisual.OnChangeSkin(listSkinSO.GetRandomSkinSO());
        ResetToSpawnPoint();
        ChangeState(BotStates.Idle);
    }

    public void OnInit(BaseHouse basehouse, BoxCollider patrolArea, float stayTimer, string characterName) {
        this.baseHouse = basehouse;
        this.patrolArea = patrolArea;
        this.stayTimer = stayTimer;
        this.characterName = characterName;
        capturedBrainrotSOs.Clear();
        OnInit();
    }

    public override void OnDespawn() {
        ChangeState(null);
        StopMoving();
        DropBrainrot();
        isTraversingLink = false;
        base.OnDespawn();
    }

    public override void StoreCapturedBrainrot() {
        if (!IsHoldingCapturedBrainrot()) return;

        if (capturedBrainrotSOs.Count >= inventoryCapacity) {
            capturedBrainrotSOs.RemoveAt(0);
        }
        capturedBrainrotSOs.Add(currentHoldBrainrotSO);
        DropBrainrot();
    }

    public override void InitBaseHouse() {
        baseHouse.OnInit(this, Random.Range(1, baseHouse.GetMaxLevel() + 1));
        HandleRandomBaseSlots();
        base.InitBaseHouse();
    }

    public override void Dead() {
        if (currentState == BotStates.Dead) return;

        ChangeState(BotStates.Dead);
    }

    public void OnRespawn() {
        ResetToSpawnPoint();
        ChangeState(BotStates.Idle);
    }

    public void OnEnterIdle() {
        stateTimer = Random.Range(stateTimerMax / 2, stateTimerMax);
        StopMoving();
        HandleRandomHoldBrainrot();
    }

    public void OnExecuteIdle() {
        stateTimer -= Time.deltaTime;
        characterVisual.OnIdle();

        if (stateTimer > 0) return;

        if (currentHoldBrainrotSO == null && Random.value < catchRate) {
            ChangeState(BotStates.Catch);
        }
        else {
            ChangeState(BotStates.Patrol);
        }
    }

    public void OnEnterPatrol() {
        agent.areaMask = safeAreaMask;
        SetDestination(GetRandomTargetPoint());
        ContinueMoving();
    }

    public void OnExecutePatrol() {
        characterVisual.OnRun();

        if (!IsArrived()) return;

        ChangeState(BotStates.Idle);
    }

    public void OnEnterCatch() {
        agent.areaMask = NavMesh.AllAreas;
        stateTimer = catchTimerMax;
        targetBrainrotSpawner = FloorManager.Instance.GetRandomBrainrotSpawner();
        targetBrainrot = null;
        ContinueMoving();
    }

    public void OnExecuteCatch() {
        characterVisual.OnRun();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0) {
            ChangeState(BotStates.Return);
            return;
        }

        if (!IsValidTargetBrainrot()) {
            HandleFindTargetBrainrot();
            return;
        }

        if (IsInCatchRange()) {
            CaptureTargetBrainrot();
            ChangeState(BotStates.Return);
        }
    }

    public void OnEnterReturn() {
        agent.areaMask = NavMesh.AllAreas;
        SetDestination(GetRandomTargetPoint());
        ContinueMoving();
    }

    public void OnExecuteReturn() {
        characterVisual.OnRun();

        if (!IsArrived()) return;

        ChangeState(BotStates.Idle);
    }

    public void OnEnterDead() {
        stateTimer = deadTimerMax;
        isTraversingLink = false;
        StopMoving();
        DropBrainrot();
        characterVisual.OnDead();
    }

    public void OnExecuteDead() {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0) return;

        OnRespawn();
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

    private void HandleRandomBaseSlots() {
        BrainrotRarity lowRarity = GetRandomLowRarity();
        BrainrotRarity highRarity = lowRarity < GetRarityByLevel().maxRarity ? lowRarity + 1 : lowRarity;
        List<BrainrotSO> lowBrainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(lowRarity);
        List<BrainrotSO> highBrainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(highRarity);
        List<BaseSlot> unlockedSlots = baseHouse.GetUnlockedSlots();

        for (int i = 0; i < unlockedSlots.Count; i++) {
            if (Random.value < emptySlotRate) continue;

            BrainrotSO brainrotSO = GetRandomBrainrotSO(lowBrainrotSOs, highBrainrotSOs);
            if (brainrotSO == null) continue;

            unlockedSlots[i].LoadBrainrot(brainrotSO, 0);
        }
    }

    private BrainrotRarity GetRandomLowRarity() {
        BotRarityByLevel rarityByLevel = GetRarityByLevel();

        return (BrainrotRarity)Random.Range((int)rarityByLevel.minRarity, (int)rarityByLevel.maxRarity);
    }

    private BotRarityByLevel GetRarityByLevel() {
        int levelIndex = Mathf.Clamp(baseHouse.GetHouseLevel() - 1, 0, listRarityByLevel.Count - 1);
        return listRarityByLevel[levelIndex];
    }

    private BrainrotSO GetRandomBrainrotSO(List<BrainrotSO> lowBrainrotSOs, List<BrainrotSO> highBrainrotSOs) {
        List<BrainrotSO> brainrotSOs = Random.value < highRarityRate ? highBrainrotSOs : lowBrainrotSOs;
        if (brainrotSOs.Count == 0) brainrotSOs = lowBrainrotSOs.Count > 0 ? lowBrainrotSOs : highBrainrotSOs;
        if (brainrotSOs.Count == 0) return null;

        return brainrotSOs[Random.Range(0, brainrotSOs.Count)];
    }

    private void ChangeState(BotState newState) {
        currentState?.OnExit(this);

        currentState = newState;

        currentState?.OnEnter(this);
    }

    private void InitNavMeshArea() {
        floorArea = NavMesh.GetAreaFromName(Constant.FLOOR_AREA);
        safeAreaMask = NavMesh.AllAreas & ~(1 << floorArea); // tat ca area tru floor
    }

    private Vector3 GetRandomTargetPoint() {
        Bounds bounds = patrolArea.bounds;

        for (int i = 0; i < patrolSampleRetry; i++) {
            Vector3 point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y, Random.Range(bounds.min.z, bounds.max.z));

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, patrolSampleDistance, safeAreaMask)) {
                return hit.position;
            }
        }

        return TF.position;
    }

    private void HandleRotation() {
        Vector3 moveDirection = agent.velocity;
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        TF.rotation = Quaternion.Slerp(TF.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void HandleFindTargetBrainrot() {
        targetBrainrot = targetBrainrotSpawner.GetRandomBrainrot();
        if (targetBrainrot == null) {
            ChangeState(BotStates.Return);
            return;
        }

        SetDestination(targetBrainrot.TF.position);
    }

    private bool IsValidTargetBrainrot() {
        if (targetBrainrot == null || !targetBrainrot.IsAlive) return false;

        return (targetBrainrot.TF.position - currentTargetPoint).sqrMagnitude < 0.001f;
    }

    private bool IsInCatchRange() {
        return (currentTargetPoint - TF.position).sqrMagnitude <= catchRange * catchRange;
    }

    private void CaptureTargetBrainrot() {
        HoldBrainrot(targetBrainrot.GetBrainrotSO(), targetBrainrot.TF.position, HeldBrainrotSource.Captured);
        targetBrainrot.OnCaptured();
        targetBrainrot = null;
    }

    private void StartTraverseLink() {
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        linkStartPoint = TF.position;
        linkEndPoint = linkData.endPos + Vector3.up * agent.baseOffset;
        linkPeakPoint = GetJumpPeakPoint();
        linkRiseDuration = GetJumpDuration(linkStartPoint, linkPeakPoint, jumpRiseGravity);
        linkFallDuration = GetJumpDuration(linkPeakPoint, linkEndPoint, jumpFallGravity);
        linkTimer = 0f;
        isLinkFalling = false;
        isTraversingLink = true;

        Vector3 lookDirection = linkEndPoint - linkStartPoint;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f) {
            TF.rotation = Quaternion.LookRotation(lookDirection);
        }

        characterVisual.OnStartJump();
    }

    private void HandleTraverseLink() {
        if (!isTraversingLink) StartTraverseLink();

        linkTimer += Time.deltaTime;

        if (linkTimer < linkRiseDuration) {
            float riseT = linkTimer / linkRiseDuration;
            TF.position = GetJumpArcPoint(linkStartPoint, linkPeakPoint, 1f - (1f - riseT) * (1f - riseT), riseT);
            return;
        }

        if (!isLinkFalling) {
            isLinkFalling = true;
            characterVisual.OnFalling();
        }

        float fallT = Mathf.Clamp01((linkTimer - linkRiseDuration) / linkFallDuration);
        TF.position = GetJumpArcPoint(linkPeakPoint, linkEndPoint, fallT * fallT, fallT);

        if (fallT < 1f) return;

        isTraversingLink = false;
        agent.CompleteOffMeshLink();
        characterVisual.OnRun();
    }

    // dinh cung dat sat mep tang cao hon de bot luon o tren mat tang khi di qua mep
    private Vector3 GetJumpPeakPoint() {
        bool isJumpUp = linkEndPoint.y >= linkStartPoint.y;
        Vector3 lowPoint = isJumpUp ? linkStartPoint : linkEndPoint;
        Vector3 highPoint = isJumpUp ? linkEndPoint : linkStartPoint;
        Vector3 peakPoint = Vector3.Lerp(lowPoint, highPoint, 0.5f);

        Vector3 direction = highPoint - lowPoint;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > 0.001f) {
            direction /= distance;
            Vector3 origin = lowPoint + Vector3.up * agent.height * 0.5f;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, jumpWallLayerMask, QueryTriggerInteraction.Ignore)) {
                peakPoint = lowPoint + direction * Mathf.Max(0f, hit.distance - agent.radius - jumpWallOffset);
            }
        }

        peakPoint.y = highPoint.y + jumpClearance;
        return peakPoint;
    }

    private float GetJumpDuration(Vector3 fromPoint, Vector3 toPoint, float gravity) {
        Vector3 horizontal = toPoint - fromPoint;
        horizontal.y = 0f;
        float gravityDuration = Mathf.Sqrt(2f * Mathf.Abs(toPoint.y - fromPoint.y) / gravity);

        return Mathf.Max(gravityDuration, horizontal.magnitude / jumpAirSpeed, 0.01f);
    }

    private Vector3 GetJumpArcPoint(Vector3 fromPoint, Vector3 toPoint, float heightT, float horizontalT) {
        Vector3 point = Vector3.Lerp(fromPoint, toPoint, horizontalT);
        point.y = Mathf.Lerp(fromPoint.y, toPoint.y, heightT);
        return point;
    }

    private bool IsArrived() {
        if (agent.pathPending) return false;

        return agent.remainingDistance <= arriveDistance;
    }

    private void HandleRandomHoldBrainrot() {
        if (currentHoldBrainrotSO == null) {
            if (capturedBrainrotSOs.Count == 0 || Random.value >= holdRandomBrainrotRate) return;

            BrainrotSO brainrotSO = capturedBrainrotSOs[Random.Range(0, capturedBrainrotSOs.Count)];
            HoldBrainrot(brainrotSO, characterVisual.GetBrainrotHoldPosition(), HeldBrainrotSource.Inventory);
            return;
        }

        if (heldBrainrotSource != HeldBrainrotSource.Inventory || Random.value >= dropRandomBrainrotRate) return;

        DropBrainrot();
    }

    private bool IsHoldingCapturedBrainrot() {
        return heldBrainrotSource == HeldBrainrotSource.Captured;
    }

    private void FaceToBaseHouse() {
        Vector3 direction = baseHouse.GetSpawnForward();
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        TF.rotation = Quaternion.LookRotation(direction);
    }

    private void ResetToSpawnPoint() {
        isTraversingLink = false;
        DropBrainrot();
        agent.Warp(spawnPoint);
        TF.position = spawnPoint;
        FaceToBaseHouse();
        characterVisual.OnInit();
    }
}