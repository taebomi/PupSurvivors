using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PupSurvivors/Stage/Exp Table", fileName = "ExpTable", order = 9999)]
public class ExpTable : ScriptableObject
{
    public int[] requireExpArr;
}
