using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage;
using PupSurvivors.Stage.UI;
using UnityEngine;

public class EmotionBubbleController : MonoBehaviour
{
    private EmotionBubble _curEmotionBubble;

    private void Update()
    {
        if (_curEmotionBubble)
        {
            _curEmotionBubble.transform.position = transform.position;
        }
    }

    public void SetEmotion(EmotionBubble.Emotion emotion)
    {
        if (!_curEmotionBubble)
        {
            _curEmotionBubble = StageManager.Instance.EmotionBubblePool.Get();
            _curEmotionBubble.ActionOnCloseFinished += ResetCurEmotionBubble;
        }

        _curEmotionBubble.Set(emotion).Forget();
    }

    public void RemoveEmotionBubble()
    {
        if (_curEmotionBubble)
        {
            _curEmotionBubble.Remove().Forget();
        }
    }

    private void ResetCurEmotionBubble()
    {
        _curEmotionBubble.ActionOnCloseFinished -= ResetCurEmotionBubble;
        _curEmotionBubble = null;
    }
}