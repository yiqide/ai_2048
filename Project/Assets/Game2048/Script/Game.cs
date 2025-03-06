using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
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

    
    // 初始化游戏逻辑和相关变量
    private void Start()
    {
        gameLogic = new GameLogic(4, 0.9f);
        var data = gameLogic.GetGrid();
        SetGameBox(data);
    }

    #region 基础游戏逻辑
    
    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="dir"></param>
    /// <returns>移动是否成功</returns>
    private bool MoveDir(GameLogic.MoveDirection dir)
    {
        if (isShowAnimation && !canMove) return false;

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
        }

        return moved;
    }

    // 动画结束后更新游戏状态
    private IEnumerator _enumerator(float time)
    {
        yield return new WaitForSeconds(time);
        SetGameBox(gameLogic.GetGrid());
        canMove = true;
    }

    // 更新游戏界面显示
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
        markText.text = gameLogic.GetScore().ToString();
    }

    // 显示移动动画
    private void ShowAnimations(List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations)
    {
        foreach (var item in animations)
        {
            var fromBoxItem = boxItems[item.fromRow * 4 + item.fromCol];
            var toBoxItem = boxItems[item.toRow * 4 + item.toCol];
            fromBoxItem.Hid();
            MoveBoxItem moveBox = Instantiate(moveBoxItem, transform).GetComponent<MoveBoxItem>();
            moveBox.StartAnimation(moveTime, fromBoxItem.transform.position, toBoxItem.transform.position,
                fromBoxItem.count);
        }
    }
    #endregion
    
     private int previousScore = 0;
    private int previousMaxTile = 0;
    private int invalidMovesCount = 0;
    private const int MAX_INVALID_MOVES = 4;

    public override void OnEpisodeBegin()
    {
        gameLogic = new GameLogic(4, 0.9f);
        var data = gameLogic.GetGrid();
        SetGameBox(data);
        previousScore = 0;
        previousMaxTile = 2;
        invalidMovesCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var grid = gameLogic.GetGrid();

        // 1. 基础网格状态（使用log2归一化）
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float normalizedValue = grid[i, j] == 0 ? 0 : Mathf.Log(grid[i, j], 2) / 11f;
                sensor.AddObservation(normalizedValue);
            }
        }

        // 2. 空格位置分布
        int emptyCount = 0;
        int[] rowEmptyCount = new int[4];
        int[] colEmptyCount = new int[4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] == 0)
                {
                    emptyCount++;
                    rowEmptyCount[i]++;
                    colEmptyCount[j]++;
                }
            }
        }

        // 添加空格分布信息
        sensor.AddObservation(emptyCount / 16f); // 总空格比例
        for (int i = 0; i < 4; i++)
        {
            sensor.AddObservation(rowEmptyCount[i] / 4f); // 每行空格比例
            sensor.AddObservation(colEmptyCount[i] / 4f); // 每列空格比例
        }

        // 3. 单调性分析（数字是否按序排列）
        // 单调性分析
        float[] monotonicity = gameLogic.GetMonotonicityAnalysis();
        foreach (float mono in monotonicity)
            sensor.AddObservation(mono);

        // 4. 相邻格子的关系
        // 平滑度
        sensor.AddObservation(gameLogic.GetSmoothness());
        // 分析每个方向的移动
        foreach (GameLogic.MoveDirection dir in Enum.GetValues(typeof(GameLogic.MoveDirection)))
        {
            if (dir != GameLogic.MoveDirection.Invalid)
            {
                var analysis = gameLogic.AnalyzeMove(dir);
                sensor.AddObservation(analysis.canMove);
                sensor.AddObservation(analysis.mergeCount / 8f);
                sensor.AddObservation(analysis.mergeValue / 2048f);
                sensor.AddObservation(analysis.emptyAfterMove / 16f);
                sensor.AddObservation(analysis.maxTileAfterMove / 2048f);
            }
        }


        // 6. 游戏状态指标
        sensor.AddObservation(gameLogic.GetScore() / 20000f); // 归一化分数
        sensor.AddObservation(gameLogic.GetMaxTile() / 2048f); // 归一化最大数字
        sensor.AddObservation(invalidMovesCount / (float)MAX_INVALID_MOVES);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        if (gameLogic.IsGameOver())
        {
            EndEpisode();
            return;
        }

        int action = actions.DiscreteActions[0];
        GameLogic.MoveDirection direction = (GameLogic.MoveDirection)action;
        bool moved = MoveDir(direction);

        // 计算奖励
        float reward = CalculateReward(moved);
        AddReward(reward);

        if (!moved)
        {
            invalidMovesCount++;
            if (invalidMovesCount >= MAX_INVALID_MOVES)
            {
                AddReward(-1f); // 惩罚连续无效移动
                EndEpisode();
            }
        }
        else
        {
            invalidMovesCount = 0;
        }
    }

    private float CalculateReward(bool moved)
    {
        float reward = 0;
        int currentScore = gameLogic.GetScore();
        int currentMaxTile = gameLogic.GetMaxTile();

        if (!moved)
        {
            return -0.1f; // 惩罚无效移动
        }

        // 奖励分数增长
        if (currentScore > previousScore)
        {
            reward += (currentScore - previousScore) / 1000f;
        }

        // 奖励最大数字增长
        if (currentMaxTile > previousMaxTile)
        {
            reward += 0.5f;
        }

        // 奖励保持角落策略
        reward += EvaluateCornerStrategy();

        previousScore = currentScore;
        previousMaxTile = currentMaxTile;

        return reward;
    }

    private float EvaluateCornerStrategy()
    {
        var grid = gameLogic.GetGrid();
        float reward = 0;
        
        // 检查右下角是否是最大数字
        int maxTile = gameLogic.GetMaxTile();
        if (grid[3, 3] == maxTile)
        {
            reward += 0.1f;
        }

        // 检查数字是否呈递减趋势
        bool isDescending = true;
        for (int i = 3; i >= 1; i--)
        {
            if (grid[3, i] < grid[3, i-1] || grid[i, 3] < grid[i-1, 3])
            {
                isDescending = false;
                break;
            }
        }
        if (isDescending)
        {
            reward += 0.1f;
        }

        return reward;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKeyDown(KeyCode.UpArrow)) discreteActionsOut[0] = 0;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) discreteActionsOut[0] = 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) discreteActionsOut[0] = 2;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) discreteActionsOut[0] = 3;
    }
}
