using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PathFinder
{
    private Transform _targetTr;
    private Vector2Int _currentTargetPos, _lastTargetPos;

    private PathData _groundPath, _airPath;

    private int _rowCenter, _columnCenter;

    private CancellationTokenSource _cts;


    public PathFinder(int rowSize, int columnSize, Transform target)
    {
        _targetTr = target;
        _groundPath = new PathData(rowSize, columnSize, LayerMask.GetMask("Wall", "Ground"));
        _airPath = new PathData(rowSize, columnSize, LayerMask.GetMask("Wall"));
        _rowCenter = (int)(rowSize * 0.5f);
        _columnCenter = (int)(columnSize * 0.5f);
    }

    public async UniTaskVoid Execute()
    {
        _cts.CancelAndDispose();
        _cts = new CancellationTokenSource();
        while (true)
        {
            await UniTask.Yield(_cts.Token);

            _currentTargetPos = Vector2Int.FloorToInt(_targetTr.position);
            if (_currentTargetPos == _lastTargetPos) // 이전 위치와 동일하면  
            {
                continue;
            }


            _lastTargetPos = _currentTargetPos;
        }
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    public bool CanSpawn(Vector2 pos, bool isGround)
    {
        var diff = Vector2Int.FloorToInt(pos) - _currentTargetPos;
        var pathData = isGround
            ? _groundPath.Cells[diff.y + _rowCenter][diff.x + _columnCenter]
            : _airPath.Cells[diff.y + _rowCenter][diff.x + _columnCenter];

        return pathData.Dist >= PathData.Cell.CenterTile;
    }

    public Vector2 GetDirection(Vector2 pos, bool isGround)
    {
        var diff = Vector2Int.FloorToInt(pos) - _currentTargetPos;

        // 길찾기 시스템 영역 밖일 경우, 플레이어 향한 방향 리턴 
        if (Mathf.Abs(diff.x) > _columnCenter || Mathf.Abs(diff.y) > _rowCenter)
        {
            return ((Vector2)_targetTr.position - pos).normalized;
        }

        var pathData = isGround
            ? _groundPath.Cells[diff.y + _rowCenter][diff.x + _columnCenter]
            : _airPath.Cells[diff.y + _rowCenter][diff.x + _columnCenter];

        return pathData.Dist switch
        {
            > 0 => pathData.MoveDir,
            PathData.Cell.UnexploredTile => Vector2.zero,
            PathData.Cell.ColliderTile => Vector2.zero,
            _ => ((Vector2)_targetTr.position - pos).normalized,
        };
    }


    public class PathData
    {
        public readonly Cell[][] Cells;
        private int _rowSize, _rowCenter;
        private int _columnSize, _columnCenter;

        private Transform _target;
        private readonly int _layerMask;

        /// <summary>
        /// 크기는 홀수 값만 넣도록
        /// </summary>
        public PathData(int rowSize, int columnSize, int layerMask)
        {
            Cells = new Cell[rowSize][];
            for (var row = 0; row < rowSize; row++)
            {
                Cells[row] = new Cell[columnSize];
            }

            _rowSize = rowSize;
            _rowCenter = (int)(rowSize * 0.5f);
            _columnSize = columnSize;
            _columnCenter = (int)(columnSize * 0.5f);

            _layerMask = layerMask;
        }

        public void UpdatePosition(Vector2Int currentPos, Vector2Int lastPos)
        {
            var diff = currentPos - lastPos;
            if (Mathf.Abs(diff.x) >= _columnSize || Mathf.Abs(diff.y) >= _rowSize)
            {
                var centerPos = currentPos + new Vector2(0.5f - _columnCenter, 0.5f - _rowCenter);
                for (var row = 0; row < _rowSize; row++)
                {
                    for (var column = 0; column < _columnSize; column++)
                    {
                        var tileCenter = centerPos + new Vector2(column, row);
                        var result = Physics2D.OverlapCircle(tileCenter, 0.25f, _layerMask);
                        Cells[row][column].Dist = result ? Cell.ColliderTile : Cell.UnexploredTile;
                    }
                }
            }
        }

        public struct Cell
        {
            public int Dist;
            public Direction NearestCellDir;
            public Vector2 MoveDir;

            public const int CenterTile = 0;
            public const int ColliderTile = -1;
            public const int FindingTile = -2;
            public const int UnexploredTile = -3;

            [Flags]
            public enum Direction
            {
                None = 0,
                Up = 1 << 1,
                Down = 1 << 2,
                Left = 1 << 3,
                Right = 1 << 4,
                UpLeft = Up | Left,
                UpRight = Up | Right,
                DownLeft = Down | Left,
                DownRight = Down | Right,
                All = Up | Down | Left | Right,
            }
        }
    }
}