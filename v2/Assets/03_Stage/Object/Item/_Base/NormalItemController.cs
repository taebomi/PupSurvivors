using Cysharp.Threading.Tasks;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace PupSurvivors.Stage.Item
{
    /// <summary>
    /// 경험치 크리스탈/젬, 냥을 제외한 풀링이 사용되지 않는 1회성 아이템의 컨트롤러
    /// </summary>
    public class NormalItemController : ItemController
    {
        private ItemBase _item;

        protected override void Awake()
        {
            base.Awake();
            _item = GetComponent<ItemBase>();
        }

        public override void Set(Vector3 spawnPos)
        {
            bodySortingGroup.sortingOrder = -1;
            transform.position = spawnPos;
            CanFlow = true;
            rb.simulated = true;
            if (IsVisible)
            {
                Flow().Forget();
            }
        }

        public override async UniTaskVoid Set(Vector3 spawnPos, Vector3 destPos)
        {
            CanFlow = false;
            await MoveTo(spawnPos, destPos);
            Set(spawnPos);
        }

        public override void Apply(Player target)
        {
            seEventChannelSO.RaiseEvent(useSe);
            _item.Apply(target);
            Destroy(gameObject);
        }
    }
}