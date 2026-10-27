using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeldBrainrotSource {
    None,
    Captured,
    Inventory,
}

public class Character : GameUnit {
    [SerializeField] protected CharacterStat characterStat;
    [SerializeField] protected CharacterVisual characterVisual;
    [SerializeField] protected BaseHouse baseHouse;

    [Header("Attack")]
    [SerializeField] protected float attackHeight = 2.875f; // do cao tam vung gay (ngang nguc)
    [SerializeField] protected float attackTimerMax = 0.9f;
    [SerializeField] protected float hitRange = 5.5f; // vung trung hinh quat tinh tu nguoi danh
    [SerializeField] protected float hitAngle = 150f; // goc quat phia truoc
    [SerializeField] protected float hitHeightTolerance = 3f; // chenh cao toi da giua 2 nguoi
    [SerializeField] protected float attackTargetExtraRange = 1.5f; // muc tieu da nham luc bam danh duoc tinh xa hon, khong can trong goc
    [SerializeField] protected float knockbackForce = 60f;
    [SerializeField] protected float knockbackUpForce = 45f;

    [Header("Knocked Down")]
    [SerializeField] protected float lyingTimerMax = 1.5f; // nam tren dat bao lau moi dung day
    [SerializeField] protected float getUpExitNormalizedTime = 0.85f; // anim dung day chay toi day thi tra lai dieu khien, phan con lai blend sang idle / run
    [SerializeField] protected float getUpTimerMax = 3f; // phong khi anim dung day khong chay thi van dung day

    [Header("Scatter Brainrot")]
    [SerializeField] protected Vector2 scatterDistanceRange = new Vector2(1.5f, 3f); // brainrot dang bat bi vang ra cach cho dung bao xa
    [SerializeField] protected float scatterGroundCheckDistance = 10f;

    protected BrainrotSO currentHoldBrainrotSO;
    protected HeldBrainrotSource heldBrainrotSource;
    protected Vector3 spawnPoint;
    protected string characterName;
    protected float lastAttackTime = -1f;
    protected bool isAttacking;
    protected Character attackTarget; // muc tieu da nham luc bam danh (player tu nham, bot tu gan sau)
    protected Brainrot scatteredBrainrot; // brainrot vua vang ra khi bi danh / chet, nguoi danh doc de chay toi nhat

    private int hunterCount; // so bot dang san nguoi nay, gioi han de khong bi hoi dong

    // nhan vat dang hoat dong (player + bot dang spawn). quet bang danh sach nay thay vi OverlapSphere
    // vi OverlapSphere tam rong de bi san / tuong / trigger lap day bo dem, khong thay nhan vat
    private static List<Character> activeCharacters = new List<Character>();

    private List<Character> hitCharacters = new List<Character>();

    public static List<Character> GetActiveCharacters() {
        return activeCharacters;
    }

    protected virtual void OnEnable() {
        if (!activeCharacters.Contains(this)) {
            activeCharacters.Add(this);
        }
    }

    protected virtual void OnDisable() {
        activeCharacters.Remove(this);
    }

    private void OnDrawGizmosSelected() {
        if (characterVisual == null) return;

        Vector3 center = GetAttackCenter();
        Vector3 forward = characterVisual.TF.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(-hitAngle * 0.5f, Vector3.up) * forward * hitRange);
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(hitAngle * 0.5f, Vector3.up) * forward * hitRange);
        Gizmos.DrawLine(center, center + forward * hitRange);
    }

    public virtual void OnInit() {
        spawnPoint = baseHouse.GetSpawnPosition();
        InitBaseHouse();
    }

    public virtual void OnDespawn() { }

    public virtual void StoreCapturedBrainrot() { }

    public virtual void OnCaptureBrainrot() { }

    public virtual void OnInteractBaseSlot() { }

    public virtual void Dead() { }

    public virtual void InitBaseHouse() { }

    public virtual float GetMoneyMultiplier() {
        return 1f;
    }

    public BaseHouse GetBaseHouse() {
        return baseHouse;
    }

    public string GetCharacterName() {
        return characterName;
    }

    // chet thi khong nham / danh trung nua
    public virtual bool IsHittable() {
        return true;
    }

    // dang bay / nam / dung day sau khi bi danh
    public virtual bool IsKnocked() {
        return false;
    }

    public bool IsHoldingCapturedBrainrot() {
        return heldBrainrotSource == HeldBrainrotSource.Captured;
    }

    public BrainrotSO GetCurrentHoldBrainrotSO() {
        return currentHoldBrainrotSO;
    }

    public int GetHunterCount() {
        return hunterCount;
    }

    public void AddHunter(int amount) {
        hunterCount = Mathf.Max(0, hunterCount + amount);
    }

    public void Attack() {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        OnStartAttack();
        characterVisual.OnAttack();
    }

    // goi tu animation event dung frame gay cham muc tieu
    public void OnAttackHit() {
        if (!isAttacking) return;

        isAttacking = false;
        hitCharacters.Clear();

        // gom truoc roi moi danh, vi OnHit co the doi state lam danh sach thay doi
        for (int i = 0; i < activeCharacters.Count; i++) {
            Character character = activeCharacters[i];
            if (character == this || !character.IsHittable() || !IsInHitArea(character)) continue;

            hitCharacters.Add(character);
        }

        for (int i = 0; i < hitCharacters.Count; i++) {
            Character character = hitCharacters[i];
            character.scatteredBrainrot = null;
            character.OnHit(GetKnockbackVelocity(character), this);
            OnHitCharacter(character, character.scatteredBrainrot);
        }

        attackTarget = null;
    }

    public virtual void OnHit(Vector3 knockbackVelocity, Character attacker) { }

    // danh trung 1 nguoi, scatteredBrainrot la brainrot nguoi do vua lam roi (null neu khong cam)
    protected virtual void OnHitCharacter(Character target, Brainrot scatteredBrainrot) { }

    protected virtual bool CanAttack() {
        return Time.time - lastAttackTime >= attackTimerMax;
    }

    protected virtual void OnStartAttack() { }

    protected virtual void HoldBrainrot(BrainrotSO brainrotSO, Vector3 fromPosition, HeldBrainrotSource source) {
        currentHoldBrainrotSO = brainrotSO;
        heldBrainrotSource = source;
        characterVisual.OnCaptureBrainrot(brainrotSO);
        characterVisual.FlyBrainrotFrom(fromPosition);
    }

    protected virtual void DropBrainrot() {
        currentHoldBrainrotSO = null;
        heldBrainrotSource = HeldBrainrotSource.None;
        characterVisual.OnDropBrainrot();
    }

    // bi danh / chet: brainrot dang bat (chua cat vao kho) vang ra roi xuong dat gan do, ai cung nhat lai duoc
    // duoi chan khong co dat (roi khoi map) thi mat
    protected void ScatterCapturedBrainrot(LayerMask groundLayerMask) {
        if (heldBrainrotSource != HeldBrainrotSource.Captured || currentHoldBrainrotSO == null) return;

        BrainrotSO brainrotSO = currentHoldBrainrotSO;
        Vector3 fromPosition = characterVisual.GetBrainrotHoldPosition();
        DropBrainrot();

        if (!TryGetScatterPoint(fromPosition, groundLayerMask, out Vector3 landPoint)) return;

        BrainrotSpawner brainrotSpawner = FloorManager.Instance.GetClosestBrainrotSpawner(landPoint);
        if (brainrotSpawner == null) return;

        scatteredBrainrot = brainrotSpawner.SpawnDroppedBrainrot(brainrotSO, fromPosition, landPoint);
    }

    // thu 1 diem ngau nhien xung quanh, khong co dat (mep san) thi roi ngay duoi chan
    private bool TryGetScatterPoint(Vector3 fromPosition, LayerMask groundLayerMask, out Vector3 landPoint) {
        Vector2 random = Random.insideUnitCircle.normalized * Random.Range(scatterDistanceRange.x, scatterDistanceRange.y);
        Vector3[] candidates = { TF.position + new Vector3(random.x, 0f, random.y), TF.position };

        foreach (Vector3 candidate in candidates) {
            Vector3 origin = new Vector3(candidate.x, fromPosition.y, candidate.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, scatterGroundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore)) {
                landPoint = hit.point;
                return true;
            }
        }

        landPoint = Vector3.zero;
        return false;
    }

    // cham dat sau khi bi danh: xoay huong nhan vat ngau nhien de moi lan nam mot huong
    protected void KnockDown(Transform facingTF) {
        float yawOffset = Random.Range(-180f, 180f);
        facingTF.rotation = Quaternion.AngleAxis(yawOffset, Vector3.up) * facingTF.rotation;
        characterVisual.OnKnockedDown(yawOffset);
    }

    // gan nhu dinh nhau thi luon trung, muc tieu da nham chi can trong tam, con lai phai nam trong goc quat
    protected bool IsInHitArea(Character target) {
        Vector3 offset = target.TF.position - TF.position;
        if (Mathf.Abs(offset.y) > hitHeightTolerance) return false;

        offset.y = 0f;
        float distance = offset.magnitude;
        if (target == attackTarget) return distance <= hitRange + attackTargetExtraRange;
        if (distance > hitRange) return false;

        return distance < 0.5f || Vector3.Angle(characterVisual.TF.forward, offset) <= hitAngle * 0.5f;
    }

    private Vector3 GetAttackCenter() {
        return TF.position + Vector3.up * attackHeight;
    }

    private Vector3 GetKnockbackVelocity(Character target) {
        Vector3 direction = target.TF.position - TF.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = characterVisual.TF.forward;

        return direction.normalized * knockbackForce + Vector3.up * knockbackUpForce;
    }
}