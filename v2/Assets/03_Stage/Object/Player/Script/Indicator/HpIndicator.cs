using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HpIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hpSr;

    private float _maxHp;
    private const float IndicatorLength = 1.5f;
    private const float IndicatorHeight = 0.25f;

    public void OnMaxHpChanged(float max)
    {
        _maxHp = max;
    }

    public void OnHpUpdated(float value)
    {
        hpSr.size = new Vector2(value / _maxHp * IndicatorLength, IndicatorHeight);
    }
}
