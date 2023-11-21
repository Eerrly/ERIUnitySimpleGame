[PlayerState(EPlayerState.Move)]
public class PlayerMoveState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.Movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        playerEntity.Movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;
    }

    public override void OnUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        MoveSystem.UpdatePosition(playerEntity);
        MoveSystem.UpdateRotation(playerEntity);
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (KeySystem.IsYawTypeStop(playerEntity.Input.yaw))
        {
            EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.Idle);
        }
        if (KeySystem.CheckKeyCodeJDown(playerEntity))
        {
#if UNITY_DEBUG
            Logger.Log(LogLevel.Info, $"玩家ID:{playerEntity.ID} 玩家状态：{System.Enum.GetName(typeof(EPlayerState), StateId)} 触发按键 J");
#endif
        }
    }

    public override void OnCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
#if UNITY_DEBUG
        Logger.Log(LogLevel.Info, $"发生碰撞 玩家状态：{System.Enum.GetName(typeof(EPlayerState), StateId)} S:{source.ID} T:{target.ID}");
#endif
    }

    public override void OnPostCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
        PhysicsSystem.CheckCollisionDir(source, target);
#if UNITY_DEBUG
        Logger.Log(LogLevel.Info, $"发生碰撞 玩家状态：{System.Enum.GetName(typeof(EPlayerState), StateId)} 碰撞方向:{System.Enum.GetName(typeof(ECollisionDir), source.Collision.collisionDir)} S:{source.ID} T:{target.ID}");
#endif
    }

    public override void OnExit(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        playerEntity.Movement.position = FixedVector3.Zero;
        playerEntity.Movement.rotation = FixedQuaternion.Identity;
        playerEntity.Movement.moveSpeed = FixedNumber.Zero;
        playerEntity.Movement.turnSpeed = 0f;
    }
}
