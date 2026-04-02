using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Material Container", fileName = "MaterialContainer", order = 1000)]
public class MaterialContainer : ScriptableObject
{
    public Material[] data;
}