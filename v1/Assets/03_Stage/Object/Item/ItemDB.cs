using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(menuName = "TaeBoMi/ItemDB", fileName = "ItemDB", order = 2000)]
public class ItemDB : ScriptableObject
{
    [field: SerializeField] public SerializedDictionary<ItemName, ItemData> itemDataDict;

    public enum ItemName
    {
        Exp,
        RefinedExp,
        Nyan,
        HotDog,
        ManaGem,
        
    }

    [Serializable]
    public class ItemData
    {
        public ItemObjectBase prefab;
        public Sprite icon;
        public int weight;
    }
}
