using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "PupSurvivors/EnemyData/독장판 Data", fileName = "독장판Data", order =100)]
public class 독장판Data : ScriptableObject
{
    [SerializeField] private 독장판생성기 독장판생성기Prefab;
    public 독장판 독장판Prefab;

    public float duration;
    [FormerlySerializedAs("speed")] public float colorChangingSpeed;
    public Color startColor, endColor;
    public SpriteContainer sprites;

    public 독장판생성기 Create독장판생성기(int capacity)
    {
        var 독장판생성기 =  Instantiate(독장판생성기Prefab);
        독장판생성기.Initialize(this, capacity);
        return 독장판생성기;
    }
}
