using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        [SerializeField] private MiniBossChest miniBossChest;

        private Transform _chestContainer;

        private void InitializeChest()
        {
            _chestContainer = new GameObject("Chest").transform;
        }

        public void CreateMiniBossChest(Vector3 pos, float ratioExp, int nyan)
        {
            var chest = Instantiate(miniBossChest, pos, quaternion.identity, _chestContainer);
            chest.Set(ratioExp, nyan);
        }
    }
}