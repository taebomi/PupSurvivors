using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private TMP_Text _tmp;
    private float[] _frameDeltaTimes;
    private int _lastFrameIndex;
    private float _deltaTime;

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _frameDeltaTimes = new float[30];
    }

    void Update()
    {
        _frameDeltaTimes[_lastFrameIndex] = Time.deltaTime;
        _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimes.Length;
        _tmp.text = $"평균 - {CalculateFPS():N0} fps / 현재 -{1/Time.deltaTime:N0} fps";
    }

    private float CalculateFPS()
    {
        var total = 0f;
        foreach (var frameDeltaTime in _frameDeltaTimes)
        {
            total += frameDeltaTime;
        }

        return _frameDeltaTimes.Length / total;
    }
}