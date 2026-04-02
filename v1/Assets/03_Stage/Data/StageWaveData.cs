using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Enemy;
using UnityEngine;

namespace PupSurvivors.Stage
{
    [CreateAssetMenu(menuName = "TaeBoMi/StageWaveData", fileName = "StageWaveData", order = 90)]
    public class StageWaveData : ScriptableObject
    {
        public WaveData[] waveData;
    }

    [Serializable]
    public class WaveData
    {
        public float time;
        public WaveType waveType;
        public WaveEnemyData[] enemyDataArr;
    }

    public enum WaveType
    {
        OutsidePeriodic,
        OutsideOnce,
    }

    [Serializable]
    public class WaveEnemyData
    {
        public EnemyName enemyName;
        public EnemyMovementType movementType;
        public int num;
    }
}