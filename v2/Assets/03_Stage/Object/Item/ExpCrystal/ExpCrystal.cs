using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage.Item
{
    public class ExpCrystal : ItemController
    {
        [SerializeField] private FloatEventChannelSO addExpEventChannelSO;
        [SerializeField] protected SpriteRenderer bodySr, shadowSr;
        [SerializeField] private SpriteContainer expSprites;

        public static int VisibleCount { get; private set; }

        private IObjectPool<ExpCrystal> _pool;


        public float ExpValue { get; private set; }

        public void Initialize(IObjectPool<ExpCrystal> pool)
        {
            _pool = pool;
        }
        
        public void AddExp(float value) => SetExp(ExpValue + value);
        
        public override void Set(Vector3 spawnPos)
        {
            CanFlow = true;
            bodySortingGroup.sortingOrder = -1;
            bodySr.enabled = true;
            shadowSr.enabled = true;
            transform.position = spawnPos;
            rb.simulated = true;
        }

        public override async UniTaskVoid Set(Vector3 spawnPos, Vector3 destPos)
        {
            CanFlow = false;
            bodySortingGroup.sortingOrder = 0;
            bodySr.enabled = true;
            shadowSr.enabled = true;
            await MoveTo(spawnPos, destPos);
            transform.position = destPos;
            CanFlow = true;
            bodySortingGroup.sortingOrder = -1;
            rb.simulated = true;
            if (IsVisible)
            {
                Flow().Forget();
            }
        }
        
        

        public void SetExp(float value)
        {
            ExpValue = value;
            bodySr.sprite = value switch
            {
                < 5 => expSprites.data[0],
                < 15 => expSprites.data[1],
                < 50 => expSprites.data[2],
                < 200 => expSprites.data[3],
                _ => expSprites.data[4]
            };
        }

        public override void OnVisibleChanged(bool isVisible)
        {
            base.OnVisibleChanged(isVisible);
            if (isVisible)
            {
                VisibleCount++;
            }
            else
            {
                VisibleCount--;
            }
        }

        public override void Apply(Player target)
        {
            bodySr.enabled = false;
            shadowSr.enabled = false;
            seEventChannelSO.RaiseEvent(useSe);
            addExpEventChannelSO.RaiseEvent(ExpValue * target.CurStats.expMultiplier);
            ExpValue = 0f;
            _pool.Push(this);
        }
    }
}