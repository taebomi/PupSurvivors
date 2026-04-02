using System;
using PupSurvivors.ObjectPool;
using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    public class RefinedExp : ItemObjectBase
    {
        private static LimitedObjectPool<RefinedExp> _managedPool;

        private Type _currentType;

        public enum Type
        {
            Lowest,
            Lower,
            Intermediate,
            Higher,
            Highest
        }

        [SerializeField] private SpriteContainer sprites;

        public static void SetManagedPool(LimitedObjectPool<RefinedExp> pool)
        {
            _managedPool = pool;
        }

        public void SetExp(Type type)
        {
            _currentType = type;
            MainSr.sprite = sprites.data[(int)type];
        }

        protected override void ApplyItem(PlayerController target)
        {
            switch (_currentType)
            {
                case Type.Lowest:
                    StageManager.Instance.AddExpRatio(0.02f);
                    break;
                case Type.Lower:
                    StageManager.Instance.AddExpRatio(0.075f);
                    break;
                case Type.Intermediate:
                    StageManager.Instance.AddExpRatio(0.2f);
                    break;
                case Type.Higher:
                    StageManager.Instance.AddExpRatio(0.5f);
                    break;
                case Type.Highest:
                    StageManager.Instance.AddExpRatio(1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _managedPool.Push(this);
        }
    }
}