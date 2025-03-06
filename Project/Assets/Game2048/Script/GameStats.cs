using UnityEngine;
using Unity.MLAgents;

public class GameStats
{
    // 游戏统计数据
    private bool isGameActive;

    // 历史最佳记录
    private int highestTile;
    
    public GameStats()
    {
        Reset();
        highestTile = 0;
    }

    public void StartNewGame()
    {
        Reset();
        isGameActive = true;
    }

    private void Reset()
    {
        isGameActive = false;
    }
    

    public void OnGameOver(int[,] finalGrid)
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        
        // 计算最终统计
        GameResult result = CalculateGameResult(finalGrid);
        
        // 如果在训练模式，记录到TensorBoard
        if (Academy.Instance.IsCommunicatorOn)
        {
            RecordToTensorBoard(result);
        }
    }

    private GameResult CalculateGameResult(int[,] grid)
    {
        int maxTile = 0;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                maxTile = Mathf.Max(maxTile, grid[i, j]);
            }
        }

        highestTile = Mathf.Max(highestTile, maxTile);
        return new GameResult
        {
            HistoryMaxTile = highestTile,
            MaxTile = maxTile,
        };
    }

    private void RecordToTensorBoard(GameResult result)
    {
        var statsRecorder = Academy.Instance.StatsRecorder;
        statsRecorder.Add("历史最大值", result.HistoryMaxTile);
        statsRecorder.Add("最大值", result.MaxTile);
    }
    
}

// 游戏结果数据结构
public struct GameResult
{
    public int HistoryMaxTile { get; set; }
    public int MaxTile { get; set; }
}