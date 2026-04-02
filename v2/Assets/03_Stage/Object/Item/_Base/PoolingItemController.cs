using Cysharp.Threading.Tasks;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace PupSurvivors.Stage.Item
{
    public class PoolingItemController : ItemController
    {
        public override void Set(Vector3 spawnPos)
        {
            CanFlow = true;
            bodySortingGroup.sortingOrder = -1;
            transform.position = spawnPos;
            rb.simulated = true;
            gameObject.SetActive(true);
        }

        public override async UniTaskVoid Set(Vector3 spawnPos, Vector3 destPos)
        {
            CanFlow = false;
            bodySortingGroup.sortingOrder = 0;
            gameObject.SetActive(true);
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
        public override void Apply(Player target)
        {
            seEventChannelSO.RaiseEvent(useSe);
            gameObject.SetActive(false);
        }
    }
}