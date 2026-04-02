using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CameraManager : Singleton<CameraManager>
{
    [field: SerializeField] public CinemachineVirtualCamera MainCam { get; private set; }

    public const float CameraHalfHeight = 9f;
    public const float CameraHalfWidth = 16f;
    private const float CameraOutsideMinHeight = CameraHalfHeight + 1;
    private const float CameraOutsideMinWidth = CameraHalfWidth + 1;
    private const float CameraOutsideMaxHeight = CameraHalfHeight + 3;
    private const float CameraOutsideMaxWidth = CameraHalfWidth + 3;

    protected override void AwakeAfter()
    {
#if UNITY_EDITOR
        if (Math.Abs(CameraHalfHeight - MainCam.m_Lens.OrthographicSize) > 0.1f)
        {
            Debug.Log(
                $"{transform.name} - 카메라 사이즈({MainCam.m_Lens.OrthographicSize})와 스크립트 사이즈({CameraHalfHeight})가 일치하지 않음. 수정 바람");
        }
#endif
    }

    public Vector3 GetCameraInsideRandomPosition()
    {
        var randomPosition = MainCam.transform.position;
        randomPosition.z = 0f;
        randomPosition += new Vector3(Random.Range(-CameraHalfWidth, CameraHalfWidth),
            Random.Range(-CameraHalfHeight, CameraHalfHeight));
        return randomPosition;
    }

    public Vector3 GetCameraOutsideRandomPosition()
    {
        var randomCondition = Random.Range(0, 4);
        var randomPosition = MainCam.transform.position;
        randomPosition.z = 0f;
        switch (randomCondition)
        {
            case 0: // 상
                randomPosition.x += Random.Range(-CameraHalfWidth, CameraOutsideMaxWidth);
                randomPosition.y += Random.Range(CameraOutsideMinHeight, CameraOutsideMaxHeight);
                break;
            case 1: // 하
                randomPosition.x += Random.Range(-CameraOutsideMaxWidth, CameraHalfWidth);
                randomPosition.y += Random.Range(-CameraOutsideMaxHeight, -CameraOutsideMinHeight);
                break;
            case 2: // 좌
                randomPosition.x += Random.Range(-CameraOutsideMaxWidth, -CameraOutsideMinWidth);
                randomPosition.y += Random.Range(-CameraHalfHeight, CameraOutsideMaxHeight);
                break;
            case 3: // 우
                randomPosition.x += Random.Range(CameraOutsideMinWidth, CameraOutsideMaxWidth);
                randomPosition.y += Random.Range(-CameraOutsideMaxHeight, CameraOutsideMinHeight);
                break;
        }

        return randomPosition;
    }
}