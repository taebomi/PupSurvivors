using PupSurvivors.Stage.Item;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class PlayerMagnet : MonoBehaviour
    {
        [SerializeField] private ItemColliderDictSO itemColliderDictSO;
        [SerializeField] private CircleCollider2D circleCollider2D;

        [SerializeField] private Player player;

        public void OnStatsUpdated(CharacterStats stats)
        {
            circleCollider2D.radius = stats.magnet * 0.01f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (itemColliderDictSO.CollDict.TryGetValue(other, out var itemBase))
            {
                itemBase.Obtain(player).Forget();
            }
        }
    }
}