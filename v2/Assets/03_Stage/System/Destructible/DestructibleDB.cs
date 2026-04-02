using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Destructible DB", fileName = "DestructibleDB")]
public class DestructibleDB : ScriptableObject
{

    public SerializedDictionary<string, DestructibleData> db;
    
    [Serializable]
    public struct DestructibleData
    {
        public Sprite bodySprite;
        public Sprite shadowSprite;
        public Vector2 offset;
        public float radius;

        public float floatingDamageYPos;
    }
    
    
}
