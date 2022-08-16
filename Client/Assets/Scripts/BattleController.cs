﻿public class BattleController : IBattleController
{
    public int nextFrame;

    public BattleEntity battleEntity { get; set; }

    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        battleEntity.Init();
        for (int i = 0; i < data.players.Length; i++)
        {
            PlayerEntity playerEntity = new PlayerEntity();
            playerEntity.Init(data.players[i]);
            battleEntity.playerList.Add(playerEntity);
        }
    }

    public override void Initialize()
    {
    }

    public override void LogicUpdate()
    {
        try
        {
            if (!Paused)
            {
                battleEntity.deltaTime = FrameEngine.frameInterval * battleEntity.timeScale;
                battleEntity.time += battleEntity.deltaTime;

                UpdateInput();

                var playerList = battleEntity.playerList;

                BattleStateMachine.Instance.Update(battleEntity, null);
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.Update(playerList[i], battleEntity);
                }

                BattleStateMachine.Instance.LateUpdate(battleEntity, null);
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.LateUpdate(playerList[i], battleEntity);
                }

                PhysicsSystem.Update(battleEntity);

                BattleStateMachine.Instance.DoChangeState(battleEntity, null);
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.DoChangeState(playerList[i], battleEntity);
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public void UpdateInput() {
        var input = BattleManager.Instance.GetInput();
        battleEntity.selfPlayerEntity.input.yaw = input.yaw - MathManager.YawOffset;
        battleEntity.selfPlayerEntity.input.key = input.key;
    }

    public override void RenderUpdate()
    {
        try
        {
            BattleManager.Instance.battleView.RenderUpdate(battleEntity);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public override void Release() { }

    public override void GameOver() { }

}