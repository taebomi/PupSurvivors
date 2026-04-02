using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpContainer : MonoBehaviour
{
    [SerializeField] private RectTransform gaugeRectTransform;
    [SerializeField] private Image levelUpGaugeImage;

    [SerializeField] private TMP_Text levelText;

    private const float GaugeHeight = 15f;
    private const float GaugeMinWidth = 15f;
    private const float GaugeMaxWidth = 1000f;

    private float _currentWidth, _destWidth, _gaugeSpeed;
    private const float DefaultGaugeSpeed = (GaugeMaxWidth - GaugeMinWidth) * 1.5f;

    private CancellationTokenSource _levelUpEffectCts;
    private CancellationTokenSource _destroyCts;


    private void Awake()
    {
        _destWidth = _currentWidth = GaugeMinWidth;
        _gaugeSpeed = DefaultGaugeSpeed;

        _destroyCts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _destroyCts.CancelAndDispose();
    }

    private void Update()
    {
        _currentWidth += _gaugeSpeed * Time.deltaTime;
        if (_currentWidth > _destWidth)
        {
            _currentWidth = _destWidth;
            _gaugeSpeed = 0f;
        }
        else if (_currentWidth < _destWidth)
        {
            _gaugeSpeed += Time.deltaTime * DefaultGaugeSpeed;
        }

        var pixel = (int)(_currentWidth * TaeBoMiCache.DotPerWidth);
        gaugeRectTransform.sizeDelta = new Vector2(pixel * TaeBoMiCache.WidthPerDot, GaugeHeight);
    }


    // 현재 경험치 비율에 따라 게이지 채우기.
    public void UpdateExpGauge(float ratio)
    {
        var value = ratio - (int)ratio;
        _destWidth = value * (GaugeMaxWidth - GaugeMinWidth) + GaugeMinWidth;
    }

    public void SetLevelText(int level)
    {
        levelText.text = $"LV.{level:00}";
    }

    public void SetLevelUpEffect(bool value)
    {
        if (value)
        {
            AnimateLevelUpGauge().Forget();
        }
        else
        {
            _levelUpEffectCts.Cancel();
            levelUpGaugeImage.enabled = false;
        }
    }

    private async UniTaskVoid AnimateLevelUpGauge()
    {
        _levelUpEffectCts?.CancelAndDispose();
        _levelUpEffectCts = new CancellationTokenSource();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_levelUpEffectCts.Token, _destroyCts.Token);
        levelUpGaugeImage.enabled = true;
        var timer = 0f;
        while (true)
        {
            while (timer > 0.8f)
            {
                timer -= 0.8f;
            }

            var value = timer - 0.58f;
            levelUpGaugeImage.material.SetFloat(TaeBoMiCache.ColorRampLuminosity, value);
            timer += Time.unscaledDeltaTime;
            await UniTask.Yield(cts.Token);
        }
    }
}