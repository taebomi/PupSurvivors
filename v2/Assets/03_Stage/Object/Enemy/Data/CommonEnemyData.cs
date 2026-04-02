using System;
using UnityEngine;

namespace PupSurvivors.Enemy
{
    public enum EnemyType
    {
        // Common
        Common,
        // Uncommon
        Elite, 
        MiniBoss,
        // Boss
        Boss,
    }
    
    public enum EnemyName
    {
        // 
        Slime = 0_0_000_0_00,
        BlueSlime = 0_0_000_0_01,
        GreenSlime = 0_0_000_0_02,
        PinkSlime = 0_0_000_0_03,

        // MiniBoss
        MiniSlimeCube = 2_0_000_0_00,
    }
    
    public enum MobilityType
    {
        Ground,
        Air,
    }

    [CreateAssetMenu(menuName = "PupSurvivors/Enemy Data", fileName = "EnemyData", order = 0)]
    public class CommonEnemyData : ScriptableObject
    {
        public EnemyName id;
        public AnimatorOverrideController aniController;
        public EnemyStats stats;
        public MobilityType mobilityType;
        public float mass;
        public float collRadius;
        public float spriteYPos;

        public AudioClip[] hitSE;



    }
}