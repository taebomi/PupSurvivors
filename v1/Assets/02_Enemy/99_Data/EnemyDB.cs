using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


namespace PupSurvivors.Enemy
{
    [CreateAssetMenu(menuName = "TaeBoMi/EnemyDatabase", fileName = "EnemyDB", order = 99)]
    public class EnemyDB : ScriptableObject
    {
        public CommonEnemy commonEnemyPrefab;
        [SerializedDictionary("Enemy Name", "Enemy Data")]
        public SerializedDictionary<EnemyName, EnemyData> enemyDict;
    }
}