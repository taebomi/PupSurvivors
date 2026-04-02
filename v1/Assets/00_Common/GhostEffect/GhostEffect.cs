using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;

public class GhostEffect : PoolableObject<GhostEffect>
{
    private SpriteRenderer _sr;

    private Tweener _fadeTweener;
    
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingLayerID = TaeBoMiCache.SortingLayer.Object;
        _sr.sortingOrder = -1;
        _fadeTweener = _sr.DOFade(0, 0.333333f)
            .From(0.75f)
            .SetEase(Ease.OutSine)
            .SetAutoKill(false);
    }

    public async UniTaskVoid Set(Vector3 pos, Sprite sprite)
    {
        transform.position = pos;
        _sr.sprite = sprite;
        _fadeTweener.Restart();
        await _fadeTweener.AwaitForComplete();
        ManagedPool.Release(this);
    }
}
