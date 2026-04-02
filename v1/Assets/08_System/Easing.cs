using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Easing
{
    private const float overshootOrAmplitude = 1.70158f;
    public static float OutQuad(float time, float duration)
    {
        return -(time /= duration) * (time - 2f);
    }

    public static float InSine(float time, float duration)
    {
        return (float)(-Math.Cos((double)time / duration * 1.5707963705062866) + 1.0);
    }

    public static float OutSine(float time, float duration)
    {
        return (float)Math.Sin((double)time / duration * 1.5707963705062866);
    }

    public static float InQuad(float time, float duration)
    {
        return (time /= duration) * time * time;
    }

    public static float InBack(float time, float duration)
    {
        return (time /= duration) * time * ((overshootOrAmplitude + 1.0f) * time - overshootOrAmplitude);
    }

    public static float OutBack(float time, float duration)
    {
        return (time = ( time / duration - 1.0f)) * time * ((overshootOrAmplitude + 1.0f) * time + overshootOrAmplitude) + 1.0f;
    }
}