﻿public class PlayerEntity : BaseEntity
{

    internal override void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;

        input.yaw = MathManager.YawStop;
        input.key = 0;

        animation.fixedTransitionDuration = 0.0f;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;
        animation.enable = true;

        attack.targets = new int[PlayerPropertyConstant.atkMaxCount];
        attack.atk = PlayerPropertyConstant.Attack;
        attack.attackDistance = PlayerPropertyConstant.AttackDistance;
        attack.lastAttackTime = -1;

        runtimeProperty.seed = BattleConstant.randomSeed;

        property.hp = PlayerPropertyConstant.HP;
        property.camp = (ECamp)data.camp;

        collision.collsionSize = PlayerPropertyConstant.CollisionRadius;

        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;
        state.count = (int)EPlayerState.Count;

        movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;

        InitBuffs();
    }

    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    public override float GetCollisionRadius(BattleEntity battleEntity)
    {
        return collision.collsionSize;
    }

}
