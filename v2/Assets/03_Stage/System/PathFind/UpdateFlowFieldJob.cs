using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Direction = TaeBoMiCache.Direction;

namespace PupSurvivors.Stage.PathFinding
{
    [BurstCompile]
    public struct UpdateFlowFieldJob : IJob
    {
        public int2 RelativePoint;

        public NativeArray<Cell> FlowField;

        private static readonly int2 Left = new(-1, 0);
        private static readonly int2 Right = new(1, 0);
        private static readonly int2 Up = new(0, 1);
        private static readonly int2 Down = new(0, -1);

        private static readonly int2[] DirectionsInt2 =
        {
            Up, Down, Left, Right
        };

        private static readonly Direction[] Directions =
        {
            Direction.Up, Direction.Down, Direction.Left, Direction.Right
        };

        private const int ColSize = PathFinder.ColSize;
        private const int RowSize = PathFinder.RowSize;

        public void Execute()
        {
            // FindingQueue.Clear();
            var findingQueue = new NativeQueue<int2>(Allocator.Temp);
        
            var centerIdx = Flatten(RelativePoint);
            FlowField[centerIdx] = new Cell {Dist =  0};
            findingQueue.Enqueue(RelativePoint);

            while (findingQueue.Count != 0)
            {
                var curPoint = findingQueue.Dequeue();
                var curIdx = Flatten(curPoint);

                var bestDir = Direction.None;
                var collDir = Direction.None;

                for (var i = 0; i < 4; i++)
                {
                    var neighborPoint = curPoint + DirectionsInt2[i];
                    var neighborDir = Directions[i];

                    var neighborIdx = Flatten(neighborPoint);

                    if (IsOutOfBound(neighborPoint))
                    {
                        continue;
                    }
                    switch (FlowField[neighborIdx].Dist)
                    {
                        case Cell.UnExplored:
                            FlowField[neighborIdx] = Cell.ExploredCell;
                            findingQueue.Enqueue(neighborPoint);
                            break;
                        case Cell.Collider:
                            collDir |= neighborDir;
                            break;
                        case >= 0:
                            bestDir |= neighborDir;
                            break;
                    }
                }

                // 2개 이상 콜라이더 인접 시
                if ((collDir & collDir - 1) != 0)
                {
                    bestDir = LeaveOnlyOneDirection(bestDir);
                }


                var curCell = FlowField[curIdx];

                switch (bestDir)
                {
                    case Direction.None:

                        break;
                    case Direction.Up:
                    case Direction.Up | Direction.Down:
                    case Direction.Cardinal:
                        curCell.Dist = FlowField[Flatten(curPoint + Up)].Dist + 1;
                        curCell.Dir = new float2(0, 1);
                        break;
                    case Direction.Down:
                        curCell.Dist = FlowField[Flatten(curPoint + Down)].Dist + 1;
                        curCell.Dir = new float2(0, -1);
                        break;
                    case Direction.Left:
                    case Direction.Left | Direction.Right:
                        curCell.Dist = FlowField[Flatten(curPoint + Left)].Dist + 1;
                        curCell.Dir = new float2(-1, 0);
                        break;
                    case Direction.Right:
                        curCell.Dist = FlowField[Flatten(curPoint + Right)].Dist + 1;
                        curCell.Dir = new float2(1, 0);
                        break;
                    case Direction.UpLeft:
                    case Direction.Up | Direction.Left | Direction.Right:
                        curCell.Dist = FlowField[Flatten(curPoint + Up)].Dist + 1;
                        curCell.Dir = math.lerp(FlowField[Flatten(curPoint + Up)].Dir,
                            FlowField[Flatten(curPoint + Left)].Dir, 0.5f);
                        break;
                    case Direction.UpRight:
                    case Direction.Up | Direction.Right | Direction.Down:
                        curCell.Dist = FlowField[Flatten(curPoint + Up)].Dist + 1;
                        curCell.Dir = math.lerp(FlowField[Flatten(curPoint + Up)].Dir,
                            FlowField[Flatten(curPoint + Right)].Dir, 0.5f);
                        break;
                    case Direction.DownLeft:
                    case Direction.Up | Direction.Left | Direction.Down:
                        curCell.Dist = FlowField[Flatten(curPoint + Down)].Dist + 1;
                        curCell.Dir = math.lerp(FlowField[Flatten(curPoint + Down)].Dir,
                            FlowField[Flatten(curPoint + Left)].Dir, 0.5f);
                        break;
                    case Direction.DownRight:
                    case Direction.Down | Direction.Left | Direction.Right:
                        curCell.Dist = FlowField[Flatten(curPoint + Down)].Dist + 1;
                        curCell.Dir = math.lerp(FlowField[Flatten(curPoint + Down)].Dir,
                            FlowField[Flatten(curPoint + Right)].Dir, 0.5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                FlowField[curIdx] = curCell;
            }

            findingQueue.Dispose();
        }

        private Direction LeaveOnlyOneDirection(Direction oriDir)
        {
            foreach (var dir in Directions)
            {
                if ((oriDir & dir) != 0)
                {
                    return dir;
                }
            }

            return Direction.None;
        }


        private bool IsOutOfBound(int2 point)
        {
            if (point.x is < 0 or >= ColSize)
            {
                return true;
            }

            if (point.y is < 0 or >= RowSize)
            {
                return true;
            }

            return false;
        }

        private static int Flatten(int2 point)
        {
            return point.x + point.y * ColSize;
        }

        private int2 UnFlatten(int idx)
        {
            var y = idx / ColSize;
            var x = idx - y * ColSize;
            return new int2(x, y);
        }
    }
}

public struct Cell
{
    public int Dist;
    public float2 Dir;

    public static readonly Cell UnexploredCell = new() { Dist = UnExplored };
    public static readonly Cell ExploredCell = new() { Dist = Explored };
    public static readonly Cell ColliderCell = new() { Dist = Collider };
    
    public const int UnExplored = -1;
    public const int Explored = -2;
    public const int Collider = -3;
}