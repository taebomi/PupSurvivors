using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Pool;
using PupSurvivors.Stage.UI;
using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    public class Nyan : PoolingItemController
    {
        [SerializeField] private SpriteRenderer bodySr;
        [SerializeField] private SpriteContainer bodySprites;

        private int _nyan;

        private IObjectPool<Nyan> _pool;

        private static readonly int[] NyanValues =
        {
            10, 25, 50, 100, 250, 500, 1000, 2500
        };

        public void Initialize(IObjectPool<Nyan> pool)
        {
            _pool = pool;
        }

        public void SetNyan(int nyan)
        {
            _nyan = nyan;

            bodySr.sprite = nyan switch
            {
                < 10 => bodySprites.data[0], // 동전
                < 25 => bodySprites.data[1], // 동주머니
                < 50 => bodySprites.data[2], // 동괴
                < 100 => bodySprites.data[3], // 은전
                < 250 => bodySprites.data[4], // 은주머니
                < 500 => bodySprites.data[5], // 은괴
                < 1000 => bodySprites.data[6], // 금전
                < 2500 => bodySprites.data[7], // 금주머니
                >= 2500 => bodySprites.data[8] // 금괴
            };
        }


        public override void Apply(Player target)
        {
            base.Apply(target);
            StageManager.Instance.AddNyan((int)(_nyan * target.CurStats.expMultiplier));
            _pool?.Push(this);
        }

        public static List<int> GenerateRandomNyanList(int nyan, int number)
        {
            var randomNyanList = new List<int>();

            for (var i = 0; i < number - 1; i++)
            {
                var randomNyan = Random.Range(1, nyan - (number - 1 - i));
                randomNyanList.Add(randomNyan);
                nyan -= randomNyan;
            }

            randomNyanList.Add(nyan);
            return randomNyanList;
        }

        public static List<int> ConvertNyan(int nyan)
        {
            var nyanCountList = Enumerable.Repeat(0,NyanValues.Length).ToList();
            var idx = NyanValues.Length - 1;
            while (nyan > NyanValues[0] && idx >= 1)
            {
                var value = NyanValues[idx];
                if (nyan >= value)
                {
                    var num = (int)(nyan / value);
                    nyanCountList[idx] = num;
                    nyan -= num * value;
                }

                idx--;
            }
            
            
            return nyanCountList;
        }
    }
}