using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private BoxItem[] boxItems;


    private GameLogic gameLogic;
    private void Start()
    {
        gameLogic = new GameLogic(4, 0.9f);
        var data = gameLogic.GetGrid();
        SetGameBox(data);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Up);
            if (moved)
            {
                SetGameBox(gameLogic.GetGrid());
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Down);
            if (moved)
            {
                SetGameBox(gameLogic.GetGrid());
            }
        }if (Input.GetKeyDown(KeyCode.A))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Left);
            if (moved)
            {
                SetGameBox(gameLogic.GetGrid());
            }
        }if (Input.GetKeyDown(KeyCode.D))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Right);
            if (moved)
            {
                SetGameBox(gameLogic.GetGrid());
            }
        }
    }

    /// <summary>
    /// 更具 grid 设置boxItems
    /// </summary>
    /// <param name="grid"></param>
    public void SetGameBox(int[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                var boxItem = boxItems[i * grid.GetLength(1) + j];
                boxItem.SetData(grid[i, j]);
            }
        }
    }
}



