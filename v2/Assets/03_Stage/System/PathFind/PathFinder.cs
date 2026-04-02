using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PupSurvivors.Stage.PathFinding
{
    public class PathFinder : Singleton<PathFinder>
    {
        public CostField CostField { get; private set; }
        public List<FlowField> FlowFields { get; private set; }
        public int2 CenterWorldPoint => CostField.CurWorldPoint;

        private CancellationTokenSource _pathFindCts;
        private const float CameraHalfHeight = CameraManager.HalfHeight;
        private const float CameraHalfWidth = CameraManager.HalfWidth;

        public const int OutSize = 10;
        public const int RowSize = ((int)CameraHalfHeight * 2 + OutSize * 2) * 2 - 1;
        public const int ColSize = ((int)CameraHalfWidth * 2 + OutSize * 2) * 2 - 1;
        public const int RowHalfSize = RowSize / 2;
        public const int ColHalfSize = ColSize / 2;

        public static readonly int2 CenterPoint = new(ColHalfSize, RowHalfSize);

        public void Initialize(Camera mainCam, Player[] players)
        {
            CostField = new CostField(mainCam.transform);
            FlowFields = FlowField.CreateInstances(CostField, players);
        }

        private void OnDestroy()
        {
            StopPathFind();
            CostField.Dispose();
            foreach (var flowField in FlowFields)
            {
                flowField.Dispose();
            }
        }

        public async UniTaskVoid StartPathFind()
        {
            _pathFindCts?.CancelAndDispose();
            _pathFindCts = new CancellationTokenSource();


            while (_pathFindCts.IsCancellationRequested is false)
            {
                var updatingFlowFieldsJobHandle = new NativeArray<JobHandle>(FlowFields.Count, Allocator.Temp);
                var isCostFieldUpdated = CostField.UpdateCostField();

                for (var idx = 0; idx < FlowFields.Count; idx++)
                {
                    var flowField = FlowFields[idx];
                    var isPosUpdated = flowField.UpdatePosition();
                    if (isCostFieldUpdated || isPosUpdated)
                    {
                        var jobHandle = flowField.UpdateFlowField();
                        updatingFlowFieldsJobHandle[idx] = jobHandle;
                    }
                }

                var combinedJobHandle = JobHandle.CombineDependencies(updatingFlowFieldsJobHandle);
                combinedJobHandle.Complete();
                updatingFlowFieldsJobHandle.Dispose();
                await UniTask.Yield(_pathFindCts.Token);
            }
        }


        public void StopPathFind() => _pathFindCts?.Cancel();

        public static int2 GetWorldPoint(Vector3 pos)
        {
            return new int2((int)Math.Floor(pos.x * 2), (int)Math.Floor(pos.y * 2));
        }

        public static bool IsOutside(int2 diff)
        {
            return math.abs(diff.x) >= ColHalfSize || math.abs(diff.y) >= RowHalfSize;
        }

        public static int Flatten(int2 point)
        {
            return point.x + point.y * ColSize;
        }

        public FlowField GetNearestFlowField(Vector3 pos)
        {
            var minDist = int.MaxValue;
            FlowField nearestFlowField = null;
            foreach (var flowField in FlowFields)
            {
                var dir = flowField.TargetPos - pos;
                var dist = math.abs((int)dir.x) + math.abs((int)dir.y);
                if (minDist <= dist)
                {
                    continue;
                }

                nearestFlowField = flowField;
                minDist = dist;
            }

            return nearestFlowField;
        }

        public bool CanSpawn(Vector3 pos, MobilityType mobilityType)
        {
            foreach (var flowField in FlowFields)
            {
                var cell = flowField.GetCell(pos, mobilityType);
                if (cell.Dist >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}