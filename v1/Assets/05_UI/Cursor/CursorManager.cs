using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D releaseTexture, pressedTexture; // 커서 텍스쳐

    private const CursorMode CursorMode = UnityEngine.CursorMode.Auto; // Auto는 OS에서 설정하는 듯
    private readonly Vector2 _hotspot = Vector2.zero; // 커서 텍스쳐 포인터 눌리는 지점 위치, 좌상단이 0,0

    private LimitedObjectPool<ClickEffect> _pressEffectPool;
    [SerializeField] private ClickEffect pressEffectPrefab;

    private void Awake()
    {
        Cursor.SetCursor(releaseTexture, _hotspot, CursorMode);
        _pressEffectPool = new LimitedObjectPool<ClickEffect>(CreatePressedEffect, 3);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(pressedTexture, _hotspot, CursorMode);
            // 이펙트 생성
            var effect = _pressEffectPool.Get();
            effect.transform.position = Input.mousePosition;
            effect.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(releaseTexture, _hotspot, CursorMode);
        }
    }

    public void ChangeCursorLockMode(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }
    
    private ClickEffect CreatePressedEffect()
    {
        var effect = Instantiate(pressEffectPrefab, transform);
        effect.SetManagedPool(_pressEffectPool);
        return effect;
    }
}