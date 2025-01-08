using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class Game : Agent
{
    [SerializeField] private BoxItem[] boxItems;
    [SerializeField] private MoveBoxItem moveBoxItem;
    [SerializeField] private Text markText;
    private GameLogic gameLogic;
    private float moveTime = 0.25f;
    private bool canMove = true;
    private bool isShowAnimation = false;

    private void Start()
    {
        gameLogic = new GameLogic(4, 0.9f);
        var data = gameLogic.GetGrid();
        SetGameBox(data);
    }
    #region 逻辑

    private void MoveUp()
    {
        MoveDir(GameLogic.MoveDirection.Up);
    }

    private void MoveDown()
    {
        MoveDir(GameLogic.MoveDirection.Down);
    }

    private void MoveLeft()
    {
        MoveDir(GameLogic.MoveDirection.Left);
    }

    private void MoveRight()
    {
        MoveDir(GameLogic.MoveDirection.Right);
    }

    private void MoveDir(GameLogic.MoveDirection dir)
    {
        if (isShowAnimation)
        {
            if (!canMove) return;
        }

        (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) =
            gameLogic.Move(dir);
        if (moved)
        {
            if (isShowAnimation)
            {
                canMove = false;
                StartCoroutine(_enumerator(moveTime));
                ShowAnimations(animations);
            }
            else
            {
                var data = gameLogic.GetGrid();
                SetGameBox(data);
            }

            if (gameLogic.IsGameOver())
            {
                OnGameOver();
            }
        }
    }

    private void OnGameOver()
    {
        Debug.LogWarning("游戏结束");
        //从新开始
    }

    private IEnumerator _enumerator(float time)
    {
        yield return new WaitForSeconds(time);
        SetGameBox(gameLogic.GetGrid());
        canMove = true;
    }

    /// <summary>
    /// 更具 grid 设置boxItems
    /// </summary>
    /// <param name="grid"></param>
    private void SetGameBox(int[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                var boxItem = boxItems[i * grid.GetLength(1) + j];
                boxItem.SetData(grid[i, j]);
            }
        }
        //设置显示分数
        markText.text = gameLogic.GetScore().ToString();
    }

    private void ShowAnimations(List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations)
    {
        foreach (var item in animations)
        {
            //通过 item 获取 boxItems 中对应的 开始和结束的 boxItem
            var fromBoxItem = boxItems[item.fromRow * 4 + item.fromCol];
            var toBoxItem = boxItems[item.toRow * 4 + item.toCol];
            fromBoxItem.Hid();
            //开始动画
            MoveBoxItem moveBox = Instantiate(moveBoxItem, transform).GetComponent<MoveBoxItem>();
            moveBox.StartAnimation(moveTime, fromBoxItem.transform.position, toBoxItem.transform.position,
                fromBoxItem.count);
        }
    }

    #endregion

    #region 代理

    private float currentScore = 0;
    private float previousScore = 0;
    private const float MOVE_PENALTY = -0.1f;
    private const float MERGE_REWARD = 0.5f;
    private const float GAME_OVER_PENALTY = -1f;

    public override void Initialize()
    {
        isShowAnimation = false; // 训练时关闭动画
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 收集4x4网格的状态
        var grid = gameLogic.GetGrid();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // 将数值归一化到0-1之间
                float normalizedValue = grid[i, j] == 0 ? 0 : Mathf.Log(grid[i, j], 2) / 11f; // 11 = log2(2048)
                sensor.AddObservation(normalizedValue);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 获取离散动作
        int action = actionBuffers.DiscreteActions[0];

        // 执行移动
        switch (action)
        {
            case 0:
                MoveUp();
                break;
            case 1:
                MoveDown();
                break;
            case 2:
                MoveLeft();
                break;
            case 3:
                MoveRight();
                break;
        }

        // 计算奖励
        currentScore = gameLogic.GetScore();
        float reward = currentScore - previousScore;

        if (reward > 0)
        {
            // 合并砖块时给予奖励
            AddReward(reward * MERGE_REWARD);
        }
        else
        {
            // 无效移动的惩罚
            AddReward(MOVE_PENALTY);
        }

        previousScore = currentScore;

        // 检查游戏是否结束
        if (gameLogic.IsGameOver())
        {
            AddReward(GAME_OVER_PENALTY);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        // 重置游戏状态
        gameLogic = new GameLogic(4, 0.9f);
        currentScore = 0;
        previousScore = 0;
        var data = gameLogic.GetGrid();
        SetGameBox(data);
    }
    // 用于测试的手动控制方法
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 0;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.A)) discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.D)) discreteActionsOut[0] = 3;
    }

    #endregion
}
