using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Enemy
{
    [Serializable]
    public record EnemyData
    {
        public EnemyName name;
        public AnimatorOverrideController aniController;
        public EnemyStats stats;
    }

    public enum EnemyType
    {
        Ground,
        Air,
    }

    [Serializable]
    public record EnemyStats
    {
        public EnemyType type;
        public int hp;
        public int power;
        public float speed;
        public int exp;
        
        public float mass;
        public float offsetY;
        public float radius;
    }

    public enum EnemyName
    {
        GreenSlime = 100000,
        FastGreenSlime = 100001,
        BlueSlime = 100010,
        FastBlueSlime = 100011,
        NormalGoblin = 101000,
        FastGoblin = 101001,
        WoodenShieldGoblin = 101010,
        
        Bat = 105000,
        Skeleton = 106000,
        
        Centaur = 110000,
    }


    public enum EnemyMovementType
    {
        ChasePlayer,
        OneDirection,
    }
}