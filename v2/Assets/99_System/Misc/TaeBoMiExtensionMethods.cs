using System.Threading;
using TMPro;
using Unity.Mathematics;

public static class TaeBoMiExtensionMethods
{
    #region UniTask

    public static void CancelAndDispose(this CancellationTokenSource cts)
    {
        cts.Cancel();
        cts.Dispose();
    }

    #endregion
    
    #region TMP

    private static readonly char[] TMPChars = new char[5];
    private static int _startIdx;
    
    private const char ZeroChar = '0';

    public static void SetIntText(this TMP_Text tmp, int number)
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