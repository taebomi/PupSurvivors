using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TempPathFinder : Singleton<TempPathFinder>
{
    private Transform _playerTr;
    private Vector2Int _currentPlayerPos;
    private Vector2Int _lastPlayerPos;

    private PathData[][] _pathData;
    private PathData _lastCenterPathData;
    private Cell _upCell, _rightCell, _downCell, _leftCell;
    private Queue<Vector2Int> _findingQueue;

    // PathFinding 가로 세로 범위, column size, row size 변경해 사이즈 변경 가능
    public const int ColumnSize = 45;
    public const int RowSize = 35;
    private const int ColumnCenter = (int)(ColumnSize * 0.5f);
    private const int RowCenter = (int)(RowSize * 0.5f);

    // 타일의 특수 용도 distance 값
    private const int NoneTile = -5;
    private const int FindingTile = -4;
    private const int ColliderTile = -1;
    private const int CenterTile = 0;

    private int _groundCheckLayerMask;
    private int _airCheckLayerMask;

#if UNITY_EDITOR

    #region 디버그 용도

    [Header("디버그")] [SerializeField] private bool debugMode = true;
    [SerializeField] private bool groundDebug, airDebug;

    [SerializeField] private Tilemap groundDebugTilemap, airDebugTilemap;

    [SerializeField] private TileBase groundCenterTileBase,
        groundDirectionTileBase,
        groundCollisionTileBase,
        groundFindingTileBase,
        airCenterTileBase,
        airDirectionTileBase,
        airCollisionTileBase,
        airFindingTileBase;

    private Matrix4x4 _identityMatrix; // 캐싱

    #endregion

#endif

    protected override void AwakeAfter()
    {
        _pathData = new PathData[RowSize][];
        for (var row = 0; row < RowSize; row++)
        {
            _pathData[row] = new PathData[ColumnSize];
        }

        _playerTr = PlayerController.Instance.transform;
        _findingQueue = new Queue<Vector2Int>(100);
        _groundCheckLayerMask = LayerMask.GetMask("Wall", "Ground");
        _airCheckLayerMask = LayerMask.GetMask("Wall");

        _currentPlayerPos = Vector2Int.FloorToInt(_playerTr.position);
        _lastPlayerPos = Vector2Int.FloorToInt(_playerTr.position) - new Vector2Int(100, 100);


#if UNITY_EDITOR
        _identityMatrix = Matrix4x4.identity;
#endif
    }

    private void Update()
    {
        _currentPlayerPos = Vector2Int.FloorToInt(_playerTr.position);
        if (_currentPlayerPos == _lastPlayerPos) return; // 이전 위치랑 동일할 경우 스킵

        RecalculatePathData();
        PathFind();

        _lastPlayerPos = _currentPlayerPos;

#if UNITY_EDITOR
        if (debugMode) DrawDebugTile();
#endif
    }

    /// <summary>
    /// 해당 위치에 유닛 생성이 가능한지
    /// </summary>
    public bool CanSpawn(Vector2 pos, bool isGround)
    {
        var posInt = Vector2Int.FloorToInt(pos);
        var pathData =
            _pathData[posInt.y - _currentPlayerPos.y + RowCenter][posInt.x - _currentPlayerPos.x + ColumnCenter];
        return isGround
            ? pathData.GroundPath.Distance < CenterTile
            : pathData.AirPath.Distance < CenterTile;
    }

    public Vector2 GetDirection(Vector2 pos, bool isGround)
    {
        var diff = Vector2Int.FloorToInt(pos) - _currentPlayerPos;
        if (diff.x is > ColumnCenter or < -ColumnCenter || diff.y is > RowCenter or < -RowCenter)
        {
            return ((Vector2)_playerTr.position - pos).normalized;
        }

        var pathData = _pathData[diff.y + RowCenter][diff.x + ColumnCenter];
        var currentPathData = isGround ? pathData.GroundPath : pathData.AirPath;

        return currentPathData.Distance switch
        {
            > 0 => currentPathData.MoveDirection,
            NoneTile => Vector2.zero,
            ColliderTile => Vector2.zero,
            _ => ((Vector2)_playerTr.position - pos).normalized
        };
    }


    private void PathFind()
    {
        _findingQueue.Clear();


        _lastCenterPathData = _pathData[RowCenter][ColumnCenter];
        _pathData[RowCenter][ColumnCenter].AirPath.Distance = 0;
        _pathData[RowCenter][ColumnCenter].GroundPath.Distance = 0;

        _findingQueue.Enqueue(new Vector2Int(ColumnCenter, RowCenter));
        while (_findingQueue.Count > 0)
        {
            var tile = _findingQueue.Dequeue();
            PathFind(tile.y, tile.x);
        }

        _findingQueue.Enqueue(new Vector2Int(ColumnCenter, RowCenter));
        while (_findingQueue.Count > 0)
        {
            var tile = _findingQueue.Dequeue();
            AirPathFind(tile.y, tile.x);
        }
    }

    private void PathFind(int row, int column)
    {
        var direction = Direction.None;
        var minDistance = int.MaxValue;

        if (row != RowSize - 1) // 위 공간 있음
        {
            _upCell = _pathData[row + 1][column].GroundPath;
            switch (_upCell.Distance)
            {
                case NoneTile:
                    _pathData[row + 1][column].GroundPath.Distance = FindingTile;

                    _findingQueue.Enqueue(new Vector2Int(column, row + 1));
                    break;
                case >= 0:
                    minDistance = _upCell.Distance;
                    direction = Direction.Up;
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (row != 0) // 아래 공간 있음
        {
            _downCell = _pathData[row - 1][column].GroundPath;
            switch (_downCell.Distance)
            {
                case NoneTile:
                    _pathData[row - 1][column].GroundPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column, row - 1));
                    break;
                case >= 0:
                    minDistance = _downCell.Distance;
                    direction = Direction.Down;
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (column != 0) // can left side
        {
            _leftCell = _pathData[row][column - 1].GroundPath;
            switch (_leftCell.Distance)
            {
                case >= 0 when _leftCell.Distance == minDistance:
                    direction |= Direction.Left;
                    break;
                case >= 0:
                    minDistance = _leftCell.Distance;
                    direction = Direction.Left;
                    break;
                case NoneTile:
                    _pathData[row][column - 1].GroundPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column - 1, row));
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (column != ColumnSize - 1) // can right side
        {
            _rightCell = _pathData[row][column + 1].GroundPath;
            switch (_rightCell.Distance)
            {
                case >= 0 when _rightCell.Distance == minDistance:
                    direction |= Direction.Right;
                    direction &= ~Direction.Left;
                    break;
                case >= 0:
                    minDistance = _rightCell.Distance;
                    direction = Direction.Right;
                    break;
                case NoneTile:
                    _pathData[row][column + 1].GroundPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column + 1, row));
                    break;
                case ColliderTile:
                    break;
            }
        }

        ref var pathData = ref _pathData[row][column].GroundPath;
        switch (direction)
        {
            case Direction.None:
                break;
            case Direction.Up:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.up;
                pathData.BestCellDirection = Direction.Up;
                break;
            case Direction.Down:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.down;
                pathData.BestCellDirection = Direction.Down;
                break;
            case Direction.Left:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.left;
                pathData.BestCellDirection = Direction.Left;
                break;
            case Direction.Right:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.right;
                pathData.BestCellDirection = Direction.Right;
                break;
            case Direction.UpLeft:
                pathData.Distance = minDistance + 1;
                var upLeftNearest = _upCell.BestCellDirection | _leftCell.BestCellDirection;
                if ((upLeftNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.up;
                }
                else if (_pathData[row + 1][column - 1].GroundPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.up;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_upCell.MoveDirection, _leftCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.UpLeft;
                break;
            case Direction.UpRight:
                pathData.Distance = minDistance + 1;
                var upRightNearest = _upCell.BestCellDirection | _rightCell.BestCellDirection;
                if ((upRightNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.right;
                }
                else if (_pathData[row + 1][column + 1].GroundPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.right;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_upCell.MoveDirection, _rightCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.UpRight;
                break;
            case Direction.DownLeft:
                pathData.Distance = minDistance + 1;
                var downLeftNearest = _downCell.BestCellDirection | _leftCell.BestCellDirection;
                if ((downLeftNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.left;
                }
                else if (_pathData[row - 1][column - 1].GroundPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.left;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_downCell.MoveDirection, _leftCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.DownLeft;
                break;

            case Direction.DownRight:
                pathData.Distance = minDistance + 1;
                var rightDownNearest = _downCell.BestCellDirection | _rightCell.BestCellDirection;
                if ((rightDownNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.down;
                }
                else if (_pathData[row - 1][column + 1].GroundPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.down;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_downCell.MoveDirection, _rightCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.DownRight;
                break;
        }
    }

    private void AirPathFind(int row, int column)
    {
        var direction = Direction.None;
        var minDistance = int.MaxValue;

        if (row != RowSize - 1) // 위 공간 있음
        {
            _upCell = _pathData[row + 1][column].AirPath;
            switch (_upCell.Distance)
            {
                case NoneTile:
                    _pathData[row + 1][column].AirPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column, row + 1));
                    break;
                case >= 0:
                    minDistance = _upCell.Distance;
                    direction = Direction.Up;
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (row != 0) // 아래 공간 있음
        {
            _downCell = _pathData[row - 1][column].AirPath;
            switch (_downCell.Distance)
            {
                case NoneTile:
                    _pathData[row - 1][column].AirPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column, row - 1));
                    break;
                case >= 0:
                    minDistance = _downCell.Distance;
                    direction = Direction.Down;
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (column != 0) // can left side
        {
            _leftCell = _pathData[row][column - 1].AirPath;
            switch (_leftCell.Distance)
            {
                case >= 0 when _leftCell.Distance == minDistance:
                    direction |= Direction.Left;
                    break;
                case >= 0:
                    minDistance = _leftCell.Distance;
                    direction = Direction.Left;
                    break;
                case NoneTile:
                    _pathData[row][column - 1].AirPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column - 1, row));
                    break;
                case ColliderTile:
                    break;
            }
        }

        if (column != ColumnSize - 1) // can right side
        {
            _rightCell = _pathData[row][column + 1].AirPath;
            switch (_rightCell.Distance)
            {
                case >= 0 when _rightCell.Distance == minDistance:
                    direction |= Direction.Right;
                    direction &= ~Direction.Left;
                    break;
                case >= 0:
                    minDistance = _rightCell.Distance;
                    direction = Direction.Right;
                    break;
                case NoneTile:
                    _pathData[row][column + 1].AirPath.Distance = FindingTile;
                    _findingQueue.Enqueue(new Vector2Int(column + 1, row));
                    break;
                case ColliderTile:
                    break;
            }
        }

        ref var pathData = ref _pathData[row][column].AirPath;
        switch (direction)
        {
            case Direction.None:
                break;
            case Direction.Up:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.up;
                pathData.BestCellDirection = Direction.Up;
                break;
            case Direction.Down:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.down;
                pathData.BestCellDirection = Direction.Down;
                break;
            case Direction.Left:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.left;
                pathData.BestCellDirection = Direction.Left;
                break;
            case Direction.Right:
                pathData.Distance = minDistance + 1;
                pathData.MoveDirection = Vector2.right;
                pathData.BestCellDirection = Direction.Right;
                break;
            case Direction.UpLeft:
                pathData.Distance = minDistance + 1;
                var upLeftNearest = _upCell.BestCellDirection | _leftCell.BestCellDirection;
                if ((upLeftNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.up;
                }
                else if (_pathData[row + 1][column - 1].AirPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.up;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_upCell.MoveDirection, _leftCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.UpLeft;

                break;
            case Direction.UpRight:
                pathData.Distance = minDistance + 1;
                var upRightNearest = _upCell.BestCellDirection | _rightCell.BestCellDirection;

                if ((upRightNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.right;
                }
                else if (_pathData[row + 1][column + 1].AirPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.right;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_upCell.MoveDirection, _rightCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.UpRight;
                break;
            case Direction.DownLeft:
                pathData.Distance = minDistance + 1;
                var downLeftNearest = _downCell.BestCellDirection | _leftCell.BestCellDirection;
                if ((downLeftNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.left;
                }
                else if (_pathData[row - 1][column - 1].AirPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.left;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_downCell.MoveDirection, _leftCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.DownLeft;

                break;

            case Direction.DownRight:
                pathData.Distance = minDistance + 1;
                var rightDownNearest = _downCell.BestCellDirection | _rightCell.BestCellDirection;
                if ((rightDownNearest | direction) == Direction.All)
                {
                    pathData.MoveDirection = Vector2.down;
                }
                else if (_pathData[row - 1][column + 1].AirPath.Distance == ColliderTile)
                {
                    pathData.MoveDirection = Vector2.down;
                }
                else
                {
                    pathData.MoveDirection =
                        Vector2.LerpUnclamped(_downCell.MoveDirection, _rightCell.MoveDirection, 0.5f);
                }

                pathData.BestCellDirection = Direction.DownRight;
                break;
        }
    }

    #region PathData 초기화 / 갱신 관련

    private void ResetPathData()
    {
        for (var row = 0; row < RowSize; row++)
        {
            for (var column = 0; column < ColumnSize; column++)
            {
                CheckTile(row, column);
            }
        }
    }

    private void CheckTile(int row, int column)
    {
        // 콜라이더 체크
        var point = new Vector2(_currentPlayerPos.x + column - ColumnCenter + 0.5f,
            _currentPlayerPos.y + row - RowCenter + 0.5f);
        var groundResult = Physics2D.OverlapCircle(point, 0.25f, _groundCheckLayerMask);
        var airResult = Physics2D.OverlapPoint(point, _airCheckLayerMask);

        // 콜라이더 여부에 따라 초기화
        ref var pathData = ref _pathData[row][column];
        pathData.GroundPath.Distance = groundResult ? ColliderTile : NoneTile;
        pathData.AirPath.Distance = airResult ? ColliderTile : NoneTile;
    }

    private void MoveTile(int row, int column, Vector2Int diff)
    {
        ref var pathData = ref _pathData[row][column];
        var oriPathData = _pathData[row + diff.y][column + diff.x];
        pathData.GroundPath.Distance = oriPathData.GroundPath.Distance == ColliderTile ? ColliderTile : NoneTile;
        pathData.AirPath.Distance = oriPathData.AirPath.Distance == ColliderTile ? ColliderTile : NoneTile;
    }

    private void RecalculatePathData()
    {
        var diff = _currentPlayerPos - _lastPlayerPos;
        if (Mathf.Abs(diff.x) >= ColumnSize || Mathf.Abs(diff.y) >= RowSize)
        {
            ResetPathData();
            return;
        }

        _pathData[RowCenter][ColumnCenter] = _lastCenterPathData;

        // 기존에 있던 콜라이더 정보들 옮겨주고 ( 땡겨옴 )
        // 새로 알아야 될 구역을 체크
        switch (diff)
        {
            case { x: >= 0, y: >= 0 }:
                for (var row = 0; row < RowSize - diff.y; row++)
                {
                    for (var column = 0; column < ColumnSize - diff.x; column++)
                    {
                        MoveTile(row, column, diff);
                    }
                }

                for (var row = 0; row < RowSize - diff.y; row++)
                {
                    for (var column = ColumnSize - diff.x; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                for (var row = RowSize - diff.y; row < RowSize; row++)
                {
                    for (var column = 0; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                break;
            case { x: >= 0, y: <= 0 }:
                for (var row = RowSize - 1; row >= -diff.y; row--)
                {
                    for (var column = 0; column < ColumnSize - diff.x; column++)
                    {
                        MoveTile(row, column, diff);
                    }
                }

                for (var row = -diff.y; row < RowSize; row++)
                {
                    for (var column = ColumnSize - diff.x; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                for (var row = -diff.y - 1; row >= 0; row--)
                {
                    for (var column = 0; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                break;
            case { x: <= 0, y: >= 0 }:
                for (var row = 0; row < RowSize - diff.y; row++)
                {
                    for (var column = ColumnSize - 1; column >= -diff.x; column--)
                    {
                        MoveTile(row, column, diff);
                    }
                }

                for (var row = 0; row < RowSize - diff.y; row++)
                {
                    for (var column = -diff.x - 1; column >= 0; column--)
                    {
                        CheckTile(row, column);
                    }
                }

                for (var row = RowSize - diff.y; row < RowSize; row++)
                {
                    for (var column = 0; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                break;
            case { x: <= 0, y: <= 0 }:
                for (var row = RowSize - 1; row >= -diff.y; row--)
                {
                    for (var column = ColumnSize - 1; column >= -diff.x; column--)
                    {
                        MoveTile(row, column, diff);
                    }
                }

                for (var row = -diff.y; row < RowSize; row++)
                {
                    for (var column = -diff.x - 1; column >= 0; column--)
                    {
                        CheckTile(row, column);
                    }
                }

                for (var row = -diff.y - 1; row >= 0; row--)
                {
                    for (var column = 0; column < ColumnSize; column++)
                    {
                        CheckTile(row, column);
                    }
                }

                break;
        }
    }

    #endregion

#if UNITY_EDITOR

    #region 디버그 관련

    private void DrawDebugTile()
    {
        if (groundDebug)
            groundDebugTilemap.ClearAllTiles();
        if (airDebug)
            airDebugTilemap.ClearAllTiles();
        for (var row = 0; row < RowSize; row++)
        {
            for (var column = 0; column < ColumnSize; column++)
            {
                var pos = _currentPlayerPos - new Vector2Int(ColumnCenter - column, RowCenter - row);
                var pathData = _pathData[row][column];
                if (groundDebug)
                {
                    SetDebugTile(pos, pathData.GroundPath, true);
                }

                if (airDebug)
                {
                    SetDebugTile(pos, pathData.AirPath, false);
                }
            }
        }
    }

    private void SetDebugTile(Vector2Int pos, Cell cell, bool isGround)
    {
        var vector3IntPos = new Vector3Int(pos.x, pos.y);
        switch (cell.Distance)
        {
            case > 0:
            {
                if (isGround)
                {
                    groundDebugTilemap.SetTile(vector3IntPos, groundDirectionTileBase);
                    var quaternion =
                        Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, cell.MoveDirection));
                    groundDebugTilemap.SetTransformMatrix(vector3IntPos,
                        Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one));
                }
                else
                {
                    airDebugTilemap.SetTile(vector3IntPos, airDirectionTileBase);
                    var quaternion =
                        Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, cell.MoveDirection));
                    airDebugTilemap.SetTransformMatrix(vector3IntPos,
                        Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one));
                }

                return;
            }
            case ColliderTile:
                if (isGround)
                {
                    groundDebugTilemap.SetTile(vector3IntPos, groundCollisionTileBase);
                    groundDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }
                else
                {
                    airDebugTilemap.SetTile(vector3IntPos, airCollisionTileBase);
                    airDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }

                break;
            case FindingTile:
                if (isGround)
                {
                    groundDebugTilemap.SetTile(vector3IntPos, groundFindingTileBase);
                    groundDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }
                else
                {
                    airDebugTilemap.SetTile(vector3IntPos, airFindingTileBase);
                    airDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }

                break;

            default:
                if (isGround)
                {
                    groundDebugTilemap.SetTile(vector3IntPos, groundCenterTileBase);
                    groundDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }
                else
                {
                    airDebugTilemap.SetTile(vector3IntPos, airCenterTileBase);
                    airDebugTilemap.SetTransformMatrix(vector3IntPos, _identityMatrix);
                }

                break;
        }
    }

    #endregion

#endif

    private struct PathData
    {
        public Cell GroundPath;
        public Cell AirPath;
    }

    private struct Cell
    {
        public int Distance;
        public Direction BestCellDirection;
        public Vector2 MoveDirection;
    }

    [Flags]
    private enum Direction
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