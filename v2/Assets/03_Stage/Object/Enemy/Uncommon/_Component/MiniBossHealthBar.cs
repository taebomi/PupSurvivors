using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Pool;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MiniBossHealthBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fgSr, bgSr; // todo 몬스터 크기에 따라 bg도 변경됨.

    private float _maxHp;
    private const float BarWidthMultiplier = 1.5f; // todo 몬스터 크기에 따라 곱해줄 size 변수 추가하기
    private const float BarHeight = 0.25f;

    private IHpNotifier _healthSystem;

    /// <summary>
    /// 사용 시 대상의 자식으로 넣고 위치 설정해주기
    /// </summary>
    /// <param name="healthSystem"></param>
    public void Initialize(IHpNotifier healthSystem)
    {
        _healthSystem = healthSystem;
        _healthSystem.ActionOnCurHpChanged += OnHpChanged;
        _healthSystem.ActionOnMaxHpChanged += OnMaxHpChanged;
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.ActionOnCurHpChanged -= OnHpChanged;
            _healthSystem.ActionOnMaxHpChanged -= OnMaxHpChanged;
        }
    }

    private void OnMaxHpChanged(float maxHp)
    {
        _maxHp = maxHp;
    }

    private void OnHpChanged(float curHp)
    {
        fgSr.size = new Vector2(curHp / _maxHp * BarWidthMultiplier, BarHeight);
    }
}