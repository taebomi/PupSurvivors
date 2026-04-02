using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PupSurvivors.Stage.PathFinding
{
    [BurstCompile]
    public struct ResetFlowFieldJob : IJobParallelFor
    {
        public byte Option;
        [ReadOnly] public NativeArray<byte> CostField;
        [WriteOnly] public NativeArray<Cell> FlowField;
    
        public void Execute(int index)
        {
            if ((CostField[index]&Option) != 0)
            {
                FlowField[index] = Cell.ColliderCell;
            }
            else
            {
                FlowField[index] = Cell.UnexploredCell;
            }
        }
    }
}