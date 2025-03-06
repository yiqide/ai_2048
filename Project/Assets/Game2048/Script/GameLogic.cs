// 逻辑部分：GameLogic.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GameLogic
{
    private int currentScore = 0; // 添加分数字段
    private int[,] grid; // 网格数组，存储当前游戏状态
    private int gridSize; // 网格的大小（行列数）
    private Random random; // 随机数生成器，用于生成随机数字
    private double probabilityOfTwo = 0.9; // 配置生成2的概率

    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right,
        Invalid
    }

    // 在构造函数中初始化分数
    public GameLogic(int size, double probabilityOfTwo = 0.9)
    {
        gridSize = size;
        grid = new int[gridSize, gridSize];
        random = new Random();
        this.probabilityOfTwo = probabilityOfTwo;
        currentScore = 0;  // 初始化分数
        SpawnRandomTile();
        SpawnRandomTile();
    }
    // 添加获取分数的方法
    public int GetScore()
    {
        return currentScore;
    }
    // 添加重置分数的方法（在游戏重新开始时调用）
    public void ResetScore()
    {
        currentScore = 0;
    }
    // 获取当前网格状态
    public int[,] GetGrid() => (int[,])grid.Clone();

    public int GetMaxTile()
    {
        int result = 0;
        foreach (var item in grid)
        {
            result =Math.Max(result, item);
        }
        return result;
    }
    // 判断游戏是否失败
    public bool IsGameOver()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] == 0) return false; // 有空格，未失败

                // 检查周围是否有相同的数字
                if (row > 0 && grid[row, col] == grid[row - 1, col]) return false;
                if (row < gridSize - 1 && grid[row, col] == grid[row + 1, col]) return false;
                if (col > 0 && grid[row, col] == grid[row, col - 1]) return false;
                if (col < gridSize - 1 && grid[row, col] == grid[row, col + 1]) return false;
            }
        }
        return true; // 无法移动，游戏结束
    }

    // 根据方向移动并返回是否成功及动画信息
    public (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) Move(MoveDirection direction)
    {
        bool moved = false;
        List<(int, int, int, int, int)> animations = new List<(int, int, int, int, int)>();

        switch (direction)
        {
            case MoveDirection.Up:
                moved = MoveUp(animations);
                break;
            case MoveDirection.Down:
                moved = MoveDown(animations);
                break;
            case MoveDirection.Left:
                moved = MoveLeft(animations);
                break;
            case MoveDirection.Right:
                moved = MoveRight(animations);
                break;
        }

        if (moved)
        {
            SpawnRandomTile(); // 如果有移动，生成一个新的随机数字
        }
        return (moved, animations);
    }

    // 向上移动
    private bool MoveUp(List<(int, int, int, int, int)> animations)
    {
        bool moved = false;
        for (int col = 0; col < gridSize; col++)
        {
            int[] column = new int[gridSize]; // 提取列数据
            for (int row = 0; row < gridSize; row++)
            {
                column[row] = grid[row, col];
            }

            int[] newColumn = Collapse(column, out var mergeInfo); // 合并列数据
            for (int row = 0; row < gridSize; row++)
            {
                if (grid[row, col] != newColumn[row])
                {
                    moved = true;
                }
                grid[row, col] = newColumn[row]; // 更新网格数据
            }

            foreach (var (from, to, value) in mergeInfo)
            {
                animations.Add((from, col, to, col, value));
            }
        }
        return moved;
    }

    // 向下移动
    private bool MoveDown(List<(int, int, int, int, int)> animations)
    {
        bool moved = false;
        for (int col = 0; col < gridSize; col++)
        {
            int[] column = new int[gridSize]; // 提取列数据
            for (int row = 0; row < gridSize; row++)
            {
                column[row] = grid[row, col];
            }

            Array.Reverse(column); // 反转列数据以便合并
            int[] newColumn = Collapse(column, out var mergeInfo);
            Array.Reverse(newColumn); // 恢复顺序

            for (int row = 0; row < gridSize; row++)
            {
                if (grid[row, col] != newColumn[row])
                {
                    moved = true;
                }
                grid[row, col] = newColumn[row]; // 更新网格数据
            }

            foreach (var (from, to, value) in mergeInfo)
            {
                animations.Add((gridSize - 1 - from, col, gridSize - 1 - to, col, value));
            }
        }
        return moved;
    }

    // 向左移动
    private bool MoveLeft(List<(int, int, int, int, int)> animations)
    {
        bool moved = false;
        for (int row = 0; row < gridSize; row++)
        {
            int[] line = new int[gridSize]; // 提取行数据
            for (int col = 0; col < gridSize; col++)
            {
                line[col] = grid[row, col];
            }

            int[] newLine = Collapse(line, out var mergeInfo); // 合并行数据
            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] != newLine[col])
                {
                    moved = true;
                }
                grid[row, col] = newLine[col]; // 更新网格数据
            }

            foreach (var (from, to, value) in mergeInfo)
            {
                animations.Add((row, from, row, to, value));
            }
        }
        return moved;
    }

    // 向右移动
    private bool MoveRight(List<(int, int, int, int, int)> animations)
    {
        bool moved = false;
        for (int row = 0; row < gridSize; row++)
        {
            int[] line = new int[gridSize]; // 提取行数据
            for (int col = 0; col < gridSize; col++)
            {
                line[col] = grid[row, col];
            }

            Array.Reverse(line); // 反转行数据以便合并
            int[] newLine = Collapse(line, out var mergeInfo);
            Array.Reverse(newLine); // 恢复顺序

            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] != newLine[col])
                {
                    moved = true;
                }
                grid[row, col] = newLine[col]; // 更新网格数据
            }

            foreach (var (from, to, value) in mergeInfo)
            {
                animations.Add((row, gridSize - 1 - from, row, gridSize - 1 - to, value));
            }
        }
        return moved;
    }

    // 合并数组中的非零元素，返回合并信息，在合并时增加分数
    private int[] Collapse(int[] line, out List<(int from, int to, int value)> mergeInfo)
    {
        mergeInfo = new List<(int, int, int)>();
        int[] collapsed = new int[gridSize];
        int index = 0;
        bool merged = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == 0) continue;
            if (!merged && index > 0 && collapsed[index - 1] == line[i])
            {
                collapsed[index - 1] *= 2;
                // 在合并时增加分数
                currentScore += collapsed[index - 1];
                mergeInfo.Add((i, index - 1, collapsed[index - 1]));
                merged = true;
            }
            else
            {
                mergeInfo.Add((i, index, line[i]));
                collapsed[index++] = line[i];
                merged = false;
            }
        }
        return collapsed;
    }
    // 在网格中随机生成一个数字（2或4）
    private void SpawnRandomTile()
    {
        int emptyCount = 0; // 统计空格数量
        foreach (int value in grid)
        {
            if (value == 0) emptyCount++;
        }

        if (emptyCount == 0) return; // 如果没有空格，直接返回

        int target = random.Next(emptyCount); // 随机选择一个空格
        int counter = 0;
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] == 0)
                {
                    if (counter == target)
                    {
                        grid[row, col] = random.NextDouble() < probabilityOfTwo ? 2 : 4; // 根据概率生成数字2或4
                        return;
                    }
                    counter++;
                }
            }
        }
    }
    
    
    
    
    // 移动分析结果的数据结构
    public struct MoveAnalysis
    {
        public bool canMove;        // 是否可以移动
        public int mergeCount;      // 可能的合并次数
        public int mergeValue;      // 合并后获得的总值
        public int emptyAfterMove;  // 移动后的空格数
        public int maxTileAfterMove;// 移动后的最大数字
    }

    // 分析特定方向的移动结果
    public MoveAnalysis AnalyzeMove(MoveDirection direction)
    {
        // 创建网格副本进行模拟
        int[,] simulatedGrid = (int[,])grid.Clone();
        int currentScore = 0;
        
        // 模拟移动
        var (moved, mergeInfo) = SimulateMove(simulatedGrid, direction, ref currentScore);

        MoveAnalysis analysis = new MoveAnalysis
        {
            canMove = moved,
            mergeCount = mergeInfo.Count,
            mergeValue = currentScore,
            emptyAfterMove = CountEmptyTiles(simulatedGrid),
            maxTileAfterMove = GetMaxTileFromGrid(simulatedGrid)
        };

        return analysis;
    }

    // 模拟移动而不改变实际游戏状态
    private (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)>) 
        SimulateMove(int[,] simulatedGrid, MoveDirection direction, ref int scoreGain)
    {
        bool moved = false;
        List<(int, int, int, int, int)> mergeInfo = new List<(int, int, int, int, int)>();

        switch (direction)
        {
            case MoveDirection.Up:
                moved = SimulateMoveUp(simulatedGrid, mergeInfo, ref scoreGain);
                break;
            case MoveDirection.Down:
                moved = SimulateMoveDown(simulatedGrid, mergeInfo, ref scoreGain);
                break;
            case MoveDirection.Left:
                moved = SimulateMoveLeft(simulatedGrid, mergeInfo, ref scoreGain);
                break;
            case MoveDirection.Right:
                moved = SimulateMoveRight(simulatedGrid, mergeInfo, ref scoreGain);
                break;
        }

        return (moved, mergeInfo);
    }

    private bool SimulateMoveUp(int[,] grid, List<(int, int, int, int, int)> mergeInfo, ref int scoreGain)
    {
        bool moved = false;
        for (int col = 0; col < gridSize; col++)
        {
            int[] column = new int[gridSize];
            for (int row = 0; row < gridSize; row++)
            {
                column[row] = grid[row, col];
            }

            int[] newColumn = SimulateCollapse(column, out var colMergeInfo, ref scoreGain);
            
            // 更新模拟网格和移动信息
            for (int row = 0; row < gridSize; row++)
            {
                if (grid[row, col] != newColumn[row])
                {
                    moved = true;
                    grid[row, col] = newColumn[row];
                }
            }

            foreach (var (from, to, value) in colMergeInfo)
            {
                mergeInfo.Add((from, col, to, col, value));
            }
        }
        return moved;
    }

    private bool SimulateMoveDown(int[,] grid, List<(int, int, int, int, int)> mergeInfo, ref int scoreGain)
    {
        bool moved = false;
        for (int col = 0; col < gridSize; col++)
        {
            int[] column = new int[gridSize];
            for (int row = 0; row < gridSize; row++)
            {
                column[row] = grid[row, col];
            }

            Array.Reverse(column);
            int[] newColumn = SimulateCollapse(column, out var colMergeInfo, ref scoreGain);
            Array.Reverse(newColumn);

            for (int row = 0; row < gridSize; row++)
            {
                if (grid[row, col] != newColumn[row])
                {
                    moved = true;
                    grid[row, col] = newColumn[row];
                }
            }

            foreach (var (from, to, value) in colMergeInfo)
            {
                mergeInfo.Add((gridSize - 1 - from, col, gridSize - 1 - to, col, value));
            }
        }
        return moved;
    }

    private bool SimulateMoveLeft(int[,] grid, List<(int, int, int, int, int)> mergeInfo, ref int scoreGain)
    {
        bool moved = false;
        for (int row = 0; row < gridSize; row++)
        {
            int[] line = new int[gridSize];
            for (int col = 0; col < gridSize; col++)
            {
                line[col] = grid[row, col];
            }

            int[] newLine = SimulateCollapse(line, out var rowMergeInfo, ref scoreGain);

            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] != newLine[col])
                {
                    moved = true;
                    grid[row, col] = newLine[col];
                }
            }

            foreach (var (from, to, value) in rowMergeInfo)
            {
                mergeInfo.Add((row, from, row, to, value));
            }
        }
        return moved;
    }

    private bool SimulateMoveRight(int[,] grid, List<(int, int, int, int, int)> mergeInfo, ref int scoreGain)
    {
        bool moved = false;
        for (int row = 0; row < gridSize; row++)
        {
            int[] line = new int[gridSize];
            for (int col = 0; col < gridSize; col++)
            {
                line[col] = grid[row, col];
            }

            Array.Reverse(line);
            int[] newLine = SimulateCollapse(line, out var rowMergeInfo, ref scoreGain);
            Array.Reverse(newLine);

            for (int col = 0; col < gridSize; col++)
            {
                if (grid[row, col] != newLine[col])
                {
                    moved = true;
                    grid[row, col] = newLine[col];
                }
            }

            foreach (var (from, to, value) in rowMergeInfo)
            {
                mergeInfo.Add((row, gridSize - 1 - from, row, gridSize - 1 - to, value));
            }
        }
        return moved;
    }

    private int[] SimulateCollapse(int[] line, out List<(int from, int to, int value)> mergeInfo, ref int scoreGain)
    {
        mergeInfo = new List<(int, int, int)>();
        int[] collapsed = new int[gridSize];
        int index = 0;
        bool merged = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == 0) continue;
            if (!merged && index > 0 && collapsed[index - 1] == line[i])
            {
                collapsed[index - 1] *= 2;
                scoreGain += collapsed[index - 1];
                mergeInfo.Add((i, index - 1, collapsed[index - 1]));
                merged = true;
            }
            else
            {
                mergeInfo.Add((i, index, line[i]));
                collapsed[index++] = line[i];
                merged = false;
            }
        }
        return collapsed;
    }

    // 工具方法
    private int CountEmptyTiles(int[,] grid)
    {
        int count = 0;
        foreach (int value in grid)
        {
            if (value == 0) count++;
        }
        return count;
    }

    private int GetMaxTileFromGrid(int[,] grid)
    {
        int max = 0;
        foreach (int value in grid)
        {
            max = Math.Max(max, value);
        }
        return max;
    }
    
    // 获取单调性分析
    public float[] GetMonotonicityAnalysis()
    {
        float[] result = new float[8]; // 4个行 + 4个列
        
        // 分析行
        for (int i = 0; i < gridSize; i++)
        {
            float increasing = 0;
            float decreasing = 0;
            for (int j = 0; j < gridSize - 1; j++)
            {
                if (grid[i, j] != 0 && grid[i, j + 1] != 0)
                {
                    float diff = Mathf.Log(grid[i, j + 1], 2) - Mathf.Log(grid[i, j], 2);
                    increasing += Mathf.Max(0, diff);
                    decreasing += Mathf.Max(0, -diff);
                }
            }
            result[i] = (decreasing - increasing) / 10f;
        }

        // 分析列
        for (int j = 0; j < gridSize; j++)
        {
            float increasing = 0;
            float decreasing = 0;
            for (int i = 0; i < gridSize - 1; i++)
            {
                if (grid[i, j] != 0 && grid[i + 1, j] != 0)
                {
                    float diff = Mathf.Log(grid[i + 1, j], 2) - Mathf.Log(grid[i, j], 2);
                    increasing += Mathf.Max(0, diff);
                    decreasing += Mathf.Max(0, -diff);
                }
            }
            result[j + 4] = (decreasing - increasing) / 10f;
        }
        
        return result;
    }

    // 获取平滑度分析
    public float GetSmoothness()
    {
        float smoothness = 0;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i, j] != 0)
                {
                    float value = Mathf.Log(grid[i, j], 2);
                    if (j < gridSize - 1 && grid[i, j + 1] != 0)
                    {
                        smoothness -= Mathf.Abs(value - Mathf.Log(grid[i, j + 1], 2));
                    }
                    if (i < gridSize - 1 && grid[i + 1, j] != 0)
                    {
                        smoothness -= Mathf.Abs(value - Mathf.Log(grid[i + 1, j], 2));
                    }
                }
            }
        }
        return smoothness / 32f;
    }
}
