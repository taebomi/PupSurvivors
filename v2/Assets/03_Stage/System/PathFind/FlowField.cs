using System;
using System.Collections.Generic;
using PupSurvivors.Enemy;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace PupSurvivors.Stage.PathFinding
{
    public class FlowField : IDisposable
    {
        private readonly Transform _targetTr;
        private readonly CostField _costField;

        private int2 _curWorldPoint, _lastTargetWorldPoint;

        public Vector3 TargetPos => _targetTr.position;
        public NativeArray<Cell> GroundCells { get; }
        public NativeArray<Cell> AirCells { get; }

        private const int RowSize = PathFinder.RowSize;
        private const int ColSize = PathFinder.ColSize;
        private const int RowHalfSize = PathFinder.RowHalfSize;
        private const int ColHalfSize = PathFinder.ColHalfSize;

        private FlowField(CostField costField, Transform playerTr)
        {
            _costField = costField;
            _targetTr = playerTr;

            _lastTargetWorldPoint = PathFinder.GetWorldPoint(_targetTr.position) - 10000;

            GroundCells = new NativeArray<Cell>(RowSize * ColSize, Allocator.Persistent);
            AirCells = new NativeArray<Cell>(RowSize * ColSize, Allocator.Persistent);
        }

        public static List<FlowField> CreateInstances(CostField costField, Player[] players)
        {
            var flowFields = new List<FlowField>(players.Length);
            foreach (var playerTr in players)
            {
                flowFields.Add(new FlowField(costField, playerTr.transform));
            }
            return flowFields;
        }

        public Cell GetCell(Vector3 pos, MobilityType mobilityType)
        {
            var cells = mobilityType is MobilityType.Ground ? GroundCells : AirCells;
            var worldPoint = PathFinder.GetWorldPoint(pos);
            var diff = worldPoint - _curWorldPoint;
            var index = PathFinder.Flatten(diff);
            return cells[index];
        }

        public Vector2 GetDir(Vector3 pos, MobilityType mobilityType)
        {
            return GetCell(pos, mobilityType).Dir;
        }

        public bool UpdatePosition()
        {
            _curWorldPoint = PathFinder.GetWorldPoint(_targetTr.position);
            if (_curWorldPoint.Equals(_lastTargetWorldPoint))
            {
                return false;
            }
            else
            {
                _lastTargetWorldPoint = _curWorldPoint;
                return true;
            }
        }

        public JobHandle UpdateFlowField()
        {
            var diffWorldPoint = _curWorldPoint - _costField.CurWorldPoint;

            var resetGroundFlowFieldJob = new ResetFlowFieldJob
            {
                CostField = _costField.Cells,
                FlowField = GroundCells,
                Option = CostField.GroundColl | CostField.WallColl
            };
            var resetAirFlowFieldJob = new ResetFlowFieldJob
            {
                CostField = _costField.Cells,
                FlowField = AirCells,
                Option = CostField.WallColl
            };
            var resetGroundFlowFieldJobHandle = resetGroundFlowFieldJob.Schedule(RowSize * ColSize, 32);
            var resetAirFlowFieldJobHandle = resetAirFlowFieldJob.Schedule(RowSize * ColSize, 32);

            // 범위 바깥이면 길찾기 초기화 후 종료
            if (PathFinder.IsOutside(diffWorldPoint))
            {
                return JobHandle.CombineDependencies(resetGroundFlowFieldJobHandle, resetAirFlowFieldJobHandle);
            }

            var relativePoint = diffWorldPoint + PathFinder.CenterPoint;

            var updateGroundJob = new UpdateFlowFieldJob
            {
                FlowField = GroundCells,
                RelativePoint = relativePoint,
            };
            var updateAirJob = new UpdateFlowFieldJob
            {
                FlowField = AirCells,
                RelativePoint = relativePoint,
            };

            var updateGroundJobHandle = updateGroundJob.Schedule(resetGroundFlowFieldJobHandle);
            var updateAirJobHandle = updateAirJob.Schedule(resetAirFlowFieldJobHandle);
        
            return JobHandle.CombineDependencies(updateGroundJobHandle, updateAirJobHandle);
        }


        public void Dispose()
        {
            GroundCells.Dispose();
            AirCells.Dispose();
        }
    }
}