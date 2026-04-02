using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamagableDetector : MonoBehaviour
{
    public static Dictionary<int, IDamagable> DamagableDict { get; } = new(StageManager.MaxDamagableNum);
    private static HashSet<IDamagable> VisibleDamagableSet { get; } = new(StageManager.MaxDamagableNum);

    private LinkedList<IDamagable>[] _damagableListByDist;

    private IDamagable _nearestDamagable;
    private bool _isNearestCheckedCurrentFrame;
    private CancellationTokenSource _nearestCheckingCts;

    private const int MaxCheckingDist = 20;

    private void Awake()
    {
        _damagableListByDist = new LinkedList<IDamagable>[MaxCheckingDist];
        for (var i = 0; i < _damagableListByDist.Length; i++)
        {
            _damagableListByDist[i] = new LinkedList<IDamagable>();
        }
    }


    public void OnStageStateChanged(StageManager.StageState state)
    {
        if (state == StageManager.StageState.Init)
        {
            DamagableDict.Clear();
            VisibleDamagableSet.Clear();
        }
    }


    #region Visible

    public static void AddVisible(IDamagable target)
    {
        VisibleDamagableSet.Add(target);
    }

    public static void RemoveInvisible(IDamagable target)
    {
        VisibleDamagableSet.Remove(target);
    }

    public static IDamagable GetRandomVisible()
    {
        if (VisibleDamagableSet.Count == 0)
        {
            return null;
        }

        IDamagable result = null;
        var count = 0;

        foreach (var damagable in VisibleDamagableSet)
        {
            count++;
            if (Random.Range(0, count) == 0)
            {
                result = damagable;
            }
        }

        return result;
    }

    public static int GetRandomVisible(IDamagable[] randomDamagables)
    {
        var size = randomDamagables.Length;
        var currentIdx = 0;

        foreach (var damagable in VisibleDamagableSet)
        {
            if (currentIdx < size)
            {
                randomDamagables[currentIdx] = damagable;
            }
            else
            {
                var rand = Random.Range(0, currentIdx + 1);
                if (rand < size)
                {
                    randomDamagables[rand] = damagable;
                }
            }

            currentIdx++;
        }

        return currentIdx;
    }

    #endregion

    #region Nearest

    // 
    public IDamagable GetNearest()
    {
        // 이번 프레임에 이미 체크했으면 캐싱 반환
        if (_isNearestCheckedCurrentFrame)
        {
            return _nearestDamagable;
        }

        FlagNearestChecking().Forget();


        return null;
    }

    private async UniTaskVoid FlagNearestChecking()
    {
        _nearestCheckingCts?.CancelAndDispose();
        _nearestCheckingCts = new CancellationTokenSource();
        _isNearestCheckedCurrentFrame = true;
        await UniTask.Yield(_nearestCheckingCts.Token);
        _isNearestCheckedCurrentFrame = false;
    }

    #endregion
}