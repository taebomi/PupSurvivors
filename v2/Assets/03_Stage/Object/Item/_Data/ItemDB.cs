using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using PupSurvivors.Stage.Item;
using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Stage/ItemDB", fileName = "ItemDB")]
public class ItemDB : ScriptableObject
{
    public ExpCrystal expCrystalPrefab;
    public ExpGem expGemPrefab;
    public Nyan nyanPrefab;

    public SerializedDictionary<ItemName, ItemData> db;
    
    [Serializable]
    public class ItemData
    {
        public ItemController prefab;
        public float yOffset;
        public int weight;

        public Sprite icon;
    }

    public enum ItemName
    {
        Exp,
        RefinedExp,
        Nyan,
        HotDog,
    }
}