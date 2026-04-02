using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using UnityEngine;

public class EmotionBubble : MonoBehaviour
{
    [SerializeField] private Animator ani;

    public Action ActionOnCloseFinished;

    private Emotion _curEmotion;

    private IObjectPool<EmotionBubble> _pool;

    private CancellationTokenSource _aniCts;

    private static readonly int OpenNameHash = Animator.StringToHash("Open");
    private static readonly int CloseNameHash = Animator.StringToHash("Close");


    public enum Emotion
    {
        Chest,
        Dialogue,
    }

    public void Initialize(IObjectPool<EmotionBubble> pool)
    {
        _pool = pool;
    }

    public async UniTaskVoid Set(Emotion emotion)
    {
        _aniCts?.CancelAndDispose();
        _aniCts = new CancellationTokenSource();
        _curEmotion = emotion;
        var aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        var curStateNameHash = aniStateInfo.shortNameHash;
        if (curStateNameHash == CloseNameHash)
        {
            ani.Play(OpenNameHash, 0, 1f - aniStateInfo.normalizedTime);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }

        while (1f > aniStateInfo.normalizedTime)
        {
            await UniTask.Yield(_aniCts.Token);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }

        ani.Play(_curEmotion.ToString());
    }

    public async UniTaskVoid Remove()
    {
        _aniCts?.CancelAndDispose();
        _aniCts = new CancellationTokenSource();
        var aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        var shortNameHash = aniStateInfo.shortNameHash;
        if (shortNameHash == OpenNameHash) // 열리던 중이면 해당 지점부터 닫기 재생
        {
            ani.Play(CloseNameHash, 0, 1f - aniStateInfo.normalizedTime);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }
        else if (shortNameHash != CloseNameHash) // 그 외엔 닫기 재생
        {
            ani.Play(CloseNameHash);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }

        while (aniStateInfo.shortNameHash != CloseNameHash) // 닫기 재생 시작 대기
        {
            await UniTask.Yield(_aniCts.Token);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }

        while (1f > aniStateInfo.normalizedTime) // 닫기 재생 완료 대기
        {
            await UniTask.Yield(_aniCts.Token);
            aniStateInfo = ani.GetCurrentAnimatorStateInfo(0);
        }

        ActionOnCloseFinished?.Invoke();
        _pool.Push(this);
    }
}