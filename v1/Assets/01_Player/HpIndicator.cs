using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    private float _maxHp;
    private const float IndicatorLength = 1.5f;
    private const float IndicatorHeight = 0.25f;

    public void OnMaxHpChanged(float max)
    {
        _maxHp = max;
    }

    public void OnHpUpdated(float value)
    {
        sr.size = new Vector2(value / _maxHp * IndicatorLength, IndicatorHeight);
    }
}
