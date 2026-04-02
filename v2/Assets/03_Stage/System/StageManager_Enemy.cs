using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        // public Dictionary<int, DamagableHealthSystemBase> DamagableDict { get; private set; }
        [field:SerializeField] public DamagableSO DamagableSo { get; private set; }

        private void InitEnemy()
        {
            // DamagableDict = new Dictionary<int, DamagableHealthSystemBase>();
        }
    }
}