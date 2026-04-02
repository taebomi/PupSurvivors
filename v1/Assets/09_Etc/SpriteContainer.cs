using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "TaeBoMi/Sprite Container", fileName = "SpriteContainer", order = 0)]
public partial class SpriteContainer : ScriptableObject
{
    [FormerlySerializedAs("sprites")] public Sprite[] data;
}
