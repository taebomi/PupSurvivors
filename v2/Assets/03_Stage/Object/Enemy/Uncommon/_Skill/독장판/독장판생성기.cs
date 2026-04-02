using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Pool;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class 독장판생성기 : MonoBehaviour
{
    private IObjectPool<독장판> _독장판Pool;

    public void Initialize(독장판Data 독장판Data, int capacity)
    {
        _독장판Pool = new ObjectPool<독장판>(
            () =>
            {
                var 독장판 = Instantiate(독장판Data.독장판Prefab, transform);
                독장판.Initialize(_독장판Pool, 독장판Data);
                return 독장판;
            },
            capacity, null, null,
            Object.Destroy);
    }

    public void CreatePoison(Vector3 pos)
    {
        var 독장판 = _독장판Pool.Get();
        독장판.transform.position = pos;
        독장판.Set().Forget();
    }

    public void Destroy()
    {
        _독장판Pool.Destroy();
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // todo 플레이어에게 데미지 주기
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }
}