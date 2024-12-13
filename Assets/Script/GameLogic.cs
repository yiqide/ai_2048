// 逻辑部分：GameLogic.cs

using System;
using System.Collections.Generic;

public class GameLogic
{
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

    public GameLogic(int size, double probabilityOfTwo = 0.9)
    {
        gridSize = size;
        grid = new int[gridSize, gridSize];
        random = new Random();
        this.probabilityOfTwo = probabilityOfTwo;
        SpawnRandomTile(); // 初始化时生成两个随机数字
        SpawnRandomTile();
    }

    // 获取当前网格状态
    public int[,] GetGrid() => (int[,])grid.Clone();

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

    // 合并数组中的非零元素，返回合并信息
    private int[] Collapse(int[] line, out List<(int from, int to, int value)> mergeInfo)
    {
        mergeInfo = new List<(int, int, int)>();
        int[] collapsed = new int[gridSize]; // 合并后的数组
        int index = 0; // 用于跟踪写入位置
        bool merged = false; // 标记是否发生了合并

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == 0) continue; // 跳过零元素
            if (!merged && index > 0 && collapsed[index - 1] == line[i])
            {
                collapsed[index - 1] *= 2; // 合并相同元素
                mergeInfo.Add((i, index - 1, collapsed[index - 1]));
                merged = true; // 标记合并
            }
            else
            {
                mergeInfo.Add((i, index, line[i]));
                collapsed[index++] = line[i]; // 写入非零元素
                merged = false; // 重置合并标记
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
}
