using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private BoxItem[] boxItems;
    [SerializeField] private MoveBoxItem moveBoxItem;

    private GameLogic gameLogic;
    private float moveTime = 0.25f;
    private bool canMove = true;
    private void Start()
    {
        gameLogic = new GameLogic(4, 0.9f);
        var data = gameLogic.GetGrid();
        SetGameBox(data);
    }


    private void Update()
    {
        if (!canMove) return;
        if (Input.GetKeyDown(KeyCode.W))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Up);
            if (moved)
            {
                canMove = false;
                StartCoroutine(_enumerator(moveTime));
                ShowAnimations(animations);
                if (gameLogic.IsGameOver())
                {
                    OnGameOver();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Down);
            if (moved)
            {
                canMove = false;
                StartCoroutine(_enumerator(moveTime));
                ShowAnimations(animations);
                if (gameLogic.IsGameOver())
                {
                    OnGameOver();
                }
            }
        }if (Input.GetKeyDown(KeyCode.A))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Left);
            if (moved)
            {
                canMove = false;
                StartCoroutine(_enumerator(moveTime));
                ShowAnimations(animations);
                if (gameLogic.IsGameOver())
                {
                    OnGameOver();
                }
            }
        }if (Input.GetKeyDown(KeyCode.D))
        {
            (bool moved, List<(int fromRow, int fromCol, int toRow, int toCol, int newValue)> animations) = gameLogic.Move(GameLogic.MoveDirection.Right);
            if (moved)
            {
                canMove = false;
                StartCoroutine(_enumerator(moveTime));
                ShowAnimations(animations);
                if (gameLogic.IsGameOver())
                {
                    OnGameOver();
                }
                
            }
        }
    }

    private void OnGameOver()
    {
        Debug.LogWarning("游戏结束");
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
            moveBox.StartAnimation(moveTime,fromBoxItem.transform.position,toBoxItem.transform.position,fromBoxItem.count);
        }
    }
}



