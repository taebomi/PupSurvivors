using System;
using System.Collections.Generic;
using PupSurvivors.Pool;
using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    public class ExpGem : PoolingItemController
    {
        [SerializeField] private FloatEventChannelSO addExpRatioEventChannelSO;
        [SerializeField] private SpriteRenderer bodySr;
        [SerializeField] private SpriteContainer bodySprites;

        private Grade _grade;

        private IObjectPool<ExpGem> _pool;

        private static readonly float[] GemValue = { 0.01f, 0.05f, 0.25f, 0.5f, 1f };

        public enum Grade
        {
            Lowest,
            Lower,
            Intermediate,
            Higher,
            Highest
        }

        public void Initialize(IObjectPool<ExpGem> pool)
        {
            _pool = pool;
        }

        public void SetGrade(Grade grade)
        {
            _grade = grade;
            bodySr.sprite = bodySprites.data[(int)grade];
        }

        public override void Apply(Player target)
        {
            base.Apply(target);
            var ratio = GemValue[(int)_grade];
            addExpRatioEventChannelSO.RaiseEvent(ratio * target.CurStats.expMultiplier);

            _pool.Push(this);
        }

        public static List<int> GetGemNumFromRatio(float ratio)
        {
            var gemCountList = new List<int>(5) { 0, 0, 0, 0, 0 };
            var idx = GemValue.Length - 1;
            while (ratio > 0f && idx >= 0)
            {
                var value = GemValue[idx];
                if (ratio >= value)
                {
                    var num = (int)(ratio / value);
                    gemCountList[idx] = num;
                    ratio -= num * value;
                }

                idx--;
            }

            return gemCountList;
        }
    }
}