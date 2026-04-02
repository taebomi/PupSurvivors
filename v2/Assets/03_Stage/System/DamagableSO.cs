using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage;
using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Stage/Visible Damagable SO", fileName = "VisibleDamagableSO", order = 99999)]
// ReSharper disable once InconsistentNaming
public class DamagableSO : ScriptableObject
{
    public readonly Dictionary<Collider2D, DamagableHealthSystemBase> CollDict = new();
    public readonly HashSet<DamagableHealthSystemBase> HashSet = new ();
}
