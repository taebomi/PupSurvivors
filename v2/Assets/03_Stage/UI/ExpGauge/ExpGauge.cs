using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExpGauge : MonoBehaviour
{
    [SerializeField] private FloatEventChannelSO ratioChangedEvent;
    [SerializeField] private IntEventChannelSO levelChangedEvent;
    
    [SerializeField] private VoidEventChannelSO maxLevelEvent;

    [SerializeField] private RectTransform gaugeRectTr;
    [SerializeField] private Image fullGaugeImage;

    [SerializeField] private TMP_Text levelText;

    private bool _isGaugeChanging;
    private float _curWidth, _destWidth, _gaugeSpeed;
    private const float DefaultGaugeSpeed = (GaugeMaxWidth - GaugeMinWidth) * 1.5f;
    private CancellationTokenSource _destroyCts, _fullGaugeEffectCts;

    private const float GaugeHeight = 15f;
    private const float GaugeMinWidth = 15f;
    private const float GaugeMaxWidth = 1000f;

    private void Awake()
    {
        ratioChangedEvent.OnEventRaised += OnExpRatioChanged;
        levelChangedEvent.OnEventRaised += OnLevelChanged;
        maxLevelEvent.OnEventRaised += OnMaxLevel;

        _destWidth = _curWidth = GaugeMinWidth;
        _gaugeSpeed = DefaultGaugeSpeed;
        OnExpRatioChanged(0f);
        OnLevelChanged(1);
        

        _destroyCts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _destroyCts.CancelAndDispose();

        levelChangedEvent.OnEventRaised -= OnLevelChanged;
        ratioChangedEvent.OnEventRaised -= OnExpRatioChanged;
        maxLevelEvent.OnEventRaised -= OnMaxLevel;
    }
    
    private void OnExpRatioChanged(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        _destWidth = ratio * (GaugeMaxWidth - GaugeMinWidth) + GaugeMinWidth;
        
        if (!_isGaugeChanging)
        {
            _isGaugeChanging = true;       
            IncreaseGauge().Forget();
        }
    }

    private async UniTaskVoid IncreaseGauge()
    {
        while (_curWidth < _destWidth && _destroyCts.IsCancellationRequested is false)
        {
            _curWidth += _gaugeSpeed * Time.deltaTime;
            _gaugeSpeed += DefaultGaugeSpeed * Time.deltaTime;
            UpdateGauge();
            await UniTask.Yield(_destroyCts.Token);
        }

        _isGaugeChanging = false;
        _curWidth = _destWidth;
        _gaugeSpeed = 0f;
        UpdateGauge();
    }

    private void UpdateGauge()
    {
        var pixel = (int)(_curWidth * TaeBoMiCache.DotPerWidth);
        gaugeRectTr.sizeDelta = new Vector2(pixel * TaeBoMiCache.WidthPerDot, GaugeHeight);

    }

    private void OnLevelChanged(int level)
    {
        levelText.text = $"Lv.{level}";
    }

    private void OnMaxLevel()
    {
        levelText.text = "Lv.Max";
        fullGaugeImage.enabled = true;
        AnimateLevelUpGauge().Forget();
    }

    private async UniTaskVoid AnimateLevelUpGauge()
    {
        _fullGaugeEffectCts?.CancelAndDispose();
        _fullGaugeEffectCts = new CancellationTokenSource();

        var value = 0f;
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_fullGaugeEffectCts.Token, _destroyCts.Token);
        while (cts.IsCancellationRequested is false)
        {
            const float maxValue = 0.8f;
            while (value > maxValue)
            {
                value -= maxValue;
            }

            const float addValue = -0.58f;
            fullGaugeImage.material.SetFloat(TaeBoMiCache.ColorRampLuminosity, value + addValue);
            value += Time.unscaledDeltaTime;
            await UniTask.Yield(cts.Token);
        }
    }
}