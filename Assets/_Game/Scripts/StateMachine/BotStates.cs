using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BotStates {
    public static readonly BotState Idle = new BotState(IdleState.OnEnter, IdleState.OnExecute, IdleState.OnExit);

    public static readonly BotState Patrol = new BotState(PatrolState.OnEnter, PatrolState.OnExecute, PatrolState.OnExit);

    public static readonly BotState Catch = new BotState(CatchState.OnEnter, CatchState.OnExecute, CatchState.OnExit);

    public static readonly BotState Return = new BotState(ReturnState.OnEnter, ReturnState.OnExecute, ReturnState.OnExit);

    public static readonly BotState Dead = new BotState(DeadState.OnEnter, DeadState.OnExecute, DeadState.OnExit);

    public static readonly BotState Knockback = new BotState(KnockbackState.OnEnter, KnockbackState.OnExecute, KnockbackState.OnExit);

    public static readonly BotState Attack = new BotState(AttackState.OnEnter, AttackState.OnExecute, AttackState.OnExit);
}

public class BotState {
    public readonly Action<Bot> OnEnter;
    public readonly Action<Bot> OnExecute;
    public readonly Action<Bot> OnExit;

    public BotState(Action<Bot> onEnter, Action<Bot> onExecute, Action<Bot> onExit) {
        OnEnter = onEnter;
        OnExecute = onExecute;
        OnExit = onExit;
    }
}