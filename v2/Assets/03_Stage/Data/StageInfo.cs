using System;
using UnityEngine;

namespace PupSurvivors.Stage
{
    [CreateAssetMenu(menuName = "PupSurvivors/Stage Info", fileName = "StageInfo", order = 555)]
    public class StageInfo : ScriptableObject
    {
        public float endTime;
        
        public GoodsValue[] GoodsValueArr;


        public GoodsValue GetCurrentGoodsValue(float curTime)
        {
            var idx = Mathf.Clamp((int)(curTime / 30f), 0, GoodsValueArr.Length);
            return GoodsValueArr[idx];
        }

        [Serializable]
        public class GoodsValue
        {
            public float time;
            public int destructibleNyan;
        }
    }
}