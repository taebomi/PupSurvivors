using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    [CreateAssetMenu(menuName = "PupSurvivors/Stage/Item Collider Dict SO", fileName = "ItemColliderDictSO", order = 9999)]
    public class ItemColliderDictSO : ScriptableObject
    {
        public readonly Dictionary<Collider2D, IItem> CollDict = new ();
    }
}