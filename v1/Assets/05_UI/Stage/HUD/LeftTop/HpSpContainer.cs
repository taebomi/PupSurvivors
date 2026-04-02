using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HpSpContainer : MonoBehaviour
{
    [SerializeField] private Image hpGaugeContainerImage, spGaugeContainerImage; // hp / sp 컨테이너 테두리
    [SerializeField] private Image hpGaugeImage; // hp 게이지
    [SerializeField] private Image[] spGaugeImages; // sp 게이지들
    [SerializeField] private Image hpIcon, spIcon; // hp / sp 아이콘
    [SerializeField] private Sprite[] hpIconSprites, spIconSprites, hpGaugeSprites, spGaugeSprites; // 들어갈 스프라이트들
    [SerializeField] private Material hpMaterial, spMaterial; // 반짝임 이펙트용 머티리얼


    private const float HpSpGaugeContainerHeight = 35f; // hp sp 게이지 컨테이너 높이
    private const float HpSpGaugeHeight = 15f; // hp sp 게이지 높이

    private int _hpGaugeCurrentWidthDot; // hp 현재 너비 도트 수
    private int _hpGaugeMaxWidthDot; // hp 최대치 너비 도트 수
    private const float HpGaugeWidthMultiplier = 2f; // 해당 값을 체력값에 곱해 체력 사이즈 설정
    private const float HpGaugeContainerPadding = 30f; // 게이지 컨테이너와 게이지 너비 차

    private const int SpGaugeMaxWidthDot = 18; // sp 최대치 너비 도트 수
    private const float SpGaugeContainerFirstSize = 120f; // sp 게이지 컨테이너 1개일때 크기
    private const float SpGaugeContainerSize = 110f; // sp 게이지 컨테이너 2개 이상일때 1개당 크기

    private int _currentSpIdx; // 현재 변동되고 있는 sp의 인덱스

    private CancellationTokenSource _disableCts;
    private CancellationTokenSource _hpEffectCts, _spEffectCts; // hp 감소, sp 찰 때 이펙트 


    private void OnEnable()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    #region Health Point

    // hp 최대치 변동 시 
    public void OnMaxHpChanged(float maxHp)
    {
        var maxHpGaugeWidth = maxHp * HpGaugeWidthMultiplier;
        _hpGaugeMaxWidthDot = (int)(maxHpGaugeWidth * TaeBoMiCache.DotPerWidth);
        hpGaugeContainerImage.rectTransform.sizeDelta =
            new Vector2(_hpGaugeMaxWidthDot * TaeBoMiCache.WidthPerDot + HpGaugeContainerPadding,
                HpSpGaugeContainerHeight);
    }

    // 데미지 입을 시
    public void OnDamaged() => PlayHpDecreaseEffect().Forget();

    private async UniTaskVoid PlayHpDecreaseEffect()
    {
        _hpEffectCts?.CancelAndDispose();
        _hpEffectCts = new CancellationTokenSource();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_disableCts.Token, _hpEffectCts.Token);

        hpIcon.sprite = hpIconSprites[1];

        // 머티리얼 Glow
        var timer = 0f;
        var value = 0f;
        while (timer < PlayerHealthSystem.InvincibleDuration)
        {
            hpMaterial.SetFloat(TaeBoMiCache.Glow, 1 - value);
            timer += Time.unscaledDeltaTime;
            value = Easing.InSine(timer, PlayerHealthSystem.InvincibleDuration);
            await UniTask.Yield(cts.Token);
        }

        hpMaterial.SetFloat(TaeBoMiCache.Glow, 0f);

        hpIcon.sprite = hpIconSprites[0];
    }

    // hp 변동 시
    public void OnHpUpdated(float currentHp)
    {
        var currentWidthPixel = (int)(currentHp * HpGaugeWidthMultiplier * TaeBoMiCache.DotPerWidth);
        if (_hpGaugeCurrentWidthDot == currentWidthPixel)
        {
            return;
        }

        // 사이즈에 따라 스프라이트 변경
        hpGaugeImage.sprite = currentWidthPixel switch
        {
            1 => hpGaugeSprites[0],
            2 => hpGaugeSprites[1],
            _ => hpGaugeSprites[2]
        };

        // 픽셀만큼 크기 변경
        hpGaugeImage.rectTransform.sizeDelta =
            new Vector2(currentWidthPixel * TaeBoMiCache.WidthPerDot, HpSpGaugeHeight);

        _hpGaugeCurrentWidthDot = currentWidthPixel;
    }

    #endregion


    #region Skill Point

    public void OnSpUpdated(float ratio)
    {
        var destIdx = (int)ratio;
        var value = ratio - destIdx;
        if (_currentSpIdx == destIdx) // 같은 칸에서 변동 있을 시
        {
            ChangeSpGaugeSize(_currentSpIdx, value);
        }
        else if (_currentSpIdx > destIdx) // 줄어들었을 경우
        {
            for (var i = _currentSpIdx; i > destIdx; i--)
            {
                ChangeSpGaugeSize(i, 0f);
            }

            ChangeSpGaugeSize(destIdx, value);
        }
        else if (_currentSpIdx < destIdx) // 많아졌을 경우
        {
            for (var i = _currentSpIdx; i < destIdx; i++)
            {
                ChangeSpGaugeSize(i, 1f);
            }

            ChangeSpGaugeSize(destIdx, value);
            PlaySpFullEffect().Forget();
        }

        _currentSpIdx = destIdx;
    }

    // idx번째 sp게이지에 ratio만큼 채우기
    private void ChangeSpGaugeSize(int idx, float ratio)
    {
        var pixel = (int)(ratio * SpGaugeMaxWidthDot);

        spGaugeImages[idx].sprite = pixel switch
        {
            1 => spGaugeSprites[0],
            2 => spGaugeSprites[1],
            _ => spGaugeSprites[2]
        };
        spGaugeImages[idx].rectTransform.sizeDelta = new Vector2(pixel * TaeBoMiCache.WidthPerDot, HpSpGaugeHeight);
    }

    // 최대치에 맞게 sp 게이지 변경
    public void OnMaxSpChanged(int num)
    {
        if (num == 0) // Sp가 존재하지 않으면 전부 비활성화 
        {
            spGaugeContainerImage.enabled = false;
            foreach (var spGaugeImage in spGaugeImages)
            {
                spGaugeImage.enabled = false;
            }

            return;
        }


        spGaugeContainerImage.enabled = true;
        spGaugeContainerImage.rectTransform.sizeDelta =
            new Vector2(SpGaugeContainerFirstSize + (num - 1) * SpGaugeContainerSize, HpSpGaugeContainerHeight);

        for (var i = 0; i < num; i++)
        {
            spGaugeImages[i].enabled = true;
        }

        for (var i = num; i < spGaugeImages.Length; i++)
        {
            spGaugeImages[i].enabled = false;
        }
    }

    // 아이콘 및 게이지 반짝이기
    private async UniTaskVoid PlaySpFullEffect()
    {
        _spEffectCts?.CancelAndDispose();
        _spEffectCts = new CancellationTokenSource();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_disableCts.Token, _spEffectCts.Token);

        spIcon.sprite = spIconSprites[1];

        // 머티리얼 Glow
        var timer = 0f;
        var value = 0f;
        const float duration = 0.2f;
        while (timer < duration)
        {
            spMaterial.SetFloat(TaeBoMiCache.Glow, 1 - value);
            timer += Time.unscaledDeltaTime;
            value = Easing.InSine(timer, duration);
            await UniTask.Yield(cts.Token);
        }

        spMaterial.SetFloat(TaeBoMiCache.Glow, 0f);
        spIcon.sprite = spIconSprites[0];
    }

    #endregion
}