using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// 데쉬
/// </summary>
public partial class PlayerController
{
    private float _currentSp;
    public float CurrentSp
    {
        get => _currentSp;
        set => _currentSp = Mathf.Clamp(value, 0f, _maxSp);
    }

    private int _maxSp;

    [field: SerializeField] public UnityEvent SpUsedEvent { get; private set; }
    [field: SerializeField] public UnityEvent<int> MaxSpChangedEvent { get; private set; }
    [field: SerializeField] public UnityEvent<float> SpUpdateEvent { get; private set; }

    public bool CanUseSkill => _currentSp >= 1f;
    private bool _isSpRegenerating;
    
    private CancellationTokenSource _spRegenCts;

    private void InitializeSkill()
    {
        _maxSp = -1;
    }

    public void SetSpFull()
    {
        _spRegenCts?.Cancel();
        _isSpRegenerating = false;
        CurrentSp = _maxSp;
        SpUpdateEvent.Invoke(CurrentSp);
    }
    
    public void OnMaxSpChanged(int num)
    {
        _spRegenCts?.Cancel();
        _isSpRegenerating = false;
        _maxSp = num;
        CurrentSp = num;
        SpUpdateEvent.Invoke(CurrentSp);
    }

    public void OnSpUsed()
    {
        CurrentSp -= 1;
        RegenerateDashGauge().Forget();
    }


    /// <summary>
    /// 발동 조건 - 데쉬 사용 시, 데쉬 게이지 변동 시
    /// </summary>
    private async UniTaskVoid RegenerateDashGauge()
    {
        if (_isSpRegenerating) return;

        _isSpRegenerating = true;
        _spRegenCts?.CancelAndDispose();
        _spRegenCts = new CancellationTokenSource();
        while (CurrentSp < CurrentStats.sp)
        {
            CurrentSp += Time.deltaTime / CurrentStats.skillCooldown;
            SpUpdateEvent.Invoke(_currentSp);
            await UniTask.Yield(_spRegenCts.Token);
        }

        _isSpRegenerating = false;
    }
}