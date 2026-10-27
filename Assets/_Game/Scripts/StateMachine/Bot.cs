using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class BotRarityByLevel {
    public BrainrotRarity minRarity;
    public BrainrotRarity maxRarity;
}

public enum KnockbackPhase {
    Flying,
    Lying,
    GettingUp,
}

public enum BotAttackIntent {
    Rob, // danh nguoi dang cam brainrot bat duoc roi nhat brainrot roi ra
    Troll, // danh choi 1-2 phat roi bo di
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

    [Header("Knockback State")]
    [SerializeField] private LayerMask groundLayerMask = 1608;
    [SerializeField] private float knockbackRiseGravity = -150f;
    [SerializeField] private float knockbackFallGravity = -125f;
    [SerializeField] private float knockbackDrag = 60f;
    [SerializeField] private float knockbackTimerMax = 0.3f;
    [SerializeField] private float knockbackFallTimerMax = 5f;
    [SerializeField] private float knockbackSampleDistance = 5f;

    [Header("Base Slot")]
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private List<BotRarityByLevel> listRarityByLevel;
    [SerializeField] private float highRarityRate = 0.35f;
    [SerializeField] private float emptySlotRate = 0.1f;

    [Header("Skin")]
    [SerializeField] private ListSkinSO listSkinSO;

    [Header("Attack State")]
    [Tooltip("Ti le bot HIEN (khong bao gio chu dong danh). 0 = moi bot deu co the danh, 1 = khong bot nao danh")]
    [Range(0f, 1f)]
    [SerializeField] private float peacefulBotRate = 0.3f;
    [Tooltip("Do hung hang cua bot khong hien, nhan vao robChancePerScan / trollChancePerScan")]
    [SerializeField] private Vector2 aggressionRange = new Vector2(0.3f, 1f);
    [SerializeField] private float huntScanInterval = 0.5f; // chu ky quet tim nguoi de cuop / troll
    [SerializeField] private float robDetectRange = 25f; // thay ai cam brainrot trong pham vi nay thi co the di cuop
    [SerializeField] private float robChancePerScan = 0.15f;
    [SerializeField] private float trollScanRange = 10f; // dang di catch gap ai trong pham vi nay thi co the danh troll
    [SerializeField] private float trollChancePerScan = 0.1f;
    [SerializeField] private float trollCooldown = 8f; // troll xong 1 luc moi troll tiep
    [SerializeField] private Vector2Int trollHitCountRange = new Vector2Int(1, 2);
    [SerializeField] private float revengeChance = 0.6f; // bi danh xong dung day thi danh lai nguoi do
    [SerializeField] private float revengeDuration = 15f;
    [SerializeField] private float chaseTimerMax = 7f; // duoi qua lau thi bo
    [SerializeField] private float chaseRepathInterval = 0.25f;
    [Tooltip("Muc tieu chay xa hon tam quet luc chon x he so nay thi bo (rob dung robDetectRange, troll dung trollScanRange)")]
    [SerializeField] private float chaseGiveUpRangeMultiplier = 1.5f;
    [SerializeField] private float swingRange = 4f; // vao tam nay thi dung lai vung gay (nho hon hitRange de muc tieu chay van trung)
    [SerializeField] private LayerMask floorLayerMask = 1024; // layer Floor, giong Player.floorLayerMask: dung tren floor moi bi san (base / vung an toan thi khong)
    [SerializeField] private float floorCheckDistance = 1f;
    [SerializeField] private int maxHuntersPerTarget = 2;
    [SerializeField] private float sameTargetCooldown = 10f; // vua san ai thi 1 luc sau moi san lai nguoi do
    [Tooltip("In log khi bot bat dau / ket thuc san de test")]
    [SerializeField] private bool isLogHunt;

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
    private Vector3 knockbackVelocity;
    private KnockbackPhase knockbackPhase;
    private bool isHoldingBat;

    private float aggression;
    private BotAttackIntent attackIntent;
    private Character huntTarget;
    private int trollHitsLeft;
    private Character lastHuntTarget;
    private float lastHuntTime = -999f;
    private Character revengeTarget;
    private float revengeTime = -999f;
    private float huntScanTimer;
    private float lastTrollTime = -999f;
    private bool isResumeCatchAfterAttack; // dang catch thi bi cuon vao danh, danh xong quay lai catch tiep
    private bool isResumingCatch;
    private float resumeCatchTimer;
    private BrainrotSpawner resumeBrainrotSpawner;
    private float chaseRepathTimer;
    private float chaseGiveUpRange;
    private Brainrot robbedBrainrot; // brainrot vua danh roi, Catch chay toi nhat dung con nay
    private bool isCatchingRobbedBrainrot; // dang chay toi nhat do vua cuop thi khong di cuop nguoi khac
    private List<Character> trollCandidates = new List<Character>();
    private string lastScanLog;

    private void Update() {
        HandlePutAwayBat();

        if (currentState == BotStates.Dead || currentState == BotStates.Knockback) {
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
        characterVisual.OnInit();
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
        aggression = Random.value < peacefulBotRate ? 0f : Random.Range(aggressionRange.x, aggressionRange.y);
        revengeTarget = null;
        lastHuntTarget = null;
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

    public override bool IsHittable() {
        return currentState != BotStates.Dead;
    }

    public override bool IsKnocked() {
        return currentState == BotStates.Knockback;
    }

    public override void OnHit(Vector3 knockbackVelocity, Character attacker) {
        if (currentState == BotStates.Dead) return;

        if (attacker != null) {
            revengeTarget = attacker;
            revengeTime = Time.time;
        }

        this.knockbackVelocity = knockbackVelocity;
        if (currentState == BotStates.Knockback) {
            // dang bay / nam / dung day ma bi danh tiep thi bay lai tu dau
            StartKnockbackFlying();
            return;
        }

        ChangeState(BotStates.Knockback);
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

        if (TryStartRob()) return;
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

        if (TryStartRob()) return;

        if (!IsArrived()) return;

        ChangeState(BotStates.Idle);
    }

    public void OnEnterCatch() {
        agent.areaMask = NavMesh.AllAreas;
        stateTimer = catchTimerMax;
        targetBrainrotSpawner = FloorManager.Instance.GetRandomBrainrotSpawner();
        targetBrainrot = null;
        ContinueMoving();

        // danh troll xong: catch tiep o khu cu voi thoi gian con lai
        if (isResumingCatch) {
            isResumingCatch = false;
            stateTimer = resumeCatchTimer;
            targetBrainrotSpawner = resumeBrainrotSpawner;
        }

        // vua cuop: chay thang toi brainrot vua danh roi
        isCatchingRobbedBrainrot = robbedBrainrot != null;
        if (robbedBrainrot != null) {
            targetBrainrot = robbedBrainrot;
            targetBrainrotSpawner = FloorManager.Instance.GetClosestBrainrotSpawner(robbedBrainrot.GetRestPosition());
            robbedBrainrot = null;
            SetDestination(targetBrainrot.GetRestPosition());
        }
    }

    public void OnExecuteCatch() {
        characterVisual.OnRun();

        if (!isCatchingRobbedBrainrot && TryStartHuntWhileCatching()) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0) {
            ChangeState(BotStates.Return);
            return;
        }

        if (!IsValidTargetBrainrot()) {
            HandleFindTargetBrainrot();
            return;
        }

        if (IsInCatchRange() && targetBrainrot.IsAlive) {
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

    public void OnEnterAttack() {
        agent.areaMask = NavMesh.AllAreas;
        stateTimer = chaseTimerMax;
        chaseRepathTimer = 0f;
        trollHitsLeft = Random.Range(trollHitCountRange.x, trollHitCountRange.y + 1);
        huntTarget.AddHunter(1);

        // brainrot lay tu kho ra cam choi thi cat di de cam gay
        if (heldBrainrotSource == HeldBrainrotSource.Inventory) {
            DropBrainrot();
        }
        isHoldingBat = true;
        characterVisual.SetHoldingBat(true);
        ContinueMoving();
    }

    public void OnExecuteAttack() {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f || !IsValidHuntTarget(huntTarget) || (huntTarget.TF.position - TF.position).sqrMagnitude > chaseGiveUpRange * chaseGiveUpRange) {
            EndAttack();
            return;
        }

        // muc tieu da cat brainrot vao kho / lam roi truoc khi bi minh danh
        if (attackIntent == BotAttackIntent.Rob && !huntTarget.IsHoldingCapturedBrainrot()) {
            EndAttack();
            return;
        }

        Vector3 offset = huntTarget.TF.position - TF.position;
        float heightOffset = Mathf.Abs(offset.y);
        offset.y = 0f;

        // khac tang (muc tieu tren cao / duoi thap) thi van duoi tiep qua link, chua vung
        if (offset.sqrMagnitude > swingRange * swingRange || heightOffset > hitHeightTolerance) {
            ChaseHuntTarget();
            return;
        }

        StopMoving();
        characterVisual.OnIdle();
        if (offset.sqrMagnitude > 0.001f) {
            TF.rotation = Quaternion.Slerp(TF.rotation, Quaternion.LookRotation(offset), rotateSpeed * Time.deltaTime);
        }

        if (CanAttack()) {
            attackTarget = huntTarget;
            Attack();
        }
    }

    // gay van cam tiep toi khi vung xong (HandlePutAwayBat), khong cat ngang luc dang vung
    public void OnExitAttack() {
        if (huntTarget != null) {
            huntTarget.AddHunter(-1);
        }

        lastHuntTarget = huntTarget;
        lastHuntTime = Time.time;
        huntTarget = null;
        attackTarget = null;
    }

    public void OnEnterDead() {
        stateTimer = deadTimerMax;
        isTraversingLink = false;
        StopMoving();
        ScatterCapturedBrainrot(groundLayerMask);
        DropBrainrot();
        characterVisual.OnDead();
    }

    public void OnExecuteDead() {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0) return;

        OnRespawn();
    }

    public void OnEnterKnockback() {
        isAttacking = false;
        isTraversingLink = false;
        StopMoving();
        ScatterCapturedBrainrot(groundLayerMask);
        StartKnockbackFlying();
    }

    public void OnExecuteKnockback() {
        if (knockbackPhase == KnockbackPhase.Lying) {
            HandleKnockbackLying();
            return;
        }

        if (knockbackPhase == KnockbackPhase.GettingUp) {
            HandleKnockbackGettingUp();
            return;
        }

        float deltaTime = Time.deltaTime;
        stateTimer += deltaTime;

        // roi qua lau thi coi nhu rot khoi map
        if (stateTimer >= knockbackFallTimerMax) {
            ChangeState(BotStates.Dead);
            return;
        }

        float gravity = knockbackVelocity.y < 0f ? knockbackFallGravity : knockbackRiseGravity;
        Vector3 horizontal = new Vector3(knockbackVelocity.x, 0f, knockbackVelocity.z);
        horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, knockbackDrag * deltaTime);
        knockbackVelocity = new Vector3(horizontal.x, knockbackVelocity.y + gravity * deltaTime, horizontal.z);

        // dam vao tuong thi mat luc ngang
        Vector3 origin = TF.position + Vector3.up * agent.height * 0.5f;
        float wallDistance = horizontal.magnitude * deltaTime + agent.radius;
        if (horizontal.sqrMagnitude > 0.001f && Physics.Raycast(origin, horizontal.normalized, wallDistance, jumpWallLayerMask, QueryTriggerInteraction.Ignore)) {
            knockbackVelocity.x = 0f;
            knockbackVelocity.z = 0f;
        }

        Vector3 deltaPosition = knockbackVelocity * deltaTime;
        if (stateTimer >= knockbackTimerMax && knockbackVelocity.y <= 0f && IsKnockbackLanded(deltaPosition, out Vector3 landPoint)) {
            LandFromKnockback(landPoint);
            return;
        }

        TF.position += deltaPosition;
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

    // bot khong co inventory: chi lay gay ra luc danh, vung xong thi cat
    protected override void OnStartAttack() {
        isHoldingBat = true;
        characterVisual.SetHoldingBat(true);
    }

    protected override void OnHitCharacter(Character target, Brainrot scatteredBrainrot) {
        if (currentState != BotStates.Attack || target != huntTarget) return;

        if (attackIntent == BotAttackIntent.Rob) {
            if (scatteredBrainrot == null) {
                EndAttack();
                return;
            }

            robbedBrainrot = scatteredBrainrot;
            LogHunt("cuop duoc " + scatteredBrainrot.GetBrainrotSO().GetName() + ", chay di nhat");
            ChangeState(BotStates.Catch);
            return;
        }

        trollHitsLeft--;
        if (trollHitsLeft <= 0) {
            EndAttack();
        }
    }

    // dang catch thi quay lai catch tiep, con lai ve Idle
    private void EndAttack() {
        LogHunt("ket thuc " + attackIntent + (isResumeCatchAfterAttack ? ", catch tiep" : ", ve idle"));
        if (isResumeCatchAfterAttack) {
            isResumingCatch = true;
            ChangeState(BotStates.Catch);
            return;
        }

        ChangeState(BotStates.Idle);
    }

    // dang san thi giu gay, khong thi cat sau khi vung xong
    private void HandlePutAwayBat() {
        if (!isHoldingBat || currentState == BotStates.Attack || Time.time - lastAttackTime < attackTimerMax) return;

        isHoldingBat = false;
        characterVisual.SetHoldingBat(false);
    }

    // Idle / Patrol: dinh ky quet xem co ai dang cam brainrot bat duoc khong, co thi tung xac suat di cuop
    private bool TryStartRob() {
        if (!IsHuntScanReady()) return false;

        return TryRob();
    }

    // dang catch tren floor: uu tien cuop, khong thi gap ai gan thi co the danh troll roi catch tiep
    private bool TryStartHuntWhileCatching() {
        if (!IsHuntScanReady()) return false;
        if (TryRob()) return true;

        if (Time.time - lastTrollTime < trollCooldown || Random.value >= trollChancePerScan * aggression) return false;

        return TryStartAttack(BotAttackIntent.Troll, FindTrollTarget());
    }

    private bool IsHuntScanReady() {
        if (aggression <= 0f) {
            LogScan("bot hien (hung hang 0), khong san");
            return false;
        }
        if (IsHoldingCapturedBrainrot()) {
            LogScan("dang cam brainrot vua bat, khong san");
            return false;
        }

        huntScanTimer -= Time.deltaTime;
        if (huntScanTimer > 0f) return false;

        huntScanTimer = huntScanInterval;
        return true;
    }

    private bool TryRob() {
        if (Random.value >= robChancePerScan * aggression) return false;

        Character target = FindRobTarget();
        if (target == null) {
            LogScan(GetRobRejectLog());
        }

        return TryStartAttack(BotAttackIntent.Rob, target);
    }

    // log ly do bo qua nguoi gan nhat dang cam brainrot bat duoc (chi log khi ly do thay doi de khong spam)
    private string GetRobRejectLog() {
        List<Character> characters = GetActiveCharacters();
        Character nearestCarrier = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (character == this || !character.IsHoldingCapturedBrainrot()) continue;

            float sqrDistance = (character.TF.position - TF.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance) {
                nearestSqrDistance = sqrDistance;
                nearestCarrier = character;
            }
        }

        if (nearestCarrier == null) return "quet rob: khong ai dang cam brainrot bat duoc";

        return "quet rob: bo qua " + nearestCarrier.GetCharacterName() + " vi " + GetHuntRejectReason(nearestCarrier, robDetectRange);
    }

    private string GetHuntRejectReason(Character character, float range) {
        float distance = Vector3.Distance(character.TF.position, TF.position);
        if (distance > range) return "xa " + distance.ToString("0") + " > " + range.ToString("0");
        if (!character.gameObject.activeInHierarchy) return "khong hoat dong";
        if (!character.IsHittable()) return "da chet";
        if (character.IsKnocked()) return "dang bi danh van";
        if (!IsOnFloorArea(character.TF.position)) return "khong dung tren layer Floor";
        if (character.GetHunterCount() >= maxHuntersPerTarget) return "da co " + character.GetHunterCount() + " bot dang san";
        if (character == lastHuntTarget && Time.time - lastHuntTime < sameTargetCooldown) return "vua san xong, cho cooldown";

        return "khong ro";
    }

    private void LogScan(string message) {
        if (!isLogHunt || message == lastScanLog) return;

        lastScanLog = message;
        LogHunt(message);
    }

    private string GetStateName(BotState state) {
        if (state == BotStates.Idle) return "Idle";
        if (state == BotStates.Patrol) return "Patrol";
        if (state == BotStates.Catch) return "Catch";
        if (state == BotStates.Return) return "Return";
        if (state == BotStates.Dead) return "Dead";
        if (state == BotStates.Knockback) return "Knockback";
        if (state == BotStates.Attack) return "Attack";

        return "None";
    }

    // dung day sau khi bi danh: co the quay lai danh nguoi vua danh minh
    private bool TryStartRevenge() {
        if (revengeTarget == null || Time.time - revengeTime > revengeDuration) return false;

        Character target = revengeTarget;
        revengeTarget = null;
        float chance = aggression > 0f ? revengeChance : revengeChance * 0.5f;
        if (Random.value >= chance) return false;
        if (!IsValidHuntTarget(target) || GetHorizontalSqrDistance(target) > robDetectRange * robDetectRange) return false;

        return TryStartAttack(BotAttackIntent.Troll, target);
    }

    private bool TryStartAttack(BotAttackIntent intent, Character target) {
        if (target == null) return false;

        isResumeCatchAfterAttack = currentState == BotStates.Catch;
        if (isResumeCatchAfterAttack) {
            resumeCatchTimer = stateTimer;
            resumeBrainrotSpawner = targetBrainrotSpawner;
        }
        if (intent == BotAttackIntent.Troll) {
            lastTrollTime = Time.time;
        }

        attackIntent = intent;
        huntTarget = target;
        // bo cuoc phai xa hon tam da dung de chon, neu khong vua chon xong da bo ngay
        float scanRange = intent == BotAttackIntent.Troll && currentState == BotStates.Catch ? trollScanRange : robDetectRange;
        chaseGiveUpRange = scanRange * chaseGiveUpRangeMultiplier;
        LogHunt("bat dau " + intent + " -> " + target.GetCharacterName());
        ChangeState(BotStates.Attack);
        return true;
    }

    // uu tien brainrot hiem nhat, bang nhau thi lay nguoi gan hon
    private Character FindRobTarget() {
        Character bestTarget = null;
        float bestScore = float.MinValue;
        List<Character> characters = GetActiveCharacters();

        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (!IsInRange(character, robDetectRange) || !character.IsHoldingCapturedBrainrot() || !IsHuntCandidate(character)) continue;

            float score = (int)character.GetCurrentHoldBrainrotSO().GetRarity() * 100f - Mathf.Sqrt(GetHorizontalSqrDistance(character));
            if (score > bestScore) {
                bestScore = score;
                bestTarget = character;
            }
        }

        return bestTarget;
    }

    private Character FindTrollTarget() {
        trollCandidates.Clear();
        List<Character> characters = GetActiveCharacters();

        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (!IsInRange(character, trollScanRange) || !IsHuntCandidate(character)) continue;

            trollCandidates.Add(character);
        }

        return trollCandidates.Count > 0 ? trollCandidates[Random.Range(0, trollCandidates.Count)] : null;
    }

    // ung vien moi: chua bi qua nhieu bot san, khong phai nguoi vua san xong
    private bool IsHuntCandidate(Character character) {
        if (!IsValidHuntTarget(character)) return false;
        if (character.GetHunterCount() >= maxHuntersPerTarget) return false;

        return character != lastHuntTarget || Time.time - lastHuntTime >= sameTargetCooldown;
    }

    // hop le de san: dang hoat dong, chua chet, khong dang bi danh van va dang o tren floor (khong o base / vung an toan)
    // khong gioi han tang vi bot di len tang khac duoc qua link
    private bool IsValidHuntTarget(Character character) {
        if (character == null || character == this || !character.gameObject.activeInHierarchy) return false;
        if (!character.IsHittable() || character.IsKnocked()) return false;

        return IsOnFloorArea(character.TF.position);
    }

    // ban tia xuong tu chan, trung layer Floor la dang dung tren floor
    private bool IsOnFloorArea(Vector3 position) {
        Vector3 origin = position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, Vector3.down, 0.5f + floorCheckDistance, floorLayerMask, QueryTriggerInteraction.Ignore);
    }

    private void LogHunt(string message) {
        if (!isLogHunt) return;

        Debug.Log("[BotHunt] " + characterName + " (hung hang " + aggression.ToString("0.00") + "): " + message, this);
    }

    private bool IsInRange(Character character, float range) {
        return (character.TF.position - TF.position).sqrMagnitude <= range * range;
    }

    private float GetHorizontalSqrDistance(Character character) {
        Vector3 offset = character.TF.position - TF.position;
        offset.y = 0f;
        return offset.sqrMagnitude;
    }

    private void ChaseHuntTarget() {
        ContinueMoving();
        characterVisual.OnRun();

        chaseRepathTimer -= Time.deltaTime;
        if (chaseRepathTimer > 0f) return;

        chaseRepathTimer = chaseRepathInterval;
        SetDestination(huntTarget.TF.position);
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

        if (newState != null && newState != currentState) {
            LogHunt("state " + GetStateName(currentState) + " -> " + GetStateName(newState));
        }
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

    // brainrot dang roi (vua cuop) van tinh, so voi cho no se cham dat
    private bool IsValidTargetBrainrot() {
        if (targetBrainrot == null || (!targetBrainrot.IsAlive && !targetBrainrot.IsDropping)) return false;

        return (targetBrainrot.GetRestPosition() - currentTargetPoint).sqrMagnitude < 0.001f;
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

    private bool IsKnockbackLanded(Vector3 deltaPosition, out Vector3 landPoint) {
        Vector3 origin = TF.position + new Vector3(deltaPosition.x, 0f, deltaPosition.z) + Vector3.up * 0.5f;
        float distance = 0.5f + Mathf.Abs(deltaPosition.y);
        bool isLanded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, groundLayerMask, QueryTriggerInteraction.Ignore);

        landPoint = hit.point;
        return isLanded;
    }

    private void LandFromKnockback(Vector3 landPoint) {
        if (!NavMesh.SamplePosition(landPoint, out NavMeshHit hit, knockbackSampleDistance, NavMesh.AllAreas)) {
            ChangeState(BotStates.Dead);
            return;
        }

        agent.Warp(hit.position);
        TF.position = hit.position;
        knockbackPhase = KnockbackPhase.Lying;
        stateTimer = 0f;
        KnockDown(TF);
    }

    private void StartKnockbackFlying() {
        knockbackPhase = KnockbackPhase.Flying;
        stateTimer = 0f;
        characterVisual.OnKnockback(knockbackVelocity);
    }

    private void HandleKnockbackLying() {
        stateTimer += Time.deltaTime;
        if (stateTimer < lyingTimerMax) return;

        knockbackPhase = KnockbackPhase.GettingUp;
        stateTimer = 0f;
        characterVisual.OnGetUp();
    }

    // dung day gan xong thi ve Idle, anim idle / run tu blend chong len phan cuoi
    private void HandleKnockbackGettingUp() {
        stateTimer += Time.deltaTime;
        if (!characterVisual.IsGetUpFinished(getUpExitNormalizedTime) && stateTimer < getUpTimerMax) return;

        if (TryStartRevenge()) return;
        ChangeState(BotStates.Idle);
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