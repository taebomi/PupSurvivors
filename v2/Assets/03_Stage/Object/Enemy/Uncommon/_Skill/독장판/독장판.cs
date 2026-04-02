using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

public class 독장판 : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D coll;

    private 독장판Data _data;
    
    private IObjectPool<독장판> _pool;

    private CancellationTokenSource _destroyCts;
    
    public void Initialize(IObjectPool<독장판> pool, 독장판Data 독장판Data)
    {
        _pool = pool;
        _data = 독장판Data;
    }

    private void Awake()
    {
        _destroyCts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _destroyCts.Cancel();
        _destroyCts.Dispose();
    }


    public async UniTaskVoid Set()
    {
        var sprites = _data.sprites.data;
        sr.sprite = sprites[Random.Range(0, sprites.Length)];
        
        coll.enabled = true;
        gameObject.SetActive(true);
            
        var timer = 0f;
        while (timer < _data.duration)
        {
            sr.color = Color.Lerp(_data.startColor, _data.endColor, (Mathf.Sin(Time.time * _data.colorChangingSpeed) + 1)*0.5f);
            timer += Time.deltaTime;
            await UniTask.Yield(_destroyCts.Token);
        }
        coll.enabled = false;

        timer = 0f;
        const float fadingDuration = 0.5f;
        while (timer < fadingDuration)
        {
            sr.color -= new Color(0f, 0f, 0f, 1 / fadingDuration * Time.deltaTime);
            timer += Time.deltaTime;
            await UniTask.Yield(_destroyCts.Token);
        }

        
        gameObject.SetActive(false);
        _pool.Push(this);
    }
}
