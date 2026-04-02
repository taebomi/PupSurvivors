using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public partial class StageManager
{
    public Dictionary<int, IDamagable> DamagableDict { get; private set; }
    
    public const int MaxDamagableNum = MaxDestructibleNum + MaxCommonEnemyNum;
    private const int MaxCommonEnemyNum = 750;
    
    private void InitializeDamagable()
    {
        DamagableDict = new Dictionary<int, IDamagable>(MaxDamagableNum);
    }
}