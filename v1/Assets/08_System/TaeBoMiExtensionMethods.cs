using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public static class TaeBoMiExtensionMethods
{
    public static void CancelAndDispose(this CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
    }

    #region TMP

    private static readonly char[] TMPChars = new char[5];
    private static int _startIdx;
    private const char ZeroChar = '0';

    public static void SetIntText(this TextMeshPro tmp, int number)
    {
        for (var i = TMPChars.Length - 1; i >= 0; i--)
        {
            TMPChars[i] = (char)((number % 10) + ZeroChar);
            
            number /= 10;
            if (number == 0)
            {
                _startIdx = i;
                break;
            }
        }

        tmp.SetCharArray(TMPChars, _startIdx, TMPChars.Length - _startIdx);
    }

    #endregion
}