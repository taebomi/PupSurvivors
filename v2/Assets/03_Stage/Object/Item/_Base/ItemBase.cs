using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    public abstract class ItemBase : MonoBehaviour
    {
        protected ItemController ItemController;

        protected virtual void Awake()
        {
            ItemController = GetComponent<ItemController>();
        }

        public abstract void Apply(Player target);

        public void Set(Vector3 spawnPos) => ItemController.Set(spawnPos);
        public void Set(Vector3 spawnPos, Vector3 dropPos) => ItemController.Set(spawnPos, dropPos);
    }
}