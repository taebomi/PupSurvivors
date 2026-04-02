using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace PupSurvivors.Stage.PathFinding
{
    public class CostField : IDisposable
    {
        private NativeArray<byte> _cells;
        public NativeArray<byte> Cells => _cells;

        public Transform TargetTr { get; }
        private int2 _curWorldPoint, _lastWorldPoint;

        public int2 CurWorldPoint => _curWorldPoint;

        private readonly int _groundLayerMask, _airLayerMask;

        public const byte NoColl = 0;
        public const byte GroundColl = 1 << 0;
        public const byte WallColl = 1 << 1;

        private const int RowSize = PathFinder.RowSize;
        private const int ColSize = PathFinder.ColSize;
        private const int RowHalfSize = PathFinder.RowHalfSize;
        private const int ColHalfSize = PathFinder.ColHalfSize;

        public CostField(Transform camTr)
        {
            TargetTr = camTr;

            _cells = new NativeArray<byte>(RowSize * ColSize, Allocator.Persistent);

            _groundLayerMask = TaeBoMiCache.GetLayerMask(TaeBoMiCache.LayerName.Ground) |
                               TaeBoMiCache.GetLayerMask(TaeBoMiCache.LayerName.Wall);
            _airLayerMask = TaeBoMiCache.GetLayerMask(TaeBoMiCache.LayerName.Wall);

            _lastWorldPoint = PathFinder.GetWorldPoint(TargetTr.position) - 10000;
        }

        public void Dispose()
        {
            _cells.Dispose();
        }

        public bool UpdateCostField()
        {
            _curWorldPoint = PathFinder.GetWorldPoint(TargetTr.position);
            if (_curWorldPoint.Equals(_lastWorldPoint))
            {
                return false;
            }

            var diff = _curWorldPoint - _lastWorldPoint;
            if (math.abs(diff.x) >= ColSize || math.abs(diff.y) >= RowSize)
            {
                CheckAll();
            }
            else
            {
                MoveAndCheck(diff);
            }

            _lastWorldPoint = _curWorldPoint;
            return true;
        }

        private void CheckAll()
        {
            for (var row = 0; row < RowSize; row++)
            {
                for (var col = 0; col < ColSize; col++)
                {
                    Check(new int2(col, row));
                }
            }
        }

        private void MoveAndCheck(int2 diff)
        {
            switch (diff)
            {
                case { x: >= 0, y: >= 0 }:
                    for (var row = 0; row < RowSize - diff.y; row++)
                    {
                        for (var col = 0; col < ColSize - diff.x; col++)
                        {
                            Move(new int2(col, row), diff);
                        }
                    }

                    for (var row = 0; row < RowSize - diff.y; row++)
                    {
                        for (var col = ColSize - diff.x; col < ColSize; col++)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    for (var row = RowSize - diff.y; row < RowSize; row++)
                    {
                        for (var col = 0; col < ColSize; col++)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    break;
                case { x: >= 0, y: <= 0 }:
                    for (var row = RowSize - 1; row >= -diff.y; row--)
                    {
                        for (var col = 0; col < ColSize - diff.x; col++)
                        {
                            Move(new int2(col, row), diff);
                        }
                    }

                    for (var row = -diff.y; row < RowSize; row++)
                    {
                        for (var col = ColSize - diff.x; col < ColSize; col++)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    for (var row = -diff.y - 1; row >= 0; row--)
                    {
                        for (var col = 0; col < ColSize; col++)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    break;
                case { x: <= 0, y: >= 0 }:
                    for (var row = 0; row < RowSize - diff.y; row++)
                    {
                        for (var col = ColSize - 1; col >= -diff.x; col--)
                        {
                            Move(new int2(col, row), diff);
                        }
                    }

                    for (var row = 0; row < RowSize - diff.y; row++)
                    {
                        for (var col = -diff.x - 1; col >= 0; col--)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    for (var row = RowSize - diff.y; row < RowSize; row++)
                    {
                        for (var col = 0; col < ColSize; col++)
                        {
                            Check(new int2(col, row));
                        }
                    }

                    break;
                case { x: <= 0, y: <= 0 }:
                    for (var row = RowSize - 1; row >= -diff.y; row--)
                    {
                        for (var col = ColSize - 1; col >= -diff.x; col--)
                        {
                            Move(new int2(col,row), diff);
                        }
                    }

                    for (var row = -diff.y; row < RowSize; row++)
                    {
                        for (var col = -diff.x - 1; col >= 0; col--)
                        {
                            Check(new int2(col,row));
                        }
                    }

                    for (var row = -diff.y - 1; row >= 0; row--)
                    {
                        for (var col = 0; col < ColSize; col++)
                        {
                            Check(new int2(col,row));
                        }
                    }

                    break;
            }
        }

        private void Check(int2 point)
        {
            var tileCenterPos = new Vector2(
                (_curWorldPoint.x + point.x - ColHalfSize) * 0.5f + 0.25f,
                (_curWorldPoint.y + point.y - RowHalfSize) * 0.5f + 0.25f
            );

            var idx = Flatten(point);
            if (Physics2D.OverlapCircle(tileCenterPos, 0.1f, _airLayerMask))
            {
                _cells[idx] = WallColl;
            }
            else if (Physics2D.OverlapCircle(tileCenterPos, 0.1f, _groundLayerMask))
            {
                _cells[idx] = GroundColl;
            }
            else
            {
                _cells[idx] = NoColl;
            }
        }

        private void Move(int2 ori, int2 diff) => _cells[Flatten(ori)] = _cells[Flatten(ori + diff)];
        
        private static int Flatten(int2 point)
        {
            return point.x + point.y * ColSize;
        }

        private static int2 UnFlatten(int idx)
        {
            var x = idx / ColSize;
            var y = idx - x * ColSize;
            return new int2(x, y);
        }
    }
}