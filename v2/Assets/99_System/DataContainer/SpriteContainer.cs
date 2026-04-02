using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Sprite Container", fileName = "SpriteContainer", order = 1000)]
public class SpriteContainer : ScriptableObject
{
    public Sprite[] data;
}
