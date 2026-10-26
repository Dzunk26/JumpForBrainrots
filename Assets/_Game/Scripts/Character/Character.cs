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
    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected float attackRadius = 2.25f;
    [SerializeField] protected float attackHeight = 2.875f;
    [SerializeField] protected float attackTimerMax = 0.9f;
    [SerializeField] protected float knockbackForce = 60f;
    [SerializeField] protected float knockbackUpForce = 45f;
    [SerializeField] protected LayerMask characterLayerMask = ~0;

    protected BrainrotSO currentHoldBrainrotSO;
    protected HeldBrainrotSource heldBrainrotSource;
    protected Vector3 spawnPoint;
    protected string characterName;
    protected float lastAttackTime = -1f;
    protected bool isAttacking;

    private Collider[] hitColliders = new Collider[16];
    private List<Character> hitCharacters = new List<Character>();

    private void OnDrawGizmosSelected() {
        if (characterVisual == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackPoint(), attackRadius);
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

    public void Attack() {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        characterVisual.OnAttack();
    }

    // goi tu animation event dung frame gay cham muc tieu
    public void OnAttackHit() {
        if (!isAttacking) return;

        isAttacking = false;
        hitCharacters.Clear();
        int count = Physics.OverlapSphereNonAlloc(GetAttackPoint(), attackRadius, hitColliders, characterLayerMask, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++) {
            Character character = Cache.GetCharacter(hitColliders[i]);
            if (character == null || character == this || hitCharacters.Contains(character)) continue;

            hitCharacters.Add(character);
            character.OnHit(GetKnockbackVelocity(character));
        }
    }

    public virtual void OnHit(Vector3 knockbackVelocity) { }

    protected virtual bool CanAttack() {
        return Time.time - lastAttackTime >= attackTimerMax;
    }

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

    private Vector3 GetAttackPoint() {
        return characterVisual.TF.position + characterVisual.TF.forward * attackRange + Vector3.up * attackHeight;
    }

    private Vector3 GetKnockbackVelocity(Character target) {
        Vector3 direction = target.TF.position - TF.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = characterVisual.TF.forward;

        return direction.normalized * knockbackForce + Vector3.up * knockbackUpForce;
    }
}