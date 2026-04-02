using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Enemy
{
    [CreateAssetMenu(menuName = "PupSurvivors/Enemy/Uncommon Data", fileName = "UncommonEnemyData", order = 100)]
    public class UncommonEnemyData : ScriptableObject
    {
        public EnemyType type;
        
        public UncommonEnemyBase prefab;
        public AnimatorOverrideController aniController; // 외형 바꾸는 용도
        public MobilityType mobilityType;
        public Vector3 floatingDamagePos;
        public Vector3 spriteCenterPos;

        [Header("Stats")] public UncommonEnemyStats stats;
    }
}
