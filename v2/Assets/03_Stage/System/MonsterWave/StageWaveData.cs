using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    [CreateAssetMenu(menuName = "PupSurvivors/Stage Wave Data", fileName = "StageWaveData", order = 0)]
    public class StageWaveData : ScriptableObject
    {
        public List<CommonWaveData> commonWaveData;
        [FormerlySerializedAs("eliteWaveData")] public List<UncommonWaveData> uncommonWaveData;
        public List<BossWaveData> bossWaveData;
    }

    [Serializable]
    public abstract class WaveData
    {
        public float time;
        
    }

    [Serializable]
    public class CommonWaveData : WaveData
    {
        public SpawnOption option;
        public Data[] data;

        [Serializable]
        public class Data
        {
            public CommonEnemyData commonEnemyData;
            public int num;
        }
        
        public enum SpawnOption
        {
            StartPeriodic,
            StopPeriodic,
            Once,
        }
    }


    [Serializable]
    public class UncommonWaveData : WaveData
    {
        public UncommonEnemyData enemyData;
    }

    [Serializable]
    public class BossWaveData : WaveData
    {
    }

    public enum WaveOption
    {
        StartPeriodic = 100,
        StopPeriodic = 101,
        Once = 200,
    }

    [Serializable]
    public class WaveEnemyData
    {
        public EnemyName id;
        public EnemyMoveDirType moveDirType;
        public int num;
    }

    public enum EnemyMoveDirType
    {
        ChasePlayer,
        ToPlayerOnce,
    }
}