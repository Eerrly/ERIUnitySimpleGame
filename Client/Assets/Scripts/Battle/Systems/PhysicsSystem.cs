/// <summary>
/// 物理对象
/// </summary>
public struct PhysisPlayer
{
    public int ID;

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        return ID == ((PhysisPlayer)obj).ID;
    }

    public static bool operator ==(PhysisPlayer a, PhysisPlayer b)
    {
        return a.ID == b.ID;
    }

    public static bool operator !=(PhysisPlayer a, PhysisPlayer b)
    {
        return a.ID != b.ID;
    }
}

/// <summary>
/// 物理系统
/// </summary>
[EntitySystem]
public class PhysicsSystem
{
    /// <summary>
    /// 轮询
    /// </summary>
    /// <param name="battleEntity"></param>
    public static void Update(BattleEntity battleEntity)
    {
        var entities = battleEntity.Entities;
        foreach (var source in entities)
        {
            source.RuntimeProperty.closedPlayers.Clear();
            foreach (var target in entities)
            {
                if(source.ID == target.ID)
                {
                    continue;
                }
                var radius = source.GetCollisionRadius(battleEntity) + target.GetCollisionRadius(battleEntity);
                var sqrMagnitudeXZ = (target.Transform.pos - source.Transform.pos).sqrMagnitudeLongXZ;
                if (sqrMagnitudeXZ <= radius * radius)
                {
                    source.RuntimeProperty.closedPlayers.Add(new PhysisPlayer() { ID = target.ID });
                }
            }
        }

        foreach (var source in entities)
        {
            var closedPlayers = source.RuntimeProperty.closedPlayers;
            for (int j = 0; j < closedPlayers.Count; j++)
            {
                var target = battleEntity.FindEntity(closedPlayers[j].ID);
                UpdateCollision(source, target, battleEntity);
            }
        }
    }

    /// <summary>
    /// 轮询碰撞检测
    /// </summary>
    /// <param name="source">玩家A</param>
    /// <param name="target">玩家B</param>
    /// <param name="battleEntity">战斗实体</param>
    private static void UpdateCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
        var sMove = source.Movement.position;
        var tMove = target.Movement.position;

        var vecS2T = (source.Transform.pos - target.Transform.pos).YZero();
        var vecT2S = (target.Transform.pos - source.Transform.pos).YZero();

        if (FixedVector3.Dot(ref vecS2T, ref tMove) > FixedNumber.Zero)
        {
            sMove = CombineForce(tMove, sMove);
        }
        else
        {
            vecS2T = -vecS2T;
            if (FixedVector3.Dot(sMove, vecS2T) > FixedNumber.Zero)
            {
                sMove -= FixedVector3.Project(sMove, vecS2T);
            }
        }
        source.Movement.position = sMove;

        if (PlayerStateMachine.Instance.GetState(source.State.curStateId) is PlayerBaseState state)
        {
            if (FixedVector3.Dot(vecT2S, sMove) > FixedNumber.Zero)
            {
                state.OnCollision(source, target, battleEntity);
            }
            state.OnPostCollision(source, target, battleEntity);
        }
    }

    /// <summary>
    /// 合并碰撞之后的移动向量
    /// </summary>
    /// <param name="aDeltaMove">玩家A位移向量</param>
    /// <param name="bDeltaMove">玩家B位移向量</param>
    /// <returns></returns>
    private static FixedVector3 CombineForce(FixedVector3 aDeltaMove, FixedVector3 bDeltaMove)
    {
        var lenB = bDeltaMove.Magnitude;
        var lenA = aDeltaMove.Magnitude;

        var normalB = bDeltaMove.Normalized;
        var normalA = aDeltaMove.Normalized;

        var dot = FixedVector3.Dot(normalB, normalA);
        var lenC = dot * lenA;
        var cDeltaMove = normalA * lenC;

        lenC = FixedMath.Max(lenB, lenC);
        return normalB * lenC + (bDeltaMove - cDeltaMove);
    }

    
    /// <summary>
    /// 检测碰撞方向
    /// -45  45
    ///    \/
    ///    /\
    /// -135 135
    /// </summary>
    public static void CheckCollisionDir(BaseEntity source, BaseEntity target)
    {
        source.Collision.collisionDir = 0;
        var direction = target.Transform.pos - source.Transform.pos;
        var angle = FixedVector3.AngleIntSingle(source.Transform.fwd, direction.YZero().Normalized);
        if(-45 < angle && angle < 45)
        {
            source.Collision.collisionDir = (int)ECollisionDir.Forward;
        }
        else if(-135 < angle && angle < -45)
        {
            source.Collision.collisionDir = (int)ECollisionDir.Left;
        }
        else if(135 > angle && angle > 45)
        {
            source.Collision.collisionDir = (int)ECollisionDir.Right;
        }
        else if(135 < angle || angle < -135)
        {
            source.Collision.collisionDir = (int)ECollisionDir.Back;
        }
    }

}
