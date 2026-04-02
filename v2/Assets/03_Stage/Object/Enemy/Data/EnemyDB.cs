using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using PupSurvivors.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    [CreateAssetMenu(menuName = "PupSurvivors/Enemy DB", fileName = "EnemyDB", order = 500)]
    public class EnemyDB : ScriptableObject
    {
        public Material normalMaterial, outlineMaterial;
        [FormerlySerializedAs("dict")]
        [Header("Common Enemy")]
        [SerializedDictionary("Enemy Name", "Enemy Data")]
        public SerializedDictionary<EnemyName, CommonEnemyData> commonDict;

        [Header("Uncommon")] public SerializedDictionary<EnemyName, UncommonEnemyData> uncommonDict;

    }
}