using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PupSurvivors.ObjectPool;
using PupSurvivors.Stage.UI;
using PupSurvivors.Stage.UI.LevelUp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public partial class StageManager
{
    private LimitedObjectPool<ExpObject> _expPool;
    [SerializeField] private ExpObject expObjectPrefab;

    [SerializeField] private ExpTable expTable; // 레벨 당 필요한 경험치 테이블

    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private ExpContainer expContainer;

    public int CurrentLevel { get; private set; }
    private float _requiredExp;
    private float _currentExp;

    private const int MaxExpObjectNumber = 600;
    private const int MaxVisibleExpObjectCount = 300;


    private void InitializeExp()
    {
        var expObjectContainer = new GameObject("Exp Objects").transform;
        expObjectContainer.SetParent(ObjectPoolContainer);
        _expPool = new LimitedObjectPool<ExpObject>(() =>
        {
            var expObject = Instantiate(expObjectPrefab, expObjectContainer);
            expObject.SetManagedPool(_expPool);
            ItemColliderDict.Add(expObject.MainCollider.GetInstanceID(), expObject);
            return expObject;
        }, MaxExpObjectNumber);
        
        CurrentLevel = 1;
        _currentExp = 0f;
        UpdateRequiredExp();
        expContainer.UpdateExpGauge(0f);
    }
    
    public void CreateExpObject(float expValue, Vector3 pos)
    {
        // 화면에 존재할 수 있는 수보다 많이 보일 경우 생성 X
        if (ExpObject.VisibleCount > MaxVisibleExpObjectCount)
        {
            // 플레이어가 가만히 파밍하는 것을 방지하기 위함.
            // 경험치 합쳐줄 경우 여기서 작업 ( git에 있음 )
            return;
        }

        // 현재 시야 내 경험치일 경우 다시 뽑기
        var exp = _expPool.Get();
        while (exp.IsVisible)
        {
            exp = _expPool.Get();
        }

        // 화면 밖 경험치 있는 오브젝트일 경우 경험치 뭉쳐주기
        if (exp.ExpValue > 0f)
        {
            var mergeExp = _expPool.Get();
            mergeExp.AddExp(exp.ExpValue);
        }

        exp.Set(pos);
        exp.SetExp(expValue);
    }


    public void AddExp(float exp)
    {
        _currentExp += exp;
        if (_currentExp >= _requiredExp)
        {
            LevelUp().Forget();
        }

        expContainer.UpdateExpGauge(_currentExp / _requiredExp);
    }

    /// <summary>
    /// 경험치통 비율만큼 획득
    /// </summary>
    /// <param name="ratio">0 ~ 1 사이 값</param>
    public void AddExpRatio(float ratio)
    {
        AddExp(_requiredExp * ratio);
    }

    public async UniTaskVoid LevelUp()
    {
        Time.timeScale = 0f;
        expContainer.SetLevelUpEffect(true);

        do
        {
            _currentExp -= _requiredExp;

            CurrentLevel++;
            UpdateRequiredExp();
            expContainer.SetLevelText(CurrentLevel);

            await levelUpUI.ShowUI(PlayerController.Instance); // 플레이어 여럿일 때 요거 반복문으로 하나씩 띄우기
        } while (_currentExp >= _requiredExp);

        expContainer.SetLevelUpEffect(false);
        Time.timeScale = 1f;
    }

    private void UpdateRequiredExp()
    {
        _requiredExp = expTable.requiredExp[CurrentLevel - 1];
    }
}