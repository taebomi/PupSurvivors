using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private StageWaveData currentStageWaveData; // todo 스테이지 추가 시 - 현재 스테이지에 따라 불러오도록 수정
    [SerializeField] private EnemyDB enemyDB;


    private CancellationTokenSource _disableCts;

    private void Awake()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    public void OnStageStateChanged(StageManager.StageState state)
    {
        if (state == StageManager.StageState.Start)
        {
            StartWave().Forget();
        }
    }


    private async UniTaskVoid StartWave()
    {

        foreach (var waveData in currentStageWaveData.waveData)
        {
            
            
            // 다음 웨이브까지 대기 
            await UniTask.Delay(TimeSpan.FromSeconds(waveData.time), cancellationToken: _disableCts.Token);
        }
    }
    
}